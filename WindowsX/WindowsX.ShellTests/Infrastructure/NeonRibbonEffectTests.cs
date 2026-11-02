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
    public class NeonRibbonEffectTests
    {
        [TestMethod]
        public void TypeAndOldConfigurationRemainCompatible()
        {
            Assert.AreEqual(8, (int)MouseEffectType.InkFluid);
            Assert.AreEqual(9, (int)MouseEffectType.NeonRibbon);
            foreach (var json in new[] { "{}", "{\"MouseEffectSetting\":null}", "{\"MouseEffectSetting\":{\"InkFluidOpacity\":0.4}}" })
                Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.NeonRibbonOpacity);
            var config = new MainConfig();
            config.MouseEffectSetting.SelectedEffectType = (int)MouseEffectType.NeonRibbon;
            config.MouseEffectSetting.IsEnabled = true;
            var restored = JsonConvert.DeserializeObject<MainConfig>(JsonConvert.SerializeObject(config))!;
            Assert.AreEqual(9, restored.MouseEffectSetting.SelectedEffectType);
            Assert.IsTrue(restored.MouseEffectSetting.IsEnabled);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double input, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { NeonRibbonOpacity = input }.NeonRibbonOpacity);
        }

        [TestMethod]
        public void RibbonHasSmoothLayeredGradientsAndContinuousPixels()
        {
            RunSta(() =>
            {
                var effect = new NeonRibbonCursorEffect();
                effect.Measure(new Size(640, 480));
                effect.Arrange(new Rect(0, 0, 640, 480));
                effect.Update(new Point(100, 200), 0.016);
                effect.Update(new Point(280, 200), 0.016);
                var drawings = Geometries(Drawing(effect)).ToArray();
                Assert.AreEqual(4, drawings.Length);
                Assert.IsTrue(drawings.All(d => d.Geometry.IsFrozen && d.Pen.IsFrozen));
                Assert.IsTrue(drawings.All(d => d.Pen.Brush is LinearGradientBrush));
                Assert.IsTrue(drawings.All(d => d.Geometry is StreamGeometry));
                Assert.IsTrue(drawings.All(d => PathGeometry.CreateFromGeometry(d.Geometry).Figures.Count == 1));
                Assert.IsTrue(drawings.All(d => d.Pen.StartLineCap == PenLineCap.Flat && d.Pen.EndLineCap == PenLineCap.Flat));
                Assert.IsTrue(drawings[0].Pen.Thickness > drawings[3].Pen.Thickness);
                Assert.AreNotEqual(((LinearGradientBrush)drawings[0].Pen.Brush).GradientStops[0].Color,
                    ((LinearGradientBrush)drawings[1].Pen.Brush).GradientStops.Last().Color);
                var pixels = Pixels(effect);
                for (var x = 101; x < 280; x++) Assert.IsTrue(pixels[(200 * 640 + x) * 4 + 3] > 0);
                Assert.AreEqual((byte)0, pixels[3]);
                Assert.IsTrue(NeonRibbonCursorEffect.PreviewImage.IsFrozen);
            });
        }

        [TestMethod]
        public void NodesRemainBoundedAndExpireWithoutStationaryEmission()
        {
            RunSta(() =>
            {
                var effect = new NeonRibbonCursorEffect();
                Assert.IsFalse(effect.IsHitTestVisible);
                for (var i = 0; i < 250; i++) effect.Update(new Point(100 + i * 3, 200), 0.001);
                Assert.AreEqual(NeonRibbonCursorEffect.MaxNodes, effect.NodeCount);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                var point = new Point(100 + 249 * 3, 200);
                effect.Update(point, 2);
                effect.Update(point, 0.016);
                Assert.AreEqual(0, effect.NodeCount);
                Assert.IsFalse(Geometries(Drawing(effect)).Any());
                effect.Clear();
                effect.Clear();
                effect.Update(point, 0.016);
                Assert.AreEqual(1, effect.NodeCount);
                Assert.AreEqual(0, Geometries(Drawing(effect)).Count());
            });
        }

        [TestMethod]
        public void InvalidUpdatesAreIgnored()
        {
            RunSta(() =>
            {
                var effect = new NeonRibbonCursorEffect();
                foreach (var elapsed in new[] { 0.0, -1, double.NaN, double.PositiveInfinity }) effect.Update(new Point(200, 200), elapsed);
                effect.Update(new Point(double.NaN, 100), 0.016);
                effect.Update(new Point(100, double.PositiveInfinity), 0.016);
                Assert.AreEqual(0, effect.NodeCount);
            });
        }

        [TestMethod]
        public void RunningRibbonUsesOpacityAndFades()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new NeonRibbonCursorEffect(settings);
                effect.Measure(new Size(640, 480));
                effect.Arrange(new Rect(0, 0, 640, 480));
                var point = new Point(200, 200);
                effect.Update(new Point(100, 200), 0.016);
                effect.Update(point, 0.016);
                var original = Alpha(Pixels(effect));
                effect.Update(point, 0.2);
                Assert.IsTrue(Alpha(Pixels(effect)) < original);
                settings.NeonRibbonOpacity = 0;
                effect.Update(point, 0.001);
                Assert.AreEqual(0L, Alpha(Pixels(effect)));
                settings.NeonRibbonOpacity = 0.5;
                effect.Update(point, 0.001);
                Assert.IsTrue(Alpha(Pixels(effect)) > 0);
                Assert.AreEqual(1.0, settings.InkFluidOpacity);
            });
        }

        [DataTestMethod]
        [DataRow("switch")]
        [DataRow("stop")]
        [DataRow("close")]
        [DataRow("start")]
        public void WindowClearsRibbon(string action)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                try
                {
                    window.UpdateMouseEffectType(MouseEffectType.NeonRibbon);
                    var effect = new NeonRibbonCursorEffect();
                    effect.Update(new Point(100, 100), 0.016);
                    var field = typeof(MouseEffectWindow).GetField("neonRibbonEffect", BindingFlags.Instance | BindingFlags.NonPublic)!;
                    field.SetValue(window, effect);
                    var canvas = (Canvas)window.FindName("canvas");
                    canvas.Children.Add(effect);
                    if (action == "switch") window.UpdateMouseEffectType(MouseEffectType.InkFluid);
                    else if (action == "stop") window.StopMouseEffect();
                    else if (action == "start") window.StartMouseEffect();
                    else window.Close();
                    Assert.AreEqual(0, effect.NodeCount);
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
                config.MouseEffectSetting.NeonRibbonOpacity = 0.4;
                var window = new MouseEffectSettingWindow(MouseEffectType.NeonRibbon, config);
                try
                {
                    Assert.AreEqual("霓虹缎带设置", window.Title);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(40.0, slider.Value);
                    Assert.AreEqual(Visibility.Collapsed, ((StackPanel)window.FindName("panel_InkFluid")).Visibility);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    slider.Value = 20;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.4, config.MouseEffectSetting.NeonRibbonOpacity);
            });
        }

        [TestMethod]
        public void DialogSavesIndependentOpacity()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-NeonTests-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    config.MouseEffectSetting.InkFluidOpacity = 0.6;
                    config.MouseEffectSetting.IsEnabled = true;
                    config.MouseEffectSetting.SelectedEffectType = 9;
                    var window = new MouseEffectSettingWindow(MouseEffectType.NeonRibbon, config);
                    Exception? failure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            ((Slider)window.FindName("slider_Opacity")).Value = 25;
                            ((TextBox)window.FindName("text_NeonRibbonColor1")).Text = " #ff0000 ";
                            ((TextBox)window.FindName("text_NeonRibbonColor2")).Text = "#00ff00";
                            ((TextBox)window.FindName("text_NeonRibbonColor3")).Text = "#0000ff";
                            ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        catch (Exception ex) { failure = ex; }
                        finally { window.Close(); }
                    }, DispatcherPriority.Loaded);
                    Assert.AreEqual(true, window.ShowDialog());
                    if (failure != null) Assert.Fail(failure.ToString());
                    var restored = ConfigHelper.LoadMainConfig().MouseEffectSetting;
                    Assert.AreEqual(0.25, restored.NeonRibbonOpacity);
                    Assert.AreEqual("#FF0000", restored.NeonRibbonColor1);
                    Assert.AreEqual("#00FF00", restored.NeonRibbonColor2);
                    Assert.AreEqual("#0000FF", restored.NeonRibbonColor3);
                    Assert.AreEqual(MouseEffectSetting.DefaultInkFluidColor1, restored.InkFluidColor1);
                    Assert.AreEqual(0.6, restored.InkFluidOpacity);
                    Assert.IsTrue(restored.IsEnabled);
                    Assert.AreEqual(9, restored.SelectedEffectType);
                }
                finally
                {
                    Environment.CurrentDirectory = original;
                    System.IO.Directory.Delete(directory, true);
                }
            });
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("#123")]
        [DataRow("#GGFFFF")]
        [DataRow("#8000FFFF")]
        public void InvalidColorsUseDefaults(string? color)
        {
            var settings = new MouseEffectSetting { NeonRibbonColor1 = color, NeonRibbonColor2 = color, NeonRibbonColor3 = color };
            Assert.AreEqual("#00F0FF", settings.NeonRibbonColor1);
            Assert.AreEqual("#9E44FF", settings.NeonRibbonColor2);
            Assert.AreEqual("#FF37B4", settings.NeonRibbonColor3);
            var restored = JsonConvert.DeserializeObject<MouseEffectSetting>("{}")!;
            Assert.AreEqual(settings.NeonRibbonColor1, restored.NeonRibbonColor1);
            Assert.AreEqual(settings.NeonRibbonColor2, restored.NeonRibbonColor2);
            Assert.AreEqual(settings.NeonRibbonColor3, restored.NeonRibbonColor3);
        }

        [TestMethod]
        public void RunningColorsUpdateAndClosedPathHasNoDots()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new NeonRibbonCursorEffect(settings);
                foreach (var point in new[] { new Point(100, 100), new Point(200, 200), new Point(200, 100), new Point(100, 100) })
                    effect.Update(point, 0.016);
                settings.NeonRibbonColor1 = "#FF0000";
                settings.NeonRibbonColor2 = "#00FF00";
                settings.NeonRibbonColor3 = "#0000FF";
                effect.Update(new Point(100, 100), 0.016);
                var drawings = Geometries(Drawing(effect)).ToArray();
                Assert.AreEqual(4, drawings.Length);
                var brush = (LinearGradientBrush)drawings[2].Pen.Brush;
                Assert.AreNotEqual(brush.StartPoint, brush.EndPoint);
                CollectionAssert.AreEqual(new[] { Colors.Red, Colors.Lime, Colors.Blue }, brush.GradientStops.Select(s => s.Color).ToArray());
                Assert.IsTrue(drawings.All(d => d.Geometry is StreamGeometry));
            });
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        public void DialogValidatesPreviewsAndResetsColorsWithoutSaving(int index)
        {
            RunSta(() =>
            {
                var config = new MainConfig();
                config.MouseEffectSetting.NeonRibbonColor1 = "#112233";
                var window = new MouseEffectSettingWindow(MouseEffectType.NeonRibbon, config);
                try
                {
                    var input = (TextBox)window.FindName("text_NeonRibbonColor" + index);
                    var preview = (Image)window.FindName("image_NeonRibbonPreview");
                    var save = (Button)window.FindName("btn_Save");
                    Assert.IsNotNull(preview.Source);
                    input.Text = "invalid";
                    Assert.IsFalse(save.IsEnabled);
                    Assert.IsNull(preview.Source);
                    input.Text = " #abcdef ";
                    Assert.IsTrue(save.IsEnabled);
                    Assert.IsNotNull(preview.Source);
                    var drawing = (DrawingImage)preview.Source;
                    var colors = ((LinearGradientBrush)Geometries(drawing.Drawing).First(d => d.Pen != null).Pen.Brush).GradientStops;
                    Assert.AreEqual(Color.FromRgb(171, 205, 239), colors[index - 1].Color);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(MouseEffectSetting.DefaultNeonRibbonColor1, ((TextBox)window.FindName("text_NeonRibbonColor1")).Text);
                    Assert.AreEqual(MouseEffectSetting.DefaultNeonRibbonColor2, ((TextBox)window.FindName("text_NeonRibbonColor2")).Text);
                    Assert.AreEqual(MouseEffectSetting.DefaultNeonRibbonColor3, ((TextBox)window.FindName("text_NeonRibbonColor3")).Text);
                }
                finally { window.Close(); }
                Assert.AreEqual("#112233", config.MouseEffectSetting.NeonRibbonColor1);
                Assert.AreEqual(MouseEffectSetting.DefaultNeonRibbonColor2, config.MouseEffectSetting.NeonRibbonColor2);
                Assert.AreEqual(MouseEffectSetting.DefaultNeonRibbonColor3, config.MouseEffectSetting.NeonRibbonColor3);
            });
        }

        private static DrawingGroup Drawing(NeonRibbonCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
        private static IEnumerable<GeometryDrawing> Geometries(Drawing drawing)
        {
            if (drawing is GeometryDrawing geometry) yield return geometry;
            else if (drawing is DrawingGroup group)
                foreach (var child in group.Children)
                    foreach (var item in Geometries(child)) yield return item;
        }
        private static byte[] Pixels(NeonRibbonCursorEffect effect)
        {
            var bitmap = new RenderTargetBitmap(640, 480, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(effect);
            var pixels = new byte[640 * 480 * 4];
            bitmap.CopyPixels(pixels, 640 * 4, 0);
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
