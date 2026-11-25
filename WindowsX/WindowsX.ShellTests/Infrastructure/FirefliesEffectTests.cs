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
    public class FirefliesEffectTests
    {
        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        public void DefaultsPreserveExistingTypes(string json)
        {
            Assert.AreEqual(19, (int)MouseEffectType.CottonCandy);
            Assert.AreEqual(20, (int)MouseEffectType.Fireflies);
            Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.FirefliesOpacity);
            Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.FirefliesFollowSpeed);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { FirefliesOpacity = value }.FirefliesOpacity);
        }

        [TestMethod]
        public void SixSoftLightsWanderAndBreatheWhenStationary()
        {
            RunSta(() =>
            {
                var effect = new FirefliesCursorEffect();
                var point = new Point(200, 200);
                effect.Update(point, 0.016);
                var before = Shapes(Drawing(effect)).ToArray();
                Assert.AreEqual(12, before.Length);
                Assert.AreEqual(6, effect.ParticleCount);
                Assert.IsTrue(before.All(s => s.Geometry is EllipseGeometry && s.Brush.IsFrozen && s.Pen == null));
                var halos = before.Where(s => s.Brush is RadialGradientBrush).ToArray();
                Assert.AreEqual(6, halos.Length);
                Assert.IsTrue(halos.All(s => ((RadialGradientBrush)s.Brush).GradientStops.Last().Color.A == 0));
                var brightness = Lights(Drawing(effect)).Select(g => g.Opacity).ToArray();
                Assert.AreEqual(6, brightness.Length);
                Assert.IsTrue(brightness.All(a => a >= 0.15 && a <= 1));
                Assert.IsTrue(brightness.Distinct().Count() > 1);
                effect.Update(point, 0.7);
                var after = Shapes(Drawing(effect)).ToArray();
                Assert.AreNotEqual(((EllipseGeometry)before[0].Geometry).Center, ((EllipseGeometry)after[0].Geometry).Center);
                Assert.IsFalse(brightness.SequenceEqual(Lights(Drawing(effect)).Select(g => g.Opacity)));
                Assert.IsFalse(effect.IsHitTestVisible);
                Assert.IsTrue(FirefliesCursorEffect.PreviewImage.IsFrozen);
                Assert.AreEqual(new Rect(0, 0, 260, 170), FirefliesCursorEffect.PreviewImage.Drawing.Bounds);
            });
        }

        [TestMethod]
        public void LightsLagBehindMovementWithDifferentSpeeds()
        {
            RunSta(() =>
            {
                var effect = new FirefliesCursorEffect();
                effect.Update(new Point(100, 100), 0.016);
                effect.Update(new Point(500, 100), 0.1);
                var centers = Shapes(Drawing(effect)).Where(s => s.Brush is RadialGradientBrush).Select(s => ((EllipseGeometry)s.Geometry).Center).ToArray();
                Assert.IsTrue(centers.All(p => p.X > 100 && p.X < 300));
                var anchors = (Point[])typeof(FirefliesCursorEffect).GetField("anchors", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(effect)!;
                Assert.IsTrue(anchors[0].X < anchors[5].X);
                effect.Update(new Point(500, 100), 10);
                Assert.IsTrue(anchors.All(p => Math.Abs(p.X - 500) < 0.01));
            });
        }

        [DataTestMethod]
        [DataRow(0.25)]
        [DataRow(1.0)]
        [DataRow(3.0)]
        public void FollowingAndBreathingAreIndependentOfFrameInterval(double speed)
        {
            RunSta(() =>
            {
                var first = new FirefliesCursorEffect(new MouseEffectSetting { FirefliesFollowSpeed = speed });
                var second = new FirefliesCursorEffect(new MouseEffectSetting { FirefliesFollowSpeed = speed });
                first.Update(new Point(100, 100), 0.01);
                second.Update(new Point(100, 100), 0.01);
                first.Update(new Point(300, 200), 0.2);
                for (var i = 0; i < 20; i++) second.Update(new Point(300, 200), 0.01);
                var a = Shapes(Drawing(first)).ToArray();
                var b = Shapes(Drawing(second)).ToArray();
                for (var i = 0; i < a.Length; i++)
                {
                    Assert.AreEqual(((EllipseGeometry)a[i].Geometry).Center.X, ((EllipseGeometry)b[i].Geometry).Center.X, 0.000001);
                    Assert.AreEqual(((EllipseGeometry)a[i].Geometry).Center.Y, ((EllipseGeometry)b[i].Geometry).Center.Y, 0.000001);
                }
                var opacityA = Lights(Drawing(first)).Select(g => g.Opacity).ToArray();
                var opacityB = Lights(Drawing(second)).Select(g => g.Opacity).ToArray();
                for (var i = 0; i < opacityA.Length; i++) Assert.AreEqual(opacityA[i], opacityB[i], 0.000001);
            });
        }

        [TestMethod]
        public void InvalidInputLongFramesAndClearKeepStateBounded()
        {
            RunSta(() =>
            {
                var effect = new FirefliesCursorEffect();
                foreach (var elapsed in new[] { 0.0, -1, double.NaN, double.PositiveInfinity }) effect.Update(new Point(100, 100), elapsed);
                effect.Update(new Point(double.NaN, 100), 0.01);
                effect.Update(new Point(100, double.PositiveInfinity), 0.01);
                Assert.AreEqual(0, effect.ParticleCount);
                for (var i = 0; i < 1000; i++) effect.Update(new Point(i, 100), 0.016);
                effect.Update(new Point(200, 200), double.MaxValue);
                Assert.AreEqual(6, effect.ParticleCount);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                Assert.AreEqual(12, Shapes(Drawing(effect)).Count());
                Assert.IsTrue(Shapes(Drawing(effect)).All(s => double.IsFinite(((EllipseGeometry)s.Geometry).Center.X)));
                var visual = VisualTreeHelper.GetChild(effect, 0);
                effect.Clear();
                effect.Clear();
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.IsTrue(Drawing(effect) == null || Drawing(effect).Bounds.IsEmpty);
                effect.Update(new Point(200, 200), 0.01);
                Assert.AreEqual(6, effect.ParticleCount);
                Assert.AreSame(visual, VisualTreeHelper.GetChild(effect, 0));
            });
        }

        [TestMethod]
        public void OpacityUpdatesLiveWithoutChangingOtherEffects()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new FirefliesCursorEffect(settings);
                effect.Measure(new Size(400, 400));
                effect.Arrange(new Rect(0, 0, 400, 400));
                effect.Update(new Point(200, 200), 0.1);
                Assert.IsTrue(Alpha(effect) > 0);
                settings.FirefliesOpacity = 0;
                effect.Update(new Point(200, 200), 0.1);
                Assert.AreEqual(0L, Alpha(effect));
                settings.FirefliesOpacity = 0.5;
                effect.Update(new Point(200, 200), 0.1);
                Assert.IsTrue(Alpha(effect) > 0);
                Assert.AreEqual(1.0, settings.CottonCandyOpacity);
            });
        }

        [DataTestMethod]
        [DataRow("switch")]
        [DataRow("stop")]
        [DataRow("restart")]
        public void WindowCreatesOneSharedEffectAndClearsIt(string action)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                try
                {
                    window.StartMouseEffect();
                    window.UpdateMouseEffectType(MouseEffectType.Fireflies);
                    var create = typeof(MouseEffectWindow).GetMethod("CreateEffects", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    create.Invoke(window, null);
                    create.Invoke(window, null);
                    var canvas = (Canvas)window.FindName("canvas");
                    Assert.AreEqual(1, canvas.Children.Count);
                    var effect = (FirefliesCursorEffect)canvas.Children[0];
                    effect.Update(new Point(100, 100), 0.01);
                    if (action == "switch") window.UpdateMouseEffectType(MouseEffectType.FollowingDot);
                    else window.StopMouseEffect();
                    if (action == "restart") window.StartMouseEffect();
                    Assert.AreEqual(0, effect.ParticleCount);
                    Assert.AreEqual(0, canvas.Children.Count);
                }
                finally { window.StopMouseEffect(); window.Close(); }
            });
        }

        [TestMethod]
        public void DialogResetsAndDiscardsDraft()
        {
            RunSta(() =>
            {
                var config = new MainConfig();
                config.MouseEffectSetting.FirefliesOpacity = 0.6;
                config.MouseEffectSetting.FirefliesFollowSpeed = 2;
                var window = new MouseEffectSettingWindow(MouseEffectType.Fireflies, config);
                try
                {
                    Assert.AreEqual("萤火虫设置", window.Title);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(60.0, slider.Value);
                    var speed = (Slider)window.FindName("slider_FirefliesFollowSpeed");
                    Assert.AreEqual(200.0, speed.Value);
                    Assert.AreEqual(25.0, speed.Minimum);
                    Assert.AreEqual(300.0, speed.Maximum);
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_FirefliesFollowSpeed")).Visibility);
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_ParticleOpacity")).Visibility);
                    Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName("panel_CottonCandySize")).Visibility);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    Assert.AreEqual(100.0, speed.Value);
                    speed.Value = 25;
                    slider.Value = 25;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.6, config.MouseEffectSetting.FirefliesOpacity);
                Assert.AreEqual(2.0, config.MouseEffectSetting.FirefliesFollowSpeed);
            });
        }

        [TestMethod]
        public void DialogPersistsOpacityAndSelectionWithoutChangingOtherEffects()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-Fireflies-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.WillowCatkinsOpacity = 0.4;
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)MouseEffectType.Fireflies;
                    var window = new MouseEffectSettingWindow(MouseEffectType.Fireflies, config);
                    Exception? failure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            ((Slider)window.FindName("slider_Opacity")).Value = 25;
                            ((Slider)window.FindName("slider_FirefliesFollowSpeed")).Value = 250;
                            ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        catch (Exception ex) { failure = ex; }
                        finally { window.Close(); }
                    }, DispatcherPriority.Loaded);
                    Assert.AreEqual(true, window.ShowDialog());
                    if (failure != null) Assert.Fail(failure.ToString());
                    Assert.AreSame(settings, config.MouseEffectSetting);
                    Assert.AreEqual(0.25, settings.FirefliesOpacity);
                    Assert.AreEqual(2.5, settings.FirefliesFollowSpeed);
                    var restored = ConfigHelper.LoadMainConfig().MouseEffectSetting;
                    Assert.AreEqual(0.25, restored.FirefliesOpacity);
                    Assert.AreEqual(2.5, restored.FirefliesFollowSpeed);
                    Assert.AreEqual(0.4, restored.WillowCatkinsOpacity);
                    Assert.AreEqual(20, restored.SelectedEffectType);
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
        [DataRow(-1.0, 0.25)]
        [DataRow(10.0, 3.0)]
        [DataRow(1.75, 1.75)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void FollowSpeedIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { FirefliesFollowSpeed = value }.FirefliesFollowSpeed);
        }

        [DataTestMethod]
        [DataRow(0.25)]
        [DataRow(3.0)]
        public void FollowSpeedChangesLiveWithoutChangingWanderOrBrightness(double speed)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new FirefliesCursorEffect(settings);
                var reference = new FirefliesCursorEffect();
                effect.Update(new Point(100, 100), 0.01);
                reference.Update(new Point(100, 100), 0.01);
                var visual = VisualTreeHelper.GetChild(effect, 0);
                settings.FirefliesFollowSpeed = speed;
                effect.Update(new Point(500, 100), 0.1);
                reference.Update(new Point(500, 100), 0.1);
                var field = typeof(FirefliesCursorEffect).GetField("anchors", BindingFlags.NonPublic | BindingFlags.Instance)!;
                var anchors = (Point[])field.GetValue(effect)!;
                var original = (Point[])field.GetValue(reference)!;
                var shapes = Shapes(Drawing(effect)).ToArray();
                var referenceShapes = Shapes(Drawing(reference)).ToArray();
                for (var i = 0; i < anchors.Length; i++)
                {
                    Assert.AreEqual(100 + 400 * (1 - Math.Exp(-0.1 * (1.2 + i * 0.22) * speed)), anchors[i].X, 0.000001);
                    Assert.IsTrue(speed < 1 ? anchors[i].X < original[i].X : anchors[i].X > original[i].X);
                    var offset = ((EllipseGeometry)shapes[i * 2].Geometry).Center - anchors[i];
                    var referenceOffset = ((EllipseGeometry)referenceShapes[i * 2].Geometry).Center - original[i];
                    Assert.AreEqual(referenceOffset.X, offset.X, 0.000001);
                    Assert.AreEqual(referenceOffset.Y, offset.Y, 0.000001);
                }
                CollectionAssert.AreEqual(Lights(Drawing(reference)).Select(g => g.Opacity).ToArray(), Lights(Drawing(effect)).Select(g => g.Opacity).ToArray());
                Assert.AreSame(visual, VisualTreeHelper.GetChild(effect, 0));
                Assert.AreEqual(6, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void OtherEffectsHideFollowSpeedPanel()
        {
            RunSta(() =>
            {
                var window = new MouseEffectSettingWindow(MouseEffectType.PixelBlocks, new MainConfig());
                try
                {
                    Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName("panel_FirefliesFollowSpeed")).Visibility);
                }
                finally { window.Close(); }
            });
        }

        private static DrawingGroup Drawing(FirefliesCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
        private static IEnumerable<DrawingGroup> Lights(Drawing drawing)
        {
            if (drawing is DrawingGroup group)
            {
                if (group.Children.OfType<GeometryDrawing>().Any()) yield return group;
                foreach (var child in group.Children)
                    foreach (var light in Lights(child)) yield return light;
            }
        }
        private static IEnumerable<GeometryDrawing> Shapes(Drawing drawing)
        {
            if (drawing is GeometryDrawing shape) yield return shape;
            else if (drawing is DrawingGroup group)
                foreach (var child in group.Children)
                    foreach (var item in Shapes(child)) yield return item;
        }
        private static long Alpha(FirefliesCursorEffect effect)
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
