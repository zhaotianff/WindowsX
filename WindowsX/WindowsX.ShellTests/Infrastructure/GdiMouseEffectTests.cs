using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WindowsX.Shell.Infrastructure.MouseEffect;
using WindowsX.Shell.Windows;

namespace WindowsX.ShellTests.Infrastructure
{
    [TestClass]
    public class GdiMouseEffectTests
    {
        [DataTestMethod]
        [DataRow(MouseEffectType.FireworksGdiPlus, 1.0)]
        [DataRow(MouseEffectType.FireworksGdiPlus, 1.5)]
        [DataRow(MouseEffectType.WillowCatkinsGdiPlus, 1.0)]
        [DataRow(MouseEffectType.WillowCatkinsGdiPlus, 1.5)]
        public void GdiRenderingProducesTransparentPremultipliedPixels(MouseEffectType type, double scale)
        {
            RunSta(() =>
            {
                using var effect = new GdiParticleCursorEffect(type);
                var viewport = new Size(640, 480);
                var dpi = new DpiScale(scale, scale);
                effect.Update(new Point(200, 160), 0.016, viewport, dpi);
                effect.Update(new Point(200, 160), 0.05, viewport, dpi);
                Assert.IsTrue(effect.ParticleCount > 0);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                var drawing = GetImageDrawing(effect);
                var image = (BitmapSource)drawing.ImageSource;
                Assert.AreEqual(PixelFormats.Pbgra32, image.Format);
                Assert.AreEqual(96 * scale, image.DpiX, 0.001);
                Assert.AreEqual(96 * scale, image.DpiY, 0.001);
                Assert.IsTrue(drawing.Rect.Contains(new Point(200, 160)));
                var pixels = new byte[image.PixelWidth * image.PixelHeight * 4];
                image.CopyPixels(pixels, image.PixelWidth * 4, 0);
                int visible = 0, transparent = 0;
                for (var i = 0; i < pixels.Length; i += 4)
                {
                    var alpha = pixels[i + 3];
                    if (alpha == 0) transparent++;
                    else visible++;
                    Assert.IsTrue(pixels[i] <= alpha && pixels[i + 1] <= alpha && pixels[i + 2] <= alpha);
                }
                Assert.IsTrue(visible > 0, "GDI+ must draw visible pixels.");
                Assert.IsTrue(transparent > 0, "The background must stay transparent.");
            });
        }

        [DataTestMethod]
        [DataRow(MouseEffectType.FireworksGdiPlus)]
        [DataRow(MouseEffectType.WillowCatkinsGdiPlus)]
        public void ParticlesExpireAndCanRestartAfterClearing(MouseEffectType type)
        {
            RunSta(() =>
            {
                using var effect = new GdiParticleCursorEffect(type);
                var viewport = new Size(640, 480);
                var dpi = new DpiScale(1, 1);
                var cursor = new Point(200, 160);
                effect.Update(cursor, 0.016, viewport, dpi);
                effect.Update(cursor, 5, viewport, dpi);
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.IsTrue(IsEmpty(effect));
                effect.Update(new Point(220, 160), 0.016, viewport, dpi);
                Assert.IsTrue(effect.ParticleCount > 0);
                effect.Clear();
                Assert.IsTrue(IsEmpty(effect));
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(cursor, 0.016, viewport, dpi);
                Assert.IsTrue(effect.ParticleCount > 0);
            });
        }

        [DataTestMethod]
        [DataRow(MouseEffectType.FireworksGdiPlus)]
        [DataRow(MouseEffectType.WillowCatkinsGdiPlus)]
        public void MovementAndDpiChangesKeepRenderingBounded(MouseEffectType type)
        {
            RunSta(() =>
            {
                using var effect = new GdiParticleCursorEffect(type);
                for (var i = 0; i < 120; i++)
                {
                    var scale = i < 60 ? 1.0 : 1.5;
                    effect.Update(new Point(50 + i * 3, 160), 0.05, new Size(640, 480), new DpiScale(scale, scale));
                    Assert.IsTrue(effect.ParticleCount <= GdiParticleCursorEffect.MaxParticles);
                    Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                    var image = (BitmapSource)GetImageDrawing(effect).ImageSource;
                    Assert.AreEqual(96 * scale, image.DpiX, 0.001);
                }
                effect.Update(new Point(-100, -100), 5, new Size(640, 480), new DpiScale(1, 1));
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.IsTrue(IsEmpty(effect));
            });
        }

        [TestMethod]
        public void DisposeIsIdempotentAndRejectsFurtherUpdates()
        {
            RunSta(() =>
            {
                using var effect = new GdiParticleCursorEffect(MouseEffectType.FireworksGdiPlus);
                effect.Update(new Point(100, 100), 0.016, new Size(640, 480), new DpiScale(1, 1));
                effect.Dispose();
                effect.Dispose();
                Assert.IsTrue(effect.IsDisposed);
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.IsTrue(IsEmpty(effect));
                Assert.ThrowsException<ObjectDisposedException>(() => effect.Update(new Point(), 0.016, new Size(640, 480), new DpiScale(1, 1)));
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GdiParticleCursorEffect(MouseEffectType.Fireworks));
            });
        }

        [TestMethod]
        public void InvalidTimeAndEmptyViewportDoNotRetainParticles()
        {
            RunSta(() =>
            {
                using var effect = new GdiParticleCursorEffect(MouseEffectType.FireworksGdiPlus);
                foreach (var elapsed in new[] { 0.0, -1.0, double.NaN, double.PositiveInfinity })
                    effect.Update(new Point(100, 100), elapsed, new Size(640, 480), new DpiScale(1, 1));
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(new Point(100, 100), 0.016, new Size(640, 480), new DpiScale(1, 1));
                effect.Update(new Point(100, 100), 0.016, new Size(), new DpiScale(1, 1));
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.IsTrue(IsEmpty(effect));
            });
        }

        [DataTestMethod]
        [DataRow("switch")]
        [DataRow("stop")]
        [DataRow("close")]
        public void WindowReleasesGdiResources(string action)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                using var effect = new GdiParticleCursorEffect(MouseEffectType.FireworksGdiPlus);
                try
                {
                    window.UpdateMouseEffectType(MouseEffectType.FireworksGdiPlus);
                    effect.Update(new Point(100, 100), 0.016, new Size(640, 480), new DpiScale(1, 1));
                    var canvas = (Canvas)window.FindName("canvas");
                    canvas.Children.Add(effect);
                    // Inject the renderer without showing a desktop overlay or polling the real cursor.
                    var field = typeof(MouseEffectWindow).GetField("gdiParticleEffect", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    field.SetValue(window, effect);
                    if (action == "switch") window.UpdateMouseEffectType(MouseEffectType.WillowCatkins);
                    else if (action == "stop") window.StopMouseEffect();
                    else window.Close();
                    Assert.IsTrue(effect.IsDisposed);
                    Assert.IsNull(field.GetValue(window));
                    Assert.AreEqual(0, canvas.Children.Count);
                }
                finally { window.Close(); }
            });
        }

        private static ImageDrawing GetImageDrawing(GdiParticleCursorEffect effect)
        {
            var visual = (DrawingVisual)VisualTreeHelper.GetChild(effect, 0);
            return visual.Drawing.Children.OfType<ImageDrawing>().Single();
        }

        private static bool IsEmpty(GdiParticleCursorEffect effect)
        {
            var visual = (DrawingVisual)VisualTreeHelper.GetChild(effect, 0);
            return visual.Drawing == null || visual.Drawing.Bounds.IsEmpty;
        }

        private static void RunSta(Action action)
        {
            Exception? failure = null;
            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception ex) { failure = ex; }
                finally { System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown(); }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (failure != null)
                Assert.Fail(failure.ToString());
        }
    }
}
