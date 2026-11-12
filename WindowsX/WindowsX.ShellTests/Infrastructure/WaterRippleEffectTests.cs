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
    public class WaterRippleEffectTests
    {
        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{\"NeonRibbonOpacity\":0.5}}")]
        public void OldConfigurationAndTypesRemainCompatible(string json)
        {
            Assert.AreEqual(9, (int)MouseEffectType.NeonRibbon);
            Assert.AreEqual(10, (int)MouseEffectType.WaterRipple);
            Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.WaterRippleOpacity);
            var settings = JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting;
            Assert.AreEqual(1.0, settings.WaterRippleSizeScale);
            Assert.AreEqual("#69D2FF", settings.WaterRippleColor);
            Assert.AreEqual(1.0 / 0.14, settings.WaterRippleFrequency);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { WaterRippleOpacity = value }.WaterRippleOpacity);
        }

        [TestMethod]
        public void HollowRingsExpandAtFixedCenterWithDelayedWaves()
        {
            RunSta(() =>
            {
                var effect = new WaterRippleCursorEffect();
                var point = new Point(200, 200);
                effect.Update(point, 0.016);
                effect.Update(point, 0.1);
                var initial = Shapes(Drawing(effect)).ToArray();
                Assert.AreEqual(3, initial.Length);
                Assert.IsTrue(initial.All(d => d.Brush == null && d.Pen.IsFrozen && d.Geometry is EllipseGeometry));
                var radius = ((EllipseGeometry)initial[0].Geometry).RadiusX;
                effect.Update(point, 0.5);
                var expanded = Shapes(Drawing(effect)).ToArray();
                Assert.AreEqual(9, expanded.Length);
                Assert.IsTrue(((EllipseGeometry)expanded[0].Geometry).RadiusX > radius);
                foreach (var drawing in expanded)
                {
                    var ellipse = (EllipseGeometry)drawing.Geometry;
                    Assert.AreEqual(point, ellipse.Center);
                    Assert.AreEqual(ellipse.RadiusX, ellipse.RadiusY);
                }
                Assert.IsTrue(WaterRippleCursorEffect.PreviewImage.IsFrozen);
                Assert.IsFalse(effect.IsHitTestVisible);
            });
        }

        [TestMethod]
        public void EmissionIsThrottledBoundedAndStopsWhenStationary()
        {
            RunSta(() =>
            {
                var effect = new WaterRippleCursorEffect();
                effect.Update(new Point(0, 0), 0.01);
                effect.Update(new Point(100, 0), 0.01);
                Assert.AreEqual(1, effect.RippleCount);
                effect.Update(new Point(100, 0), WaterRippleCursorEffect.EmissionInterval);
                Assert.AreEqual(2, effect.RippleCount);
                for (var i = 0; i < 300; i++)
                {
                    effect.Update(new Point(i * 20, 200), 0.02);
                    Assert.IsTrue(effect.RippleCount <= WaterRippleCursorEffect.MaxRipples);
                }
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                effect.Update(new Point(5980, 200), WaterRippleCursorEffect.EmissionInterval);
                effect.Update(new Point(5980, 200), 10);
                effect.Update(new Point(5980, 200), 0.016);
                Assert.AreEqual(0, effect.RippleCount);
                Assert.IsFalse(Shapes(Drawing(effect)).Any());
                effect.Update(new Point(6100, 200), 10);
                Assert.AreEqual(1, effect.RippleCount);
                effect.Clear();
                effect.Clear();
                effect.Update(new Point(6100, 200), 0.016);
                Assert.AreEqual(1, effect.RippleCount);
            });
        }

        [TestMethod]
        public void InvalidInputsDoNotEmit()
        {
            RunSta(() =>
            {
                var effect = new WaterRippleCursorEffect();
                foreach (var elapsed in new[] { 0.0, -1.0, double.NaN, double.PositiveInfinity }) effect.Update(new Point(100, 100), elapsed);
                effect.Update(new Point(double.NaN, 100), 0.016);
                effect.Update(new Point(100, double.PositiveInfinity), 0.016);
                Assert.AreEqual(0, effect.RippleCount);
            });
        }

        [TestMethod]
        public void RunningOpacityChangesAndCenterRemainsTransparent()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new WaterRippleCursorEffect(settings);
                effect.Measure(new Size(400, 400));
                effect.Arrange(new Rect(0, 0, 400, 400));
                var point = new Point(200, 200);
                effect.Update(point, 0.016);
                effect.Update(point, 0.1);
                var pixels = Pixels(effect);
                Assert.AreEqual((byte)0, pixels[(200 * 400 + 200) * 4 + 3]);
                var original = Alpha(pixels);
                Assert.IsTrue(original > 0);
                settings.WaterRippleOpacity = 0;
                effect.Update(point, 0.000001);
                Assert.AreEqual(0L, Alpha(Pixels(effect)));
                settings.WaterRippleOpacity = 0.25;
                effect.Update(point, 0.000001);
                var reduced = Alpha(Pixels(effect));
                Assert.IsTrue(reduced > 0 && reduced < original);
                Assert.AreEqual(1.0, settings.NeonRibbonOpacity);
            });
        }

        [DataTestMethod]
        [DataRow("switch")]
        [DataRow("stop")]
        [DataRow("close")]
        [DataRow("start")]
        public void WindowClearsRipples(string action)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                try
                {
                    window.UpdateMouseEffectType(MouseEffectType.WaterRipple);
                    var effect = new WaterRippleCursorEffect();
                    effect.Update(new Point(200, 200), 0.016);
                    var field = typeof(MouseEffectWindow).GetField("cursorEffect", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    field.SetValue(window, effect);
                    var canvas = (Canvas)window.FindName("canvas");
                    canvas.Children.Add(effect);
                    if (action == "switch") window.UpdateMouseEffectType(MouseEffectType.NeonRibbon);
                    else if (action == "stop") window.StopMouseEffect();
                    else if (action == "start") window.StartMouseEffect();
                    else window.Close();
                    Assert.AreEqual(0, effect.RippleCount);
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
                config.MouseEffectSetting.WaterRippleOpacity = 0.6;
                config.MouseEffectSetting.WaterRippleSizeScale = 2;
                config.MouseEffectSetting.WaterRippleFrequency = 5;
                config.MouseEffectSetting.WaterRippleColor = "#ABCDEF";
                var window = new MouseEffectSettingWindow(MouseEffectType.WaterRipple, config);
                try
                {
                    Assert.AreEqual("水波涟漪设置", window.Title);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(60.0, slider.Value);
                    Assert.AreEqual(Visibility.Visible, ((StackPanel)window.FindName("panel_WaterRipple")).Visibility);
                    var size = (Slider)window.FindName("slider_WaterRippleSize");
                    var frequency = (Slider)window.FindName("slider_WaterRippleFrequency");
                    Assert.AreEqual(5.0, frequency.Value);
                    var color = (TextBox)window.FindName("text_WaterRippleColor");
                    Assert.AreEqual(200.0, size.Value);
                    Assert.AreEqual("#ABCDEF", color.Text);
                    color.Text = "invalid";
                    Assert.IsFalse(((Button)window.FindName("btn_Save")).IsEnabled);
                    Assert.AreEqual(Visibility.Visible, ((TextBlock)window.FindName("text_WaterRippleColorError")).Visibility);
                    Assert.IsNull(((Border)window.FindName("preview_WaterRippleColor")).Background);
                    foreach (var name in new[] { "panel_Expansion", "panel_InkFluid", "panel_NeonRibbon", "panel_Trail", "panel_Following" })
                        Assert.AreEqual(Visibility.Collapsed, ((StackPanel)window.FindName(name)).Visibility);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    Assert.AreEqual(100.0, size.Value);
                    Assert.AreEqual(MouseEffectSetting.DefaultWaterRippleFrequency, frequency.Value);
                    Assert.AreEqual("#69D2FF", color.Text);
                    Assert.IsTrue(((Button)window.FindName("btn_Save")).IsEnabled);
                    Assert.AreEqual(Color.FromRgb(105, 210, 255), ((SolidColorBrush)((Border)window.FindName("preview_WaterRippleColor")).Background).Color);
                    slider.Value = 20;
                    size.Value = 300;
                    frequency.Value = 20;
                    color.Text = "#FF0000";
                }
                finally { window.Close(); }
                Assert.AreEqual(0.6, config.MouseEffectSetting.WaterRippleOpacity);
                Assert.AreEqual(2.0, config.MouseEffectSetting.WaterRippleSizeScale);
                Assert.AreEqual(5.0, config.MouseEffectSetting.WaterRippleFrequency);
                Assert.AreEqual("#ABCDEF", config.MouseEffectSetting.WaterRippleColor);
            });
        }

        [TestMethod]
        public void DialogPersistsOnlyWaterSettingsAndKeepsSelection()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-WaterTests-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    config.MouseEffectSetting.NeonRibbonOpacity = 0.6;
                    config.MouseEffectSetting.InkFluidOpacity = 0.4;
                    config.MouseEffectSetting.IsEnabled = true;
                    config.MouseEffectSetting.SelectedEffectType = (int)MouseEffectType.WaterRipple;
                    var shared = config.MouseEffectSetting;
                    var window = new MouseEffectSettingWindow(MouseEffectType.WaterRipple, config);
                    Exception? failure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            ((Slider)window.FindName("slider_Opacity")).Value = 25;
                            ((Slider)window.FindName("slider_WaterRippleSize")).Value = 250;
                            ((Slider)window.FindName("slider_WaterRippleFrequency")).Value = 12.5;
                            ((TextBox)window.FindName("text_WaterRippleColor")).Text = " #aa33ff ";
                            ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        catch (Exception ex) { failure = ex; }
                        finally { window.Close(); }
                    }, DispatcherPriority.Loaded);
                    Assert.AreEqual(true, window.ShowDialog());
                    if (failure != null) Assert.Fail(failure.ToString());
                    var restored = ConfigHelper.LoadMainConfig();
                    Assert.AreEqual(0.25, restored.MouseEffectSetting.WaterRippleOpacity);
                    Assert.AreSame(shared, config.MouseEffectSetting);
                    Assert.AreEqual(2.5, shared.WaterRippleSizeScale);
                    Assert.AreEqual(12.5, shared.WaterRippleFrequency);
                    Assert.AreEqual(12.5, restored.MouseEffectSetting.WaterRippleFrequency);
                    Assert.AreEqual("#AA33FF", shared.WaterRippleColor);
                    Assert.AreEqual(2.5, restored.MouseEffectSetting.WaterRippleSizeScale);
                    Assert.AreEqual("#AA33FF", restored.MouseEffectSetting.WaterRippleColor);
                    Assert.AreEqual(0.6, restored.MouseEffectSetting.NeonRibbonOpacity);
                    Assert.AreEqual(0.4, restored.MouseEffectSetting.InkFluidOpacity);
                    Assert.IsTrue(restored.MouseEffectSetting.IsEnabled);
                    Assert.AreEqual(10, restored.MouseEffectSetting.SelectedEffectType);
                    var reopened = new MouseEffectSettingWindow(MouseEffectType.WaterRipple, restored);
                    try
                    {
                        Assert.AreEqual(25.0, ((Slider)reopened.FindName("slider_Opacity")).Value);
                        Assert.AreEqual(250.0, ((Slider)reopened.FindName("slider_WaterRippleSize")).Value);
                        Assert.AreEqual(12.5, ((Slider)reopened.FindName("slider_WaterRippleFrequency")).Value);
                        Assert.AreEqual("#AA33FF", ((TextBox)reopened.FindName("text_WaterRippleColor")).Text);
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
            Assert.AreEqual(expected, new MouseEffectSetting { WaterRippleSizeScale = value }.WaterRippleSizeScale);
        }

        [DataTestMethod]
        [DataRow(null, "#69D2FF")]
        [DataRow("red", "#69D2FF")]
        [DataRow("#123", "#69D2FF")]
        [DataRow("#GG0000", "#69D2FF")]
        [DataRow("#80112233", "#69D2FF")]
        [DataRow(" #ab12ef ", "#AB12EF")]
        public void ColorIsNormalized(string? value, string expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { WaterRippleColor = value! }.WaterRippleColor);
        }

        [DataTestMethod]
        [DataRow(0.5)]
        [DataRow(1.75)]
        [DataRow(3.0)]
        public void RunningRipplesScaleAllLayersAroundFixedCenter(double scale)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new WaterRippleCursorEffect(settings);
                var center = new Point(200, 200);
                effect.Measure(new Size(400, 400));
                effect.Arrange(new Rect(0, 0, 400, 400));
                effect.Update(center, 0.016);
                effect.Update(center, 0.1);
                var original = Shapes(Drawing(effect)).ToArray();
                settings.WaterRippleSizeScale = scale;
                effect.Update(center, 0.000001);
                var group = ScaledGroups(Drawing(effect)).Single();
                Assert.AreEqual(scale, group.Transform.Value.M11);
                Assert.AreEqual(scale, group.Transform.Value.M22);
                Assert.AreEqual(center, group.Transform.Transform(center));
                var current = Shapes(group).ToArray();
                Assert.AreEqual(3, current.Length);
                for (var i = 0; i < current.Length; i++)
                {
                    Assert.IsNull(current[i].Brush);
                    Assert.AreSame(original[i].Pen, current[i].Pen);
                    Assert.AreEqual(((EllipseGeometry)original[i].Geometry).RadiusX, ((EllipseGeometry)current[i].Geometry).RadiusX, 0.001);
                }
                Assert.AreEqual((byte)0, Pixels(effect)[(200 * 400 + 200) * 4 + 3]);
                Assert.AreEqual(1, effect.RippleCount);
                settings.WaterRippleSizeScale = 1;
                effect.Update(center, 0.000001);
                Assert.IsFalse(ScaledGroups(Drawing(effect)).Any());
            });
        }

        [TestMethod]
        public void RunningColorsRefreshCachedPensAndRestoreDefaults()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new WaterRippleCursorEffect(settings);
                var center = new Point(200, 200);
                effect.Update(center, 0.016);
                effect.Update(center, 0.1);
                var defaults = Shapes(Drawing(effect)).Select(d => d.Pen).ToArray();
                CollectionAssert.AreEqual(new[] { Color.FromArgb(55, 75, 190, 255), Color.FromRgb(105, 210, 255), Color.FromArgb(160, 225, 250, 255) },
                    defaults.Select(p => ((SolidColorBrush)p.Brush).Color).ToArray());
                settings.WaterRippleColor = "#FF0000";
                effect.Update(center, 0.000001);
                var pens = Shapes(Drawing(effect)).Select(d => d.Pen).ToArray();
                Assert.IsTrue(pens.All(p => p.IsFrozen));
                CollectionAssert.AreEqual(new[] { Color.FromArgb(55, 255, 0, 0), Colors.Red, Color.FromArgb(160, 255, 204, 204) },
                    pens.Select(p => ((SolidColorBrush)p.Brush).Color).ToArray());
                effect.Update(center, 0.000001);
                var unchanged = Shapes(Drawing(effect)).Select(d => d.Pen).ToArray();
                for (var i = 0; i < pens.Length; i++) Assert.AreSame(pens[i], unchanged[i]);
                settings.WaterRippleColor = MouseEffectSetting.DefaultWaterRippleColor;
                effect.Update(center, 0.000001);
                var restored = Shapes(Drawing(effect)).Select(d => d.Pen).ToArray();
                for (var i = 0; i < defaults.Length; i++) Assert.AreSame(defaults[i], restored[i]);
                Assert.AreEqual(1, effect.RippleCount);
            });
        }

        [DataTestMethod]
        [DataRow(MouseEffectType.HaloRing)]
        [DataRow(MouseEffectType.WillowCatkins)]
        public void OtherSettingsHideWaterControls(MouseEffectType type)
        {
            RunSta(() =>
            {
                var window = new MouseEffectSettingWindow(type, new MainConfig());
                try { Assert.AreEqual(Visibility.Collapsed, ((StackPanel)window.FindName("panel_WaterRipple")).Visibility); }
                finally { window.Close(); }
            });
        }

        [DataTestMethod]
        [DataRow(0.0, 1.0)]
        [DataRow(-5.0, 1.0)]
        [DataRow(100.0, 20.0)]
        [DataRow(12.5, 12.5)]
        [DataRow(double.NaN, MouseEffectSetting.DefaultWaterRippleFrequency)]
        [DataRow(double.PositiveInfinity, MouseEffectSetting.DefaultWaterRippleFrequency)]
        [DataRow(double.NegativeInfinity, MouseEffectSetting.DefaultWaterRippleFrequency)]
        public void FrequencyIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { WaterRippleFrequency = value }.WaterRippleFrequency);
        }

        [DataTestMethod]
        [DataRow(1.0)]
        [DataRow(5.0)]
        [DataRow(20.0)]
        public void FrequencyControlsEmissionWithoutChangingLifetime(double frequency)
        {
            RunSta(() =>
            {
                var effect = new WaterRippleCursorEffect(new MouseEffectSetting { WaterRippleFrequency = frequency });
                effect.Update(new Point(100, 100), 0.016);
                effect.Update(new Point(120, 100), 0.99 / frequency);
                Assert.AreEqual(1, effect.RippleCount);
                effect.Update(new Point(120, 100), 0.02 / frequency);
                Assert.AreEqual(2, effect.RippleCount);
                effect.Update(new Point(120, 100), WaterRippleCursorEffect.Lifetime + 0.01);
                Assert.AreEqual(0, effect.RippleCount);
                effect.Update(new Point(140, 100), 10);
                Assert.AreEqual(1, effect.RippleCount);
            });
        }

        [TestMethod]
        public void FrequencyChangesApplyToRunningEffectAndKeepEmissionBounded()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting { WaterRippleFrequency = 1 };
                var effect = new WaterRippleCursorEffect(settings);
                effect.Update(new Point(100, 100), 0.016);
                effect.Update(new Point(120, 100), 0.2);
                Assert.AreEqual(1, effect.RippleCount);
                settings.WaterRippleFrequency = 20;
                effect.Update(new Point(120, 100), 0.001);
                Assert.AreEqual(2, effect.RippleCount);
                settings.WaterRippleFrequency = 1;
                effect.Update(new Point(140, 100), 0.06);
                Assert.AreEqual(2, effect.RippleCount);
                settings.WaterRippleFrequency = 20;
                for (var i = 0; i < 200; i++)
                {
                    effect.Update(new Point(160 + i * 20, 100), 0.05);
                    Assert.IsTrue(effect.RippleCount <= WaterRippleCursorEffect.MaxRipples);
                }
                Assert.AreEqual(WaterRippleCursorEffect.MaxRipples, effect.RippleCount);
                effect.Clear();
                effect.Update(new Point(100, 100), 0.001);
                Assert.AreEqual(1, effect.RippleCount);
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

        private static DrawingGroup Drawing(WaterRippleCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
        private static IEnumerable<GeometryDrawing> Shapes(Drawing drawing)
        {
            if (drawing is GeometryDrawing geometry) yield return geometry;
            else if (drawing is DrawingGroup group)
                foreach (var child in group.Children)
                    foreach (var shape in Shapes(child)) yield return shape;
        }
        private static byte[] Pixels(WaterRippleCursorEffect effect)
        {
            var bitmap = new RenderTargetBitmap(400, 400, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(effect);
            var pixels = new byte[400 * 400 * 4];
            bitmap.CopyPixels(pixels, 400 * 4, 0);
            return pixels;
        }
        private static long Alpha(byte[] pixels)
        {
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
