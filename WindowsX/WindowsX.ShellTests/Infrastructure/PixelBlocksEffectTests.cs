using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WindowsX.Config.Helper;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect;
using WindowsX.Shell.Windows;

namespace WindowsX.ShellTests.Infrastructure
{
    [TestClass]
    [DoNotParallelize]
    public class PixelBlocksEffectTests
    {
        [TestMethod]
        public void DefaultsAndEnumRemainCompatible()
        {
            Assert.AreEqual(20, (int)MouseEffectType.Fireflies);
            Assert.AreEqual(21, (int)MouseEffectType.PixelBlocks);
            foreach (var json in new[] { "{}", "{\"MouseEffectSetting\":null}", "{\"MouseEffectSetting\":{}}" })
            {
                Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.PixelBlocksOpacity);
                Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.PixelBlocksSizeScale);
            }
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(0.4, 0.4)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { PixelBlocksOpacity = value }.PixelBlocksOpacity);
        }

        [TestMethod]
        public void BlocksHaveCrispSquareGeometryAndMultipleColors()
        {
            RunSta(() =>
            {
                var effect = new PixelBlocksCursorEffect();
                effect.Update(new Point(100, 100), 0.016);
                effect.Update(new Point(180, 100), 0.03);
                var shapes = Shapes(Drawing(effect)).ToArray();
                Assert.AreEqual(11, shapes.Length);
                Assert.IsTrue(shapes.All(s => s.Geometry is RectangleGeometry && s.Brush is SolidColorBrush && s.Brush.IsFrozen && s.Pen == null));
                foreach (var shape in shapes)
                {
                    var square = (RectangleGeometry)shape.Geometry;
                    Assert.AreEqual(0.0, square.RadiusX);
                    Assert.AreEqual(0.0, square.RadiusY);
                    Assert.AreEqual(square.Rect.Width, square.Rect.Height);
                    Assert.AreEqual(0.0, square.Rect.Width % 2);
                    Assert.AreEqual(0.0, square.Rect.X % 2);
                    Assert.AreEqual(0.0, square.Rect.Y % 2);
                }
                Assert.AreEqual(5, shapes.Select(s => ((SolidColorBrush)s.Brush).Color).Distinct().Count());
                Assert.AreEqual(EdgeMode.Aliased, RenderOptions.GetEdgeMode(VisualTreeHelper.GetChild(effect, 0)));
                Assert.IsFalse(effect.IsHitTestVisible);
                var preview = PixelBlocksCursorEffect.PreviewImage;
                Assert.IsTrue(preview.IsFrozen);
                Assert.AreEqual(new Rect(0, 0, 260, 170), preview.Drawing.Bounds);
                Assert.AreEqual(21, Shapes(preview.Drawing).Count());
            });
        }

        [TestMethod]
        public void BlocksShrinkFadeFallAndExpireWithoutIdleEmission()
        {
            RunSta(() =>
            {
                var effect = new PixelBlocksCursorEffect();
                var cursor = new Point(100, 100);
                effect.Update(cursor, 0.01);
                var initial = ((RectangleGeometry)Shapes(Drawing(effect)).Single().Geometry).Rect;
                effect.Update(cursor, 0.65);
                Assert.AreEqual(1, effect.ParticleCount);
                var later = ((RectangleGeometry)Shapes(Drawing(effect)).Single().Geometry).Rect;
                Assert.IsTrue(later.Width <= initial.Width);
                Assert.IsTrue(Groups(Drawing(effect)).Any(g => g.Opacity > 0 && g.Opacity < 1));
                effect.Update(cursor, 2);
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.AreEqual(0, Shapes(Drawing(effect)).Count());
                effect.Update(new Point(300, 100), 0.01);
                Assert.AreEqual(1, effect.ParticleCount);
                var resumed = ((RectangleGeometry)Shapes(Drawing(effect)).Single().Geometry).Rect;
                Assert.AreEqual(300.0, resumed.X + resumed.Width / 2, 1);
            });
        }

        [TestMethod]
        public void FastMovementStaysBoundedAndClearResetsEmission()
        {
            RunSta(() =>
            {
                var effect = new PixelBlocksCursorEffect();
                effect.Update(new Point(100, 100), 0.01);
                effect.Update(new Point(10000, 100), 0.03);
                Assert.AreEqual(1 + PixelBlocksCursorEffect.MaxEmissionPerUpdate, effect.ParticleCount);
                for (var i = 0; i < 1000; i++) effect.Update(new Point(i % 2 * 10000, 100), 0.03);
                Assert.IsTrue(effect.ParticleCount <= PixelBlocksCursorEffect.MaxParticles);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                var visual = VisualTreeHelper.GetChild(effect, 0);
                effect.Clear();
                effect.Clear();
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.IsTrue(Drawing(effect) == null || Drawing(effect).Bounds.IsEmpty);
                effect.Update(new Point(100, 100), 0.01);
                Assert.AreEqual(1, effect.ParticleCount);
                Assert.AreSame(visual, VisualTreeHelper.GetChild(effect, 0));
            });
        }

        [TestMethod]
        public void InvalidInputAndLongFramesAreSafe()
        {
            RunSta(() =>
            {
                var effect = new PixelBlocksCursorEffect();
                foreach (var elapsed in new[] { 0.0, -1, double.NaN, double.PositiveInfinity }) effect.Update(new Point(100, 100), elapsed);
                effect.Update(new Point(double.NaN, 100), 0.01);
                effect.Update(new Point(100, double.PositiveInfinity), 0.01);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(new Point(100, 100), 0.01);
                effect.Update(new Point(100, 100), double.MaxValue);
                Assert.AreEqual(0, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void OpacityUpdatesLive()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new PixelBlocksCursorEffect(settings);
                effect.Measure(new Size(400, 400));
                effect.Arrange(new Rect(0, 0, 400, 400));
                effect.Update(new Point(200, 200), 0.01);
                Assert.IsTrue(Alpha(effect) > 0);
                settings.PixelBlocksOpacity = 0;
                effect.Update(new Point(200, 200), 0.01);
                Assert.AreEqual(0L, Alpha(effect));
                settings.PixelBlocksOpacity = 0.5;
                effect.Update(new Point(200, 200), 0.01);
                Assert.IsTrue(Alpha(effect) > 0);
                Assert.AreEqual(1.0, settings.FirefliesOpacity);
            });
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void WindowUsesSharedEffectAndClearsOnStopOrSwitch(bool switchEffect)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                try
                {
                    window.StartMouseEffect();
                    window.UpdateMouseEffectType(MouseEffectType.PixelBlocks);
                    var create = typeof(MouseEffectWindow).GetMethod("CreateEffects", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    create.Invoke(window, null);
                    create.Invoke(window, null);
                    var canvas = (Canvas)window.FindName("canvas");
                    Assert.AreEqual(1, canvas.Children.Count);
                    var effect = (PixelBlocksCursorEffect)canvas.Children[0];
                    effect.Update(new Point(100, 100), 0.01);
                    if (switchEffect) window.UpdateMouseEffectType(MouseEffectType.Fireflies);
                    else window.StopMouseEffect();
                    Assert.AreEqual(0, effect.ParticleCount);
                    Assert.AreEqual(0, canvas.Children.Count);
                }
                finally { window.StopMouseEffect(); window.Close(); }
            });
        }

        [TestMethod]
        public void DialogResetAndCancelLeaveSharedSettingsUnchanged()
        {
            RunSta(() =>
            {
                var config = new MainConfig();
                config.MouseEffectSetting.PixelBlocksOpacity = 0.6;
                config.MouseEffectSetting.PixelBlocksSizeScale = 2;
                var window = new MouseEffectSettingWindow(MouseEffectType.PixelBlocks, config);
                try
                {
                    Assert.AreEqual("像素方块设置", window.Title);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(60.0, slider.Value);
                    var size = (Slider)window.FindName("slider_PixelBlocksSize");
                    Assert.AreEqual(200.0, size.Value);
                    Assert.AreEqual(50.0, size.Minimum);
                    Assert.AreEqual(300.0, size.Maximum);
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_PixelBlocksSize")).Visibility);
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_ParticleOpacity")).Visibility);
                    Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName("panel_CottonCandySize")).Visibility);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    Assert.AreEqual(100.0, size.Value);
                    size.Value = 300;
                    slider.Value = 20;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.6, config.MouseEffectSetting.PixelBlocksOpacity);
                Assert.AreEqual(2.0, config.MouseEffectSetting.PixelBlocksSizeScale);
            });
        }

        [TestMethod]
        public void DialogPersistsOnlyPixelOpacityAndKeepsSelection()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-PixelBlocks-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.FirefliesOpacity = 0.4;
                    settings.WillowCatkinsOpacity = 0.7;
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)MouseEffectType.PixelBlocks;
                    var window = new MouseEffectSettingWindow(MouseEffectType.PixelBlocks, config);
                    Exception? failure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            ((Slider)window.FindName("slider_Opacity")).Value = 25;
                            ((Slider)window.FindName("slider_PixelBlocksSize")).Value = 250;
                            ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        catch (Exception ex) { failure = ex; }
                        finally { window.Close(); }
                    }, DispatcherPriority.Loaded);
                    Assert.AreEqual(true, window.ShowDialog());
                    if (failure != null) Assert.Fail(failure.ToString());
                    Assert.AreSame(settings, config.MouseEffectSetting);
                    var restored = ConfigHelper.LoadMainConfig().MouseEffectSetting;
                    Assert.AreEqual(0.25, restored.PixelBlocksOpacity);
                    Assert.AreEqual(2.5, restored.PixelBlocksSizeScale);
                    Assert.AreEqual(2.5, settings.PixelBlocksSizeScale);
                    Assert.AreEqual(0.4, restored.FirefliesOpacity);
                    Assert.AreEqual(0.7, restored.WillowCatkinsOpacity);
                    Assert.AreEqual(21, restored.SelectedEffectType);
                    Assert.IsTrue(restored.IsEnabled);
                }
                finally
                {
                    Environment.CurrentDirectory = original;
                    System.IO.Directory.Delete(directory, true);
                }
            });
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.5)]
        [DataRow(10.0, 3.0)]
        [DataRow(1.75, 1.75)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void SizeIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { PixelBlocksSizeScale = value }.PixelBlocksSizeScale);
        }

        [DataTestMethod]
        [DataRow(0.5)]
        [DataRow(1.75)]
        [DataRow(3.0)]
        public void LiveSizeScalesSquaresWithoutChangingCentersOrBaseSizes(double scale)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new PixelBlocksCursorEffect(settings);
                effect.Update(new Point(100, 100), 0.01);
                effect.Update(new Point(180, 100), 0.03);
                var field = typeof(PixelBlocksCursorEffect).GetField("particles", BindingFlags.NonPublic | BindingFlags.Instance)!;
                var particles = (System.Collections.IEnumerable)field.GetValue(effect)!;
                var sizes = particles.Cast<object>().Select(p => (double)p.GetType().GetField("Size")!.GetValue(p)!).ToArray();
                var visual = VisualTreeHelper.GetChild(effect, 0);
                settings.PixelBlocksSizeScale = scale;
                effect.Update(new Point(180, 100), 0.01);
                var reference = new DrawingGroup();
                var draw = typeof(PixelBlocksCursorEffect).GetMethod("DrawBlock", BindingFlags.NonPublic | BindingFlags.Static)!;
                using (var context = reference.Open())
                    foreach (var particle in particles)
                        draw.Invoke(null, new object[] { context, particle, 1.0 });
                var original = Shapes(reference).ToArray();
                var scaled = Shapes(Drawing(effect)).ToArray();
                Assert.AreEqual(original.Length, scaled.Length);
                for (var i = 0; i < original.Length; i++)
                {
                    var a = ((RectangleGeometry)original[i].Geometry).Rect;
                    var b = ((RectangleGeometry)scaled[i].Geometry).Rect;
                    Assert.AreEqual(a.Width * scale, b.Width, 0.000001);
                    Assert.AreEqual(a.Height * scale, b.Height, 0.000001);
                    Assert.AreEqual(a.X + a.Width / 2, b.X + b.Width / 2, 0.000001);
                    Assert.AreEqual(a.Y + a.Height / 2, b.Y + b.Height / 2, 0.000001);
                    Assert.AreSame(original[i].Brush, scaled[i].Brush);
                }
                CollectionAssert.AreEqual(sizes, particles.Cast<object>().Select(p => (double)p.GetType().GetField("Size")!.GetValue(p)!).ToArray());
                Assert.AreSame(visual, VisualTreeHelper.GetChild(effect, 0));
            });
        }

        [TestMethod]
        public void OtherEffectsHidePixelSizePanel()
        {
            RunSta(() =>
            {
                var window = new MouseEffectSettingWindow(MouseEffectType.Fireflies, new MainConfig());
                try
                {
                    Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName("panel_PixelBlocksSize")).Visibility);
                }
                finally { window.Close(); }
            });
        }

        private static DrawingGroup Drawing(PixelBlocksCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
        private static IEnumerable<DrawingGroup> Groups(Drawing drawing)
        {
            if (drawing is DrawingGroup group)
            {
                yield return group;
                foreach (var child in group.Children)
                    foreach (var nested in Groups(child)) yield return nested;
            }
        }
        private static IEnumerable<GeometryDrawing> Shapes(Drawing drawing)
        {
            if (drawing is GeometryDrawing shape) yield return shape;
            else if (drawing is DrawingGroup group)
                foreach (var child in group.Children)
                    foreach (var item in Shapes(child)) yield return item;
        }
        private static long Alpha(PixelBlocksCursorEffect effect)
        {
            var bitmap = new RenderTargetBitmap(400, 400, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(effect);
            var pixels = new byte[400 * 400 * 4];
            bitmap.CopyPixels(pixels, 400 * 4, 0);
            long sum = 0;
            for (var i = 3; i < pixels.Length; i += 4) sum += pixels[i];
            return sum;
        }
        private static void RunSta(Action action)
        {
            Exception? failure = null;
            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception ex) { failure = ex; }
                finally { Dispatcher.CurrentDispatcher.InvokeShutdown(); }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (failure != null) Assert.Fail(failure.ToString());
        }
    }
}
