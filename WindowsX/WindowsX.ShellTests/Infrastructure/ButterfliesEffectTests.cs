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
    public class ButterfliesEffectTests
    {
        [TestMethod]
        public void DefaultsAndSelectionAreCompatible()
        {
            Assert.AreEqual(21, (int)MouseEffectType.PixelBlocks);
            Assert.AreEqual(22, (int)MouseEffectType.Butterflies);
            foreach (var json in new[] { "{}", "{\"MouseEffectSetting\":null}" })
                Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.ButterfliesOpacity);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { ButterfliesOpacity = value }.ButterfliesOpacity);
        }

        [TestMethod]
        public void StationaryButterfliesOrbitFlapAndEmitOnWingCycles()
        {
            RunSta(() =>
            {
                var effect = new ButterfliesCursorEffect();
                var cursor = new Point(200, 200);
                effect.Update(cursor, 0.001);
                Assert.AreEqual(3, effect.ParticleCount);
                Assert.AreEqual(0, effect.SparkleCount);
                var before = Groups(Drawing(effect)).Where(g => g.Transform != null && !g.Transform.Value.IsIdentity).Select(g => g.Transform.Value).ToArray();
                effect.Update(cursor, 0.1);
                Assert.IsTrue(effect.SparkleCount > 0);
                var after = Groups(Drawing(effect)).Where(g => g.Transform != null && !g.Transform.Value.IsIdentity).Select(g => g.Transform.Value).ToArray();
                Assert.IsFalse(before.SequenceEqual(after));
                Assert.IsTrue(((double[])Field(effect, "wingPhases")).All(p => p >= 0 && p < 1));
                for (var i = 0; i < 2000; i++) effect.Update(cursor, 1.0 / 60);
                Assert.AreEqual(3, effect.ParticleCount);
                Assert.IsTrue(effect.SparkleCount > 0 && effect.SparkleCount <= ButterfliesCursorEffect.MaxSparkles);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                Assert.IsFalse(effect.IsHitTestVisible);
            });
        }

        [TestMethod]
        public void MovingCursorAcceleratesChaseThenBoostDecays()
        {
            RunSta(() =>
            {
                var effect = new ButterfliesCursorEffect();
                effect.Update(new Point(100, 100), 0.01);
                effect.Update(new Point(500, 100), 0.1);
                var anchors = (Point[])Field(effect, "anchors");
                for (var i = 0; i < anchors.Length; i++)
                {
                    var unboosted = 100 + 400 * (1 - Math.Exp(-(2 + i * 0.3) * 0.1));
                    Assert.IsTrue(anchors[i].X > unboosted && anchors[i].X < 500);
                }
                Assert.AreEqual(1.0, (double)Field(effect, "chaseBoost"));
                for (var i = 0; i < 300; i++) effect.Update(new Point(500, 100), 1.0 / 60);
                Assert.IsTrue((double)Field(effect, "chaseBoost") < 0.001);
                Assert.IsTrue(anchors.All(p => Math.Abs(p.X - 500) < 0.01));
            });
        }

        [TestMethod]
        public void SparklesFallAndFadeAndLongFramesDoNotBurst()
        {
            RunSta(() =>
            {
                var effect = new ButterfliesCursorEffect();
                var cursor = new Point(200, 200);
                for (var i = 0; i < 30; i++) effect.Update(cursor, 0.016);
                Assert.IsTrue(effect.SparkleCount > 0);
                var list = (System.Collections.IList)Field(effect, "sparkles");
                var particle = list[0]!;
                var type = particle.GetType();
                var draw = typeof(ButterfliesCursorEffect).GetMethod("DrawSparkle", BindingFlags.NonPublic | BindingFlags.Static)!;
                DrawingGroup Render(double age)
                {
                    type.GetField("Age")!.SetValue(particle, age);
                    var drawing = new DrawingGroup();
                    using (var context = drawing.Open()) draw.Invoke(null, new object[] { context, particle });
                    return drawing;
                }
                var early = Render(0.1);
                var late = Render(0.8);
                var earlyTransform = Groups(early).First(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value;
                var lateTransform = Groups(late).First(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value;
                Assert.IsTrue(lateTransform.OffsetY > earlyTransform.OffsetY);
                Assert.IsTrue(Groups(late).Min(g => g.Opacity) < Groups(early).Min(g => g.Opacity));
                effect.Update(cursor, 10);
                Assert.AreEqual(0, effect.SparkleCount);
                Assert.AreEqual(3, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void InvalidInputClearAndPreviewAreSafe()
        {
            RunSta(() =>
            {
                var effect = new ButterfliesCursorEffect();
                foreach (var elapsed in new[] { 0.0, -1, double.NaN, double.PositiveInfinity }) effect.Update(new Point(100, 100), elapsed);
                effect.Update(new Point(double.NaN, 100), 0.01);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(new Point(100, 100), 0.01);
                effect.Update(new Point(100, 100), double.MaxValue);
                var visual = VisualTreeHelper.GetChild(effect, 0);
                effect.Clear();
                effect.Clear();
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.AreEqual(0, effect.SparkleCount);
                Assert.IsTrue(Drawing(effect) == null || Drawing(effect).Bounds.IsEmpty);
                effect.Update(new Point(100, 100), 0.01);
                Assert.AreEqual(3, effect.ParticleCount);
                Assert.AreSame(visual, VisualTreeHelper.GetChild(effect, 0));
                var preview = ButterfliesCursorEffect.PreviewImage;
                Assert.IsTrue(preview.IsFrozen);
                Assert.AreEqual(new Rect(0, 0, 260, 170), preview.Drawing.Bounds);
            });
        }

        [TestMethod]
        public void LiveOpacityHidesButterfliesAndSparkles()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new ButterfliesCursorEffect(settings);
                effect.Measure(new Size(400, 400));
                effect.Arrange(new Rect(0, 0, 400, 400));
                for (var i = 0; i < 30; i++) effect.Update(new Point(200, 200), 0.016);
                Assert.IsTrue(effect.SparkleCount > 0);
                Assert.IsTrue(Alpha(effect) > 0);
                settings.ButterfliesOpacity = 0;
                effect.Update(new Point(200, 200), 0.016);
                Assert.AreEqual(0L, Alpha(effect));
                settings.ButterfliesOpacity = 0.5;
                effect.Update(new Point(200, 200), 0.016);
                Assert.IsTrue(Alpha(effect) > 0);
            });
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void WindowUsesSharedEffectAndClearsIt(bool switchEffect)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                try
                {
                    window.StartMouseEffect();
                    window.UpdateMouseEffectType(MouseEffectType.Butterflies);
                    var create = typeof(MouseEffectWindow).GetMethod("CreateEffects", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    create.Invoke(window, null);
                    create.Invoke(window, null);
                    var canvas = (Canvas)window.FindName("canvas");
                    Assert.AreEqual(1, canvas.Children.Count);
                    var effect = (ButterfliesCursorEffect)canvas.Children[0];
                    effect.Update(new Point(100, 100), 0.1);
                    if (switchEffect) window.UpdateMouseEffectType(MouseEffectType.Fireflies);
                    else window.StopMouseEffect();
                    Assert.AreEqual(0, effect.ParticleCount);
                    Assert.AreEqual(0, effect.SparkleCount);
                    Assert.AreEqual(0, canvas.Children.Count);
                }
                finally { window.StopMouseEffect(); window.Close(); }
            });
        }

        [TestMethod]
        public void DialogResetAndCancelPreserveSettings()
        {
            RunSta(() =>
            {
                var config = new MainConfig();
                config.MouseEffectSetting.ButterfliesOpacity = 0.6;
                var window = new MouseEffectSettingWindow(MouseEffectType.Butterflies, config);
                try
                {
                    Assert.AreEqual("蝴蝶跟随设置", window.Title);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(60.0, slider.Value);
                    Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName("panel_PixelBlocksSize")).Visibility);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    slider.Value = 20;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.6, config.MouseEffectSetting.ButterfliesOpacity);
            });
        }

        [TestMethod]
        public void DialogPersistsOnlyButterfliesAndKeepsSelection()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-Butterflies-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.WillowCatkinsOpacity = 0.4;
                    settings.PixelBlocksSizeScale = 2;
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)MouseEffectType.Butterflies;
                    var window = new MouseEffectSettingWindow(MouseEffectType.Butterflies, config);
                    Exception? failure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            ((Slider)window.FindName("slider_Opacity")).Value = 25;
                            ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        catch (Exception ex) { failure = ex; }
                        finally { window.Close(); }
                    }, DispatcherPriority.Loaded);
                    Assert.AreEqual(true, window.ShowDialog());
                    if (failure != null) Assert.Fail(failure.ToString());
                    Assert.AreSame(settings, config.MouseEffectSetting);
                    var restored = ConfigHelper.LoadMainConfig().MouseEffectSetting;
                    Assert.AreEqual(0.25, restored.ButterfliesOpacity);
                    Assert.AreEqual(0.4, restored.WillowCatkinsOpacity);
                    Assert.AreEqual(2.0, restored.PixelBlocksSizeScale);
                    Assert.AreEqual(22, restored.SelectedEffectType);
                    Assert.IsTrue(restored.IsEnabled);
                }
                finally
                {
                    Environment.CurrentDirectory = original;
                    System.IO.Directory.Delete(directory, true);
                }
            });
        }

        [TestMethod]
        public void ThreeAnimeButterfliesUseFrozenOutlinesHighlightsAndMatchingPreview()
        {
            RunSta(() =>
            {
                Assert.AreEqual(3, ButterfliesCursorEffect.ButterflyCount);
                var effect = new ButterfliesCursorEffect();
                effect.Update(new Point(200, 200), 0.001);
                var shapes = Groups(Drawing(effect)).SelectMany(g => g.Children.OfType<GeometryDrawing>()).ToArray();
                var wings = shapes.Where(s => s.Geometry is StreamGeometry && s.Brush is LinearGradientBrush && s.Pen != null).ToArray();
                Assert.AreEqual(6, wings.Length);
                Assert.IsTrue(wings.All(s => s.Pen.IsFrozen && s.Brush.IsFrozen && s.Geometry.IsFrozen));
                Assert.AreEqual(3, wings.Select(s => s.Brush).Distinct().Count());
                Assert.IsTrue(wings.All(s => ((LinearGradientBrush)s.Brush).GradientStops.Count == 5));
                Assert.IsTrue(shapes.Count(s => s.Geometry is EllipseGeometry) >= 3 * 10);
                var previewShapes = Groups(ButterfliesCursorEffect.PreviewImage.Drawing).SelectMany(g => g.Children.OfType<GeometryDrawing>());
                Assert.AreEqual(6, previewShapes.Count(s => s.Geometry is StreamGeometry && s.Brush is LinearGradientBrush && s.Pen != null));
                Assert.AreEqual(3, ((Point[])Field(effect, "anchors")).Length);
            });
        }

        private static object Field(ButterfliesCursorEffect effect, string name) => typeof(ButterfliesCursorEffect).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(effect)!;
        private static DrawingGroup Drawing(ButterfliesCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
        private static IEnumerable<DrawingGroup> Groups(Drawing drawing)
        {
            if (drawing is DrawingGroup group)
            {
                yield return group;
                foreach (var child in group.Children)
                    foreach (var nested in Groups(child)) yield return nested;
            }
        }
        private static long Alpha(ButterfliesCursorEffect effect)
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
