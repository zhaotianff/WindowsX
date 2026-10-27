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
                    var field = typeof(MouseEffectWindow).GetField("waterRippleEffect", BindingFlags.NonPublic | BindingFlags.Instance)!;
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
                var window = new MouseEffectSettingWindow(MouseEffectType.WaterRipple, config);
                try
                {
                    Assert.AreEqual("水波涟漪设置", window.Title);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(60.0, slider.Value);
                    foreach (var name in new[] { "panel_Expansion", "panel_InkFluid", "panel_NeonRibbon", "panel_Trail", "panel_Following" })
                        Assert.AreEqual(Visibility.Collapsed, ((StackPanel)window.FindName(name)).Visibility);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    slider.Value = 20;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.6, config.MouseEffectSetting.WaterRippleOpacity);
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
                    var window = new MouseEffectSettingWindow(MouseEffectType.WaterRipple, config);
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
                    var restored = ConfigHelper.LoadMainConfig();
                    Assert.AreEqual(0.25, restored.MouseEffectSetting.WaterRippleOpacity);
                    Assert.AreEqual(0.6, restored.MouseEffectSetting.NeonRibbonOpacity);
                    Assert.AreEqual(0.4, restored.MouseEffectSetting.InkFluidOpacity);
                    Assert.IsTrue(restored.MouseEffectSetting.IsEnabled);
                    Assert.AreEqual(10, restored.MouseEffectSetting.SelectedEffectType);
                    var reopened = new MouseEffectSettingWindow(MouseEffectType.WaterRipple, restored);
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
