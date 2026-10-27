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
    public class StardustEffectTests
    {
        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{\"InkFluidOpacity\":0.4}}")]
        public void OldConfigurationDefaultsAndTypesRemainCompatible(string json)
        {
            Assert.AreEqual(10, (int)MouseEffectType.WaterRipple);
            Assert.AreEqual(11, (int)MouseEffectType.Stardust);
            Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.StardustOpacity);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { StardustOpacity = value }.StardustOpacity);
        }

        [TestMethod]
        public void StarsHaveSoftHalosAndAreDistributedAlongMovement()
        {
            RunSta(() =>
            {
                var effect = new StardustCursorEffect();
                effect.Update(new Point(100, 200), 0.016);
                Assert.AreEqual(3, effect.ParticleCount);
                effect.Update(new Point(260, 200), 0.016);
                Assert.AreEqual(23, effect.ParticleCount);
                var shapes = Shapes(Drawing(effect)).ToArray();
                var halos = shapes.Where(d => d.Brush is RadialGradientBrush).ToArray();
                Assert.AreEqual(effect.ParticleCount, halos.Length);
                foreach (var shape in halos)
                {
                    var brush = (RadialGradientBrush)shape.Brush;
                    Assert.IsTrue(brush.IsFrozen);
                    Assert.AreEqual((byte)0, brush.GradientStops.Last().Color.A);
                    var ellipse = (EllipseGeometry)shape.Geometry;
                    Assert.IsTrue(ellipse.RadiusX >= 2 && ellipse.RadiusX <= 6);
                }
                var centers = halos.Select(d => ((EllipseGeometry)d.Geometry).Center.X).ToArray();
                Assert.IsTrue(centers.Any(x => x > 130 && x < 230));
                Assert.IsTrue(centers.Any(x => x >= 254));
                Assert.IsFalse(effect.IsHitTestVisible);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                var preview = StardustCursorEffect.PreviewImage;
                Assert.IsTrue(preview.IsFrozen);
                Assert.IsTrue(Shapes(preview.Drawing).Any(d => d.Pen != null));
            });
        }

        [TestMethod]
        public void TwinkleIsGentleContinuousAndPhaseDependent()
        {
            var method = typeof(StardustCursorEffect).GetMethod("Twinkle", BindingFlags.Static | BindingFlags.NonPublic)!;
            double Sample(double age, double phase) => (double)method.Invoke(null, new object[] { age, phase, 1.5 })!;
            var values = Enumerable.Range(0, 101).Select(i => Sample(i / 100.0, 0)).ToArray();
            Assert.IsTrue(values.Min() >= 0.759 && values.Max() <= 1.001);
            Assert.IsTrue(values.Max() - values.Min() > 0.2);
            for (var i = 1; i < values.Length; i++) Assert.IsTrue(Math.Abs(values[i] - values[i - 1]) < 0.02);
            Assert.AreNotEqual(Sample(0, 0), Sample(0, Math.PI / 2));
        }

        [TestMethod]
        public void StarsFadeAndStopEmittingWhenStationary()
        {
            RunSta(() =>
            {
                var effect = new StardustCursorEffect();
                var point = new Point(200, 200);
                effect.Update(point, 0.016);
                var initial = ParticleOpacities(Drawing(effect)).ToArray();
                effect.Update(point, 0.8);
                var faded = ParticleOpacities(Drawing(effect)).ToArray();
                Assert.AreEqual(3, faded.Length);
                for (var i = 0; i < faded.Length; i++) Assert.IsTrue(faded[i] < initial[i]);
                effect.Update(point, 5);
                effect.Update(point, 0.016);
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.IsFalse(Shapes(Drawing(effect)).Any());
                effect.Update(new Point(600, 200), 0.016);
                Assert.AreEqual(1, effect.ParticleCount);
                effect.Clear();
                effect.Clear();
                effect.Update(new Point(600, 200), 0.016);
                Assert.AreEqual(3, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void EmissionAndHistoryAreBoundedAndInvalidInputIsIgnored()
        {
            RunSta(() =>
            {
                var effect = new StardustCursorEffect();
                foreach (var elapsed in new[] { 0.0, -1, double.NaN, double.PositiveInfinity }) effect.Update(new Point(100, 100), elapsed);
                effect.Update(new Point(double.NaN, 100), 0.016);
                effect.Update(new Point(100, double.PositiveInfinity), 0.016);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(new Point(0, 200), 0.016);
                effect.Update(new Point(10000, 200), 0.016);
                Assert.AreEqual(3 + StardustCursorEffect.MaxEmissionPerUpdate, effect.ParticleCount);
                for (var i = 0; i < 300; i++) effect.Update(new Point(i * 200, 200), 0.001);
                Assert.AreEqual(StardustCursorEffect.MaxParticles, effect.ParticleCount);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
            });
        }

        [TestMethod]
        public void RunningStarsApplyIndependentOpacity()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new StardustCursorEffect(settings);
                effect.Measure(new Size(400, 400));
                effect.Arrange(new Rect(0, 0, 400, 400));
                var point = new Point(200, 200);
                effect.Update(point, 0.016);
                var original = Alpha(effect);
                Assert.IsTrue(original > 0);
                settings.StardustOpacity = 0;
                effect.Update(point, 0.000001);
                Assert.AreEqual(0L, Alpha(effect));
                settings.StardustOpacity = 0.25;
                effect.Update(point, 0.000001);
                Assert.IsTrue(Alpha(effect) > 0 && Alpha(effect) < original);
                Assert.AreEqual(1.0, settings.FivePointedStarsOpacity);
                Assert.AreEqual(1.0, settings.WaterRippleOpacity);
            });
        }

        [DataTestMethod]
        [DataRow("switch")]
        [DataRow("start")]
        [DataRow("stop")]
        [DataRow("close")]
        public void WindowClearsStardust(string action)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                try
                {
                    window.UpdateMouseEffectType(MouseEffectType.Stardust);
                    var effect = new StardustCursorEffect();
                    effect.Update(new Point(200, 200), 0.016);
                    var field = typeof(MouseEffectWindow).GetField("stardustEffect", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    field.SetValue(window, effect);
                    var canvas = (Canvas)window.FindName("canvas");
                    canvas.Children.Add(effect);
                    if (action == "switch") window.UpdateMouseEffectType(MouseEffectType.WaterRipple);
                    else if (action == "start") window.StartMouseEffect();
                    else if (action == "stop") window.StopMouseEffect();
                    else window.Close();
                    Assert.AreEqual(0, effect.ParticleCount);
                    Assert.AreEqual(0, canvas.Children.Count);
                    Assert.IsNull(field.GetValue(window));
                }
                finally { window.Close(); }
            });
        }

        [TestMethod]
        public void DialogLoadsResetsAndDiscardsDraft()
        {
            RunSta(() =>
            {
                var config = new MainConfig();
                config.MouseEffectSetting.StardustOpacity = 0.6;
                config.MouseEffectSetting.StardustSizeScale = 1.75;
                var window = new MouseEffectSettingWindow(MouseEffectType.Stardust, config);
                try
                {
                    Assert.AreEqual("星尘粒子设置", window.Title);
                    var size = (Slider)window.FindName("slider_StardustSize");
                    Assert.AreEqual(175.0, size.Value);
                    Assert.AreEqual(50.0, size.Minimum);
                    Assert.AreEqual(300.0, size.Maximum);
                    Assert.AreEqual(Visibility.Visible, ((StackPanel)window.FindName("panel_StardustSize")).Visibility);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(60.0, slider.Value);
                    Assert.AreEqual(Visibility.Visible, ((StackPanel)window.FindName("panel_ParticleOpacity")).Visibility);
                    Assert.AreEqual(Visibility.Collapsed, ((StackPanel)window.FindName("panel_NeonRibbon")).Visibility);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    Assert.AreEqual(100.0, size.Value);
                    size.Value = 250;
                    slider.Value = 20;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.6, config.MouseEffectSetting.StardustOpacity);
                Assert.AreEqual(1.75, config.MouseEffectSetting.StardustSizeScale);
            });
        }

        [TestMethod]
        public void DialogPersistsStardustWithoutChangingOtherEffects()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-StardustTests-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    config.MouseEffectSetting.FivePointedStarsOpacity = 0.6;
                    config.MouseEffectSetting.WaterRippleOpacity = 0.4;
                    config.MouseEffectSetting.IsEnabled = true;
                    config.MouseEffectSetting.SelectedEffectType = (int)MouseEffectType.Stardust;
                    var window = new MouseEffectSettingWindow(MouseEffectType.Stardust, config);
                    Exception? failure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            ((Slider)window.FindName("slider_Opacity")).Value = 25;
                            ((Slider)window.FindName("slider_StardustSize")).Value = 250;
                            ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        catch (Exception ex) { failure = ex; }
                        finally { window.Close(); }
                    }, DispatcherPriority.Loaded);
                    Assert.AreEqual(true, window.ShowDialog());
                    if (failure != null) Assert.Fail(failure.ToString());
                    var restored = ConfigHelper.LoadMainConfig();
                    Assert.AreEqual(0.25, restored.MouseEffectSetting.StardustOpacity);
                    Assert.AreEqual(2.5, restored.MouseEffectSetting.StardustSizeScale);
                    Assert.AreEqual(2.5, config.MouseEffectSetting.StardustSizeScale);
                    Assert.AreEqual(0.6, restored.MouseEffectSetting.FivePointedStarsOpacity);
                    Assert.AreEqual(0.4, restored.MouseEffectSetting.WaterRippleOpacity);
                    Assert.IsTrue(restored.MouseEffectSetting.IsEnabled);
                    Assert.AreEqual(11, restored.MouseEffectSetting.SelectedEffectType);
                    var reopened = new MouseEffectSettingWindow(MouseEffectType.Stardust, restored);
                    try
                    {
                        Assert.AreEqual(25.0, ((Slider)reopened.FindName("slider_Opacity")).Value);
                        Assert.AreEqual(250.0, ((Slider)reopened.FindName("slider_StardustSize")).Value);
                    }
                    finally { reopened.Close(); }
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
            Assert.AreEqual(expected, new MouseEffectSetting { StardustSizeScale = value }.StardustSizeScale);
        }

        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{\"StardustOpacity\":0.4}}")]
        public void OldConfigurationKeepsOriginalSize(string json)
        {
            Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.StardustSizeScale);
        }

        [DataTestMethod]
        [DataRow(0.5)]
        [DataRow(1.75)]
        [DataRow(3.0)]
        public void ExistingStarsScaleAroundTheirCenters(double scale)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new StardustCursorEffect(settings);
                var point = new Point(200, 200);
                effect.Update(point, 0.016);
                var original = Shapes(Drawing(effect)).Where(d => d.Geometry is EllipseGeometry).Select(d => ((EllipseGeometry)d.Geometry).RadiusX).ToArray();
                settings.StardustSizeScale = scale;
                effect.Update(point, 0.000001);
                Assert.AreEqual(3, effect.ParticleCount);
                var groups = ScaledGroups(Drawing(effect)).ToArray();
                Assert.AreEqual(3, groups.Length);
                foreach (var group in groups)
                {
                    var matrix = group.Transform.Value;
                    Assert.AreEqual(scale, matrix.M11, 0.000001);
                    Assert.AreEqual(scale, matrix.M22, 0.000001);
                    var center = ((EllipseGeometry)Shapes(group).First().Geometry).Center;
                    Assert.AreEqual(center.X, matrix.Transform(center).X, 0.000001);
                    Assert.AreEqual(center.Y, matrix.Transform(center).Y, 0.000001);
                    Assert.IsTrue(Shapes(group).Count() >= 2);
                }
                var current = Shapes(Drawing(effect)).Where(d => d.Geometry is EllipseGeometry).Select(d => ((EllipseGeometry)d.Geometry).RadiusX).ToArray();
                for (var i = 0; i < original.Length; i++) Assert.AreEqual(original[i], current[i], 0.00001);
                settings.StardustSizeScale = 1;
                effect.Update(point, 0.000001);
                Assert.IsFalse(ScaledGroups(Drawing(effect)).Any());
                Assert.AreEqual(1.0, settings.StardustOpacity);
            });
        }

        [DataTestMethod]
        [DataRow(MouseEffectType.FivePointedStars)]
        [DataRow(MouseEffectType.InkFluid)]
        [DataRow(MouseEffectType.NeonRibbon)]
        public void OtherEffectsHideStardustSize(MouseEffectType type)
        {
            RunSta(() =>
            {
                var window = new MouseEffectSettingWindow(type, new MainConfig());
                try { Assert.AreEqual(Visibility.Collapsed, ((StackPanel)window.FindName("panel_StardustSize")).Visibility); }
                finally { window.Close(); }
            });
        }

        private static IEnumerable<DrawingGroup> ScaledGroups(Drawing drawing)
        {
            if (drawing is DrawingGroup group)
            {
                if (group.Transform != null && !group.Transform.Value.IsIdentity) yield return group;
                foreach (var child in group.Children)
                    foreach (var scaled in ScaledGroups(child)) yield return scaled;
            }
        }

        private static DrawingGroup Drawing(StardustCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
        private static IEnumerable<GeometryDrawing> Shapes(Drawing drawing)
        {
            if (drawing is GeometryDrawing geometry) yield return geometry;
            else if (drawing is DrawingGroup group)
                foreach (var child in group.Children)
                    foreach (var shape in Shapes(child)) yield return shape;
        }
        private static IEnumerable<double> ParticleOpacities(Drawing drawing)
        {
            if (drawing is DrawingGroup group)
            {
                if (group.Children.OfType<GeometryDrawing>().Any()) yield return group.Opacity;
                else foreach (var child in group.Children)
                    foreach (var opacity in ParticleOpacities(child)) yield return opacity;
            }
        }
        private static long Alpha(StardustCursorEffect effect)
        {
            var bitmap = new RenderTargetBitmap(400, 400, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(effect);
            var pixels = new byte[400 * 400 * 4];
            bitmap.CopyPixels(pixels, 400 * 4, 0);
            long result = 0;
            for (var i = 3; i < pixels.Length; i += 4) result += pixels[i];
            return result;
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
