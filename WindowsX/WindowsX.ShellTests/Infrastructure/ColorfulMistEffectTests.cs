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
    public class ColorfulMistEffectTests
    {
        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{\"FallingPetalsOpacity\":0.4}}")]
        public void OldConfigurationAndTypesRemainCompatible(string json)
        {
            Assert.AreEqual(12, (int)MouseEffectType.FallingPetals);
            Assert.AreEqual(13, (int)MouseEffectType.ColorfulMist);
            Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.ColorfulMistOpacity);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { ColorfulMistOpacity = value }.ColorfulMistOpacity);
        }

        [TestMethod]
        public void MistUsesMultipleSoftColorsAndExpandsDriftsAndFades()
        {
            RunSta(() =>
            {
                var effect = new ColorfulMistCursorEffect();
                var point = new Point(200, 200);
                effect.Update(point, 0.016);
                var initial = Shapes(Drawing(effect)).ToArray();
                Assert.AreEqual(2, initial.Length);
                foreach (var shape in initial)
                {
                    Assert.IsNull(shape.Pen);
                    var brush = (RadialGradientBrush)shape.Brush;
                    Assert.IsTrue(brush.IsFrozen);
                    Assert.AreEqual(0, brush.GradientStops.Last().Color.A);
                    var alphas = brush.GradientStops.Select(s => s.Color.A).ToArray();
                    Assert.IsTrue(alphas[0] < 128 && alphas[0] > alphas[1] && alphas[1] > alphas[2]);
                }
                Assert.AreNotEqual(((RadialGradientBrush)initial[0].Brush).GradientStops[0].Color,
                    ((RadialGradientBrush)initial[1].Brush).GradientStops[0].Color);
                var before = (EllipseGeometry)initial[0].Geometry;
                effect.Update(point, 0.7);
                var after = (EllipseGeometry)Shapes(Drawing(effect)).First().Geometry;
                Assert.IsTrue(after.RadiusX > before.RadiusX);
                Assert.IsTrue(after.Center.Y < before.Center.Y);
                Assert.IsTrue(Groups(Drawing(effect)).Any(g => g.Opacity < 1));
                Assert.AreEqual(1, effect.ParticleCount);
                Assert.IsFalse(effect.IsHitTestVisible);
                var preview = ColorfulMistCursorEffect.PreviewImage;
                Assert.IsTrue(preview.IsFrozen);
                Assert.AreEqual(5, Shapes(preview.Drawing).Select(s => s.Brush).OfType<RadialGradientBrush>()
                    .Select(b => b.GradientStops[0].Color).Distinct().Count());
            });
        }

        [TestMethod]
        public void EmissionIsInterpolatedBoundedAndStopsWhenStationary()
        {
            RunSta(() =>
            {
                var effect = new ColorfulMistCursorEffect();
                effect.Update(new Point(100, 200), 0.016);
                effect.Update(new Point(220, 200), 0.016);
                Assert.AreEqual(11, effect.ParticleCount);
                Assert.IsTrue(Shapes(Drawing(effect)).Select(s => (EllipseGeometry)s.Geometry).Any(g => g.Center.X > 140 && g.Center.X < 180));
                effect.Update(new Point(10000, 200), 0.016);
                Assert.AreEqual(11 + ColorfulMistCursorEffect.MaxEmissionPerUpdate, effect.ParticleCount);
                for (var i = 0; i < 100; i++) effect.Update(new Point(i * 400, 200), 0.016);
                Assert.AreEqual(ColorfulMistCursorEffect.MaxParticles, effect.ParticleCount);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                effect.Update(new Point(39600, 200), 4);
                effect.Update(new Point(39600, 200), 0.016);
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.IsFalse(Shapes(Drawing(effect)).Any());
                effect.Update(new Point(100, 200), 0.016);
                Assert.AreEqual(1, effect.ParticleCount);
                effect.Clear();
                effect.Clear();
                Assert.IsFalse(Shapes(Drawing(effect)).Any());
                effect.Update(new Point(100, 200), 0.016);
                Assert.AreEqual(1, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void InvalidInputDoesNotEmit()
        {
            RunSta(() =>
            {
                var effect = new ColorfulMistCursorEffect();
                foreach (var elapsed in new[] { 0.0, -1, double.NaN, double.PositiveInfinity }) effect.Update(new Point(200, 200), elapsed);
                effect.Update(new Point(double.NaN, 200), 0.016);
                effect.Update(new Point(200, double.PositiveInfinity), 0.016);
                Assert.AreEqual(0, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void RunningMistUsesIndependentOpacity()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new ColorfulMistCursorEffect(settings);
                effect.Measure(new Size(400, 400));
                effect.Arrange(new Rect(0, 0, 400, 400));
                effect.Update(new Point(200, 200), 0.016);
                var initial = Alpha(effect);
                Assert.IsTrue(initial > 0);
                settings.ColorfulMistOpacity = 0;
                effect.Update(new Point(200, 200), 0.000001);
                Assert.AreEqual(0L, Alpha(effect));
                settings.ColorfulMistOpacity = 0.25;
                effect.Update(new Point(200, 200), 0.000001);
                Assert.IsTrue(Alpha(effect) > 0 && Alpha(effect) < initial);
                Assert.AreEqual(1.0, settings.InkFluidOpacity);
            });
        }

        [DataTestMethod]
        [DataRow("switch")]
        [DataRow("start")]
        [DataRow("stop")]
        [DataRow("close")]
        public void WindowClearsMist(string action)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                try
                {
                    window.UpdateMouseEffectType(MouseEffectType.ColorfulMist);
                    var effect = new ColorfulMistCursorEffect();
                    effect.Update(new Point(200, 200), 0.016);
                    var field = typeof(MouseEffectWindow).GetField("colorfulMistEffect", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    field.SetValue(window, effect);
                    var canvas = (Canvas)window.FindName("canvas");
                    canvas.Children.Add(effect);
                    if (action == "switch") window.UpdateMouseEffectType(MouseEffectType.FallingPetals);
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
                config.MouseEffectSetting.ColorfulMistOpacity = 0.6;
                var window = new MouseEffectSettingWindow(MouseEffectType.ColorfulMist, config);
                try
                {
                    Assert.AreEqual("彩色雾设置", window.Title);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(60.0, slider.Value);
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_ParticleOpacity")).Visibility);
                    foreach (var name in new[] { "panel_StardustSize", "panel_Expansion", "panel_NeonRibbon", "panel_InkFluid", "panel_Following", "panel_Trail" })
                        Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName(name)).Visibility);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    slider.Value = 20;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.6, config.MouseEffectSetting.ColorfulMistOpacity);
            });
        }

        [TestMethod]
        public void DialogPersistsOnlyMistAndKeepsSelection()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-MistTests-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.FallingPetalsOpacity = 0.6;
                    settings.InkFluidColor1 = "#123456";
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)MouseEffectType.ColorfulMist;
                    var window = new MouseEffectSettingWindow(MouseEffectType.ColorfulMist, config);
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
                    Assert.AreEqual(0.25, settings.ColorfulMistOpacity);
                    var restored = ConfigHelper.LoadMainConfig();
                    Assert.AreEqual(0.25, restored.MouseEffectSetting.ColorfulMistOpacity);
                    Assert.AreEqual(0.6, restored.MouseEffectSetting.FallingPetalsOpacity);
                    Assert.AreEqual("#123456", restored.MouseEffectSetting.InkFluidColor1);
                    Assert.IsTrue(restored.MouseEffectSetting.IsEnabled);
                    Assert.AreEqual(13, restored.MouseEffectSetting.SelectedEffectType);
                    var reopened = new MouseEffectSettingWindow(MouseEffectType.ColorfulMist, restored);
                    try { Assert.AreEqual(25.0, ((Slider)reopened.FindName("slider_Opacity")).Value); }
                    finally { reopened.Close(); }
                }
                finally
                {
                    Environment.CurrentDirectory = original;
                    System.IO.Directory.Delete(directory, true);
                }
            });
        }

        private static DrawingGroup Drawing(ColorfulMistCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
        private static IEnumerable<DrawingGroup> Groups(Drawing drawing)
        {
            if (drawing is DrawingGroup group)
            {
                yield return group;
                foreach (var child in group.Children)
                    foreach (var item in Groups(child)) yield return item;
            }
        }
        private static IEnumerable<GeometryDrawing> Shapes(Drawing drawing)
        {
            if (drawing is GeometryDrawing geometry) yield return geometry;
            else if (drawing is DrawingGroup group)
                foreach (var child in group.Children)
                    foreach (var shape in Shapes(child)) yield return shape;
        }
        private static long Alpha(ColorfulMistCursorEffect effect)
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
