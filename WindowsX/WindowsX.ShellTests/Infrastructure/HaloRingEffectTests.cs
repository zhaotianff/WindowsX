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
    public class HaloRingEffectTests
    {
        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{\"FollowingSize\":50}}")]
        public void DefaultsAndTypesRemainCompatible(string json)
        {
            Assert.AreEqual(16, (int)MouseEffectType.IconRain);
            Assert.AreEqual(17, (int)MouseEffectType.HaloRing);
            Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.HaloRingOpacity);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { HaloRingOpacity = value }.HaloRingOpacity);
        }

        [TestMethod]
        public void RingIsHollowGradientAndBreathesWhileStationary()
        {
            RunSta(() =>
            {
                var effect = new HaloRingCursorEffect();
                var point = new Point(100, 100);
                effect.Update(point, 2.4);
                var shapes = Shapes(Drawing(effect)).ToArray();
                Assert.AreEqual(4, shapes.Length);
                foreach (var shape in shapes)
                {
                    Assert.IsNull(shape.Brush);
                    Assert.IsTrue(shape.Pen.IsFrozen);
                    Assert.AreEqual(3, ((LinearGradientBrush)shape.Pen.Brush).GradientStops.Count);
                    var ring = (EllipseGeometry)shape.Geometry;
                    Assert.AreEqual(point, ring.Center);
                    Assert.AreEqual(22.0, ring.RadiusX, 0.000001);
                    Assert.AreEqual(ring.RadiusX, ring.RadiusY);
                    Assert.IsTrue(ring.RadiusX - shape.Pen.Thickness / 2 >= 16);
                }
                var dim = Groups(Drawing(effect)).Min(g => g.Opacity);
                effect.Update(point, 1.2);
                Assert.AreEqual(26.0, ((EllipseGeometry)Shapes(Drawing(effect)).First().Geometry).RadiusX, 0.000001);
                Assert.IsTrue(Groups(Drawing(effect)).Any(g => g.Opacity == 1));
                Assert.IsTrue(dim > 0);
                effect.Update(point, 1.2);
                Assert.AreEqual(22.0, ((EllipseGeometry)Shapes(Drawing(effect)).First().Geometry).RadiusX, 0.000001);
                Assert.IsFalse(effect.IsHitTestVisible);
                Assert.IsTrue(HaloRingCursorEffect.PreviewImage.IsFrozen);
            });
        }

        [TestMethod]
        public void BreathingIsFrameIndependentAndMovementLeavesNoHistory()
        {
            RunSta(() =>
            {
                var first = new HaloRingCursorEffect();
                var second = new HaloRingCursorEffect();
                var point = new Point(100, 100);
                first.Update(point, 0.6);
                for (var i = 0; i < 60; i++) second.Update(point, 0.01);
                Assert.AreEqual(((EllipseGeometry)Shapes(Drawing(first)).First().Geometry).RadiusX,
                    ((EllipseGeometry)Shapes(Drawing(second)).First().Geometry).RadiusX, 0.000001);
                for (var i = 0; i < 1000; i++) second.Update(new Point(i, i), 0.016);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(second));
                Assert.AreEqual(4, Shapes(Drawing(second)).Count());
                Assert.IsTrue(Shapes(Drawing(second)).All(s => ((EllipseGeometry)s.Geometry).Center == new Point(999, 999)));
                second.Update(point, double.MaxValue);
                Assert.IsTrue(Shapes(Drawing(second)).All(s => double.IsFinite(((EllipseGeometry)s.Geometry).RadiusX)));
                second.Clear();
                second.Clear();
                Assert.IsFalse(Shapes(Drawing(second)).Any());
                second.Update(point, 2.4);
                Assert.AreEqual(22.0, ((EllipseGeometry)Shapes(Drawing(second)).First().Geometry).RadiusX, 0.000001);
            });
        }

        [TestMethod]
        public void InvalidInputIsIgnoredAndRenderedCenterRemainsTransparent()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new HaloRingCursorEffect(settings);
                foreach (var elapsed in new[] { 0.0, -1, double.NaN, double.PositiveInfinity }) effect.Update(new Point(100, 100), elapsed);
                effect.Update(new Point(double.NaN, 100), 0.016);
                effect.Update(new Point(100, double.PositiveInfinity), 0.016);
                Assert.IsFalse(Shapes(Drawing(effect)).Any());
                effect.Measure(new Size(200, 200));
                effect.Arrange(new Rect(0, 0, 200, 200));
                effect.Update(new Point(100, 100), 1.2);
                var pixels = Pixels(effect);
                long alpha = Alpha(pixels);
                Assert.IsTrue(alpha > 0);
                for (var y = 90; y < 110; y++)
                    for (var x = 90; x < 110; x++) Assert.AreEqual((byte)0, pixels[(y * 200 + x) * 4 + 3]);
                settings.HaloRingOpacity = 0;
                effect.Update(new Point(100, 100), 2.4);
                Assert.AreEqual(0L, Alpha(Pixels(effect)));
                settings.HaloRingOpacity = 0.25;
                effect.Update(new Point(100, 100), 2.4);
                Assert.IsTrue(Alpha(Pixels(effect)) > 0 && Alpha(Pixels(effect)) < alpha);
                Assert.AreEqual(1.0, settings.IconRainOpacity);
            });
        }

        [DataTestMethod]
        [DataRow("switch")]
        [DataRow("start")]
        [DataRow("stop")]
        [DataRow("close")]
        public void WindowClearsRing(string action)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                try
                {
                    window.UpdateMouseEffectType(MouseEffectType.HaloRing);
                    var effect = new HaloRingCursorEffect();
                    effect.Update(new Point(100, 100), 0.016);
                    var field = typeof(MouseEffectWindow).GetField("haloRingEffect", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    field.SetValue(window, effect);
                    var canvas = (Canvas)window.FindName("canvas");
                    canvas.Children.Add(effect);
                    if (action == "switch") window.UpdateMouseEffectType(MouseEffectType.FollowingDot);
                    else if (action == "start") window.StartMouseEffect();
                    else if (action == "stop") window.StopMouseEffect();
                    else window.Close();
                    Assert.IsFalse(Shapes(Drawing(effect)).Any());
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
                config.MouseEffectSetting.HaloRingOpacity = 0.6;
                var window = new MouseEffectSettingWindow(MouseEffectType.HaloRing, config);
                try
                {
                    Assert.AreEqual("光晕圈设置", window.Title);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(60.0, slider.Value);
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_ParticleOpacity")).Visibility);
                    foreach (var name in new[] { "panel_Expansion", "panel_StardustSize", "panel_InkFluid", "panel_NeonRibbon", "panel_Following", "panel_Trail" })
                        Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName(name)).Visibility);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    slider.Value = 20;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.6, config.MouseEffectSetting.HaloRingOpacity);
            });
        }

        [TestMethod]
        public void DialogPersistsOnlyRingAndKeepsSelection()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-HaloRingTests-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.FollowingSize = 60;
                    settings.IconRainOpacity = 0.6;
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)MouseEffectType.HaloRing;
                    var window = new MouseEffectSettingWindow(MouseEffectType.HaloRing, config);
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
                    Assert.AreEqual(0.25, settings.HaloRingOpacity);
                    var restored = ConfigHelper.LoadMainConfig();
                    Assert.AreEqual(0.25, restored.MouseEffectSetting.HaloRingOpacity);
                    Assert.AreEqual(0.6, restored.MouseEffectSetting.IconRainOpacity);
                    Assert.AreEqual(60.0, restored.MouseEffectSetting.FollowingSize);
                    Assert.IsTrue(restored.MouseEffectSetting.IsEnabled);
                    Assert.AreEqual(17, restored.MouseEffectSetting.SelectedEffectType);
                    var reopened = new MouseEffectSettingWindow(MouseEffectType.HaloRing, restored);
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

        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{\"HaloRingOpacity\":0.6}}")]
        public void NewSettingsPreserveLegacyAppearance(string json)
        {
            var settings = JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting;
            Assert.AreEqual(1.0, settings.HaloRingSizeScale);
            Assert.AreEqual(1.0 / 2.4, settings.HaloRingBreathingFrequency);
            Assert.AreEqual("#41E1FF", settings.HaloRingColor1);
            Assert.AreEqual("#9B6EFF", settings.HaloRingColor2);
            Assert.AreEqual("#FF78C8", settings.HaloRingColor3);
            var restored = JsonConvert.DeserializeObject<MouseEffectSetting>(JsonConvert.SerializeObject(settings))!;
            Assert.AreEqual(settings.HaloRingBreathingFrequency, restored.HaloRingBreathingFrequency);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.5, 0.1)]
        [DataRow(10.0, 3.0, 2.0)]
        [DataRow(1.5, 1.5, 1.5)]
        [DataRow(double.NaN, 1.0, MouseEffectSetting.DefaultHaloRingBreathingFrequency)]
        [DataRow(double.PositiveInfinity, 1.0, MouseEffectSetting.DefaultHaloRingBreathingFrequency)]
        [DataRow(double.NegativeInfinity, 1.0, MouseEffectSetting.DefaultHaloRingBreathingFrequency)]
        public void SizeAndFrequencyAreNormalized(double value, double size, double frequency)
        {
            var settings = new MouseEffectSetting { HaloRingSizeScale = value, HaloRingBreathingFrequency = value };
            Assert.AreEqual(size, settings.HaloRingSizeScale);
            Assert.AreEqual(frequency, settings.HaloRingBreathingFrequency);
        }

        [TestMethod]
        public void ColorsAreNormalizedAndInvalidValuesUseDefaults()
        {
            var settings = new MouseEffectSetting
            {
                HaloRingColor1 = " #abcdef ", HaloRingColor2 = "#123abc", HaloRingColor3 = "#fedcba"
            };
            Assert.AreEqual("#ABCDEF", settings.HaloRingColor1);
            Assert.AreEqual("#123ABC", settings.HaloRingColor2);
            Assert.AreEqual("#FEDCBA", settings.HaloRingColor3);
            foreach (var invalid in new[] { null, "", "red", "#123", "#12345G", "#00112233" })
            {
                settings.HaloRingColor1 = invalid!;
                settings.HaloRingColor2 = invalid!;
                settings.HaloRingColor3 = invalid!;
                Assert.AreEqual(MouseEffectSetting.DefaultHaloRingColor1, settings.HaloRingColor1);
                Assert.AreEqual(MouseEffectSetting.DefaultHaloRingColor2, settings.HaloRingColor2);
                Assert.AreEqual(MouseEffectSetting.DefaultHaloRingColor3, settings.HaloRingColor3);
            }
        }

        [DataTestMethod]
        [DataRow(0.5)]
        [DataRow(2.0)]
        [DataRow(3.0)]
        public void SizeScalesRingAndGlowAndFrequencyControlsCycle(double scale)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting { HaloRingSizeScale = scale, HaloRingBreathingFrequency = 2 };
                var effect = new HaloRingCursorEffect(settings);
                var point = new Point(100, 100);
                effect.Update(point, 0.25);
                var shapes = Shapes(Drawing(effect)).ToArray();
                var widths = new[] { 12.0, 8, 5, 2 };
                for (var i = 0; i < shapes.Length; i++)
                {
                    Assert.IsNull(shapes[i].Brush);
                    Assert.AreEqual(26 * scale, ((EllipseGeometry)shapes[i].Geometry).RadiusX, 0.000001);
                    Assert.AreEqual(widths[i] * scale, shapes[i].Pen.Thickness, 0.000001);
                }
                effect.Update(point, 0.25);
                Assert.AreEqual(22 * scale, ((EllipseGeometry)Shapes(Drawing(effect)).First().Geometry).RadiusX, 0.000001);
                effect.Update(point, 0.125);
                settings.HaloRingBreathingFrequency = 1;
                effect.Update(point, 0.25);
                Assert.AreEqual(26 * scale, ((EllipseGeometry)Shapes(Drawing(effect)).First().Geometry).RadiusX, 0.000001);
                effect.Update(point, double.MaxValue);
                Assert.IsTrue(Shapes(Drawing(effect)).All(s => double.IsFinite(((EllipseGeometry)s.Geometry).RadiusX)));
            });
        }

        [TestMethod]
        public void CustomFrequencyRemainsFrameIndependent()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting { HaloRingBreathingFrequency = 0.7 };
                var first = new HaloRingCursorEffect(settings);
                var second = new HaloRingCursorEffect(settings);
                var point = new Point(100, 100);
                first.Update(point, 0.6);
                for (var i = 0; i < 60; i++) second.Update(point, 0.01);
                Assert.AreEqual(((EllipseGeometry)Shapes(Drawing(first)).First().Geometry).RadiusX,
                    ((EllipseGeometry)Shapes(Drawing(second)).First().Geometry).RadiusX, 0.000001);
            });
        }

        [TestMethod]
        public void CustomColorsAndSizeUpdateLiveAndPensAreReused()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting
                {
                    HaloRingColor1 = "#FF0000", HaloRingColor2 = "#00FF00", HaloRingColor3 = "#0000FF"
                };
                var effect = new HaloRingCursorEffect(settings);
                var point = new Point(100, 100);
                effect.Update(point, 1.2);
                var pen = Shapes(Drawing(effect)).First().Pen;
                var stops = ((LinearGradientBrush)pen.Brush).GradientStops;
                Assert.AreEqual(Colors.Red, stops[0].Color);
                Assert.AreEqual(Colors.Lime, stops[1].Color);
                Assert.AreEqual(Colors.Blue, stops[2].Color);
                effect.Update(point, 2.4);
                Assert.AreSame(pen, Shapes(Drawing(effect)).First().Pen);
                settings.HaloRingSizeScale = 2;
                settings.HaloRingColor1 = settings.HaloRingColor2 = settings.HaloRingColor3 = "#FFD700";
                effect.Update(point, 2.4);
                foreach (var shape in Shapes(Drawing(effect)))
                {
                    Assert.IsTrue(shape.Pen.IsFrozen && shape.Pen.Brush.IsFrozen);
                    Assert.AreEqual(52.0, ((EllipseGeometry)shape.Geometry).RadiusX, 0.000001);
                    Assert.IsTrue(((LinearGradientBrush)shape.Pen.Brush).GradientStops.All(s => s.Color == Colors.Gold));
                }
            });
        }

        [TestMethod]
        public void NewControlsValidateResetAndDiscardDraft()
        {
            RunSta(() =>
            {
                var config = new MainConfig();
                var settings = config.MouseEffectSetting;
                settings.HaloRingSizeScale = 2;
                settings.HaloRingBreathingFrequency = 1.5;
                settings.HaloRingColor1 = "#ABCDEF";
                var original = JsonConvert.SerializeObject(config);
                var window = new MouseEffectSettingWindow(MouseEffectType.HaloRing, config);
                try
                {
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_HaloRing")).Visibility);
                    var size = (Slider)window.FindName("slider_HaloRingSize");
                    var frequency = (Slider)window.FindName("slider_HaloRingBreathingFrequency");
                    Assert.AreEqual(200.0, size.Value);
                    Assert.AreEqual(1.5, frequency.Value);
                    Assert.AreEqual("#ABCDEF", ((TextBox)window.FindName("text_HaloRingColor1")).Text);
                    for (var i = 1; i <= 3; i++)
                    {
                        var color = (TextBox)window.FindName("text_HaloRingColor" + i);
                        color.Text = "invalid";
                        var save = (Button)window.FindName("btn_Save");
                        Assert.IsFalse(save.IsEnabled);
                        Assert.AreEqual(Visibility.Visible, ((TextBlock)window.FindName("text_HaloRingColorError")).Visibility);
                        save.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        Assert.AreEqual(original, JsonConvert.SerializeObject(config));
                        color.Text = " #abcdef ";
                        Assert.IsTrue(save.IsEnabled);
                        Assert.AreEqual(Color.FromRgb(171, 205, 239), ((SolidColorBrush)((Border)window.FindName("preview_HaloRingColor" + i)).Background).Color);
                    }
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, size.Value);
                    Assert.AreEqual(MouseEffectSetting.DefaultHaloRingBreathingFrequency, frequency.Value);
                    Assert.AreEqual(MouseEffectSetting.DefaultHaloRingColor1, ((TextBox)window.FindName("text_HaloRingColor1")).Text);
                    Assert.AreEqual(MouseEffectSetting.DefaultHaloRingColor2, ((TextBox)window.FindName("text_HaloRingColor2")).Text);
                    Assert.AreEqual(MouseEffectSetting.DefaultHaloRingColor3, ((TextBox)window.FindName("text_HaloRingColor3")).Text);
                    size.Value = 300;
                    frequency.Value = 2;
                }
                finally { window.Close(); }
                Assert.AreEqual(original, JsonConvert.SerializeObject(config));
                var other = new MouseEffectSettingWindow(MouseEffectType.IconRain, config);
                try { Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)other.FindName("panel_HaloRing")).Visibility); }
                finally { other.Close(); }
            });
        }

        [TestMethod]
        public void NewSettingsPersistAndReloadWithoutChangingOtherEffects()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-HaloRingSettingsTests-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)MouseEffectType.HaloRing;
                    settings.StardustSizeScale = 1.5;
                    settings.NeonRibbonColor1 = "#123456";
                    var window = new MouseEffectSettingWindow(MouseEffectType.HaloRing, config);
                    Exception? failure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            ((Slider)window.FindName("slider_HaloRingSize")).Value = 250;
                            ((Slider)window.FindName("slider_HaloRingBreathingFrequency")).Value = 1.5;
                            ((TextBox)window.FindName("text_HaloRingColor1")).Text = " #abcdef ";
                            ((TextBox)window.FindName("text_HaloRingColor2")).Text = "#123abc";
                            ((TextBox)window.FindName("text_HaloRingColor3")).Text = "#fedcba";
                            ((Slider)window.FindName("slider_Opacity")).Value = 40;
                            ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        catch (Exception ex) { failure = ex; }
                        finally { window.Close(); }
                    }, DispatcherPriority.Loaded);
                    Assert.AreEqual(true, window.ShowDialog());
                    if (failure != null) Assert.Fail(failure.ToString());
                    Assert.AreSame(settings, config.MouseEffectSetting);
                    var restored = ConfigHelper.LoadMainConfig();
                    Assert.AreEqual(JsonConvert.SerializeObject(settings), JsonConvert.SerializeObject(restored.MouseEffectSetting));
                    Assert.AreEqual(2.5, settings.HaloRingSizeScale);
                    Assert.AreEqual(1.5, settings.HaloRingBreathingFrequency);
                    Assert.AreEqual("#ABCDEF", settings.HaloRingColor1);
                    Assert.AreEqual("#123ABC", settings.HaloRingColor2);
                    Assert.AreEqual("#FEDCBA", settings.HaloRingColor3);
                    Assert.AreEqual(0.4, settings.HaloRingOpacity);
                    Assert.AreEqual(1.5, settings.StardustSizeScale);
                    Assert.AreEqual("#123456", settings.NeonRibbonColor1);
                    Assert.IsTrue(settings.IsEnabled);
                    Assert.AreEqual((int)MouseEffectType.HaloRing, settings.SelectedEffectType);
                    var reopened = new MouseEffectSettingWindow(MouseEffectType.HaloRing, restored);
                    try
                    {
                        Assert.AreEqual(250.0, ((Slider)reopened.FindName("slider_HaloRingSize")).Value);
                        Assert.AreEqual(1.5, ((Slider)reopened.FindName("slider_HaloRingBreathingFrequency")).Value);
                        Assert.AreEqual("#ABCDEF", ((TextBox)reopened.FindName("text_HaloRingColor1")).Text);
                        Assert.AreEqual("#123ABC", ((TextBox)reopened.FindName("text_HaloRingColor2")).Text);
                        Assert.AreEqual("#FEDCBA", ((TextBox)reopened.FindName("text_HaloRingColor3")).Text);
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

        private static DrawingGroup Drawing(HaloRingCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
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
        private static byte[] Pixels(HaloRingCursorEffect effect)
        {
            var bitmap = new RenderTargetBitmap(200, 200, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(effect);
            var pixels = new byte[200 * 200 * 4];
            bitmap.CopyPixels(pixels, 200 * 4, 0);
            return pixels;
        }
        private static long Alpha(byte[] pixels)
        {
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
