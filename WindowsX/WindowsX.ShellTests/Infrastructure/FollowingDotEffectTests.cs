using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WindowsX.Config.Helper;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect;
using WindowsX.Shell.Infrastructure.MouseEffect.Contracts;
using WindowsX.Shell.Windows;

namespace WindowsX.ShellTests.Infrastructure
{
    [TestClass]
    [DoNotParallelize]
    public class FollowingDotEffectTests
    {
        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{\"FireworksOpacity\":0.42}}")]
        public void OldConfigurationsUseDefaultFollowingGradient(string json)
        {
            var settings = JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting;
            Assert.AreEqual("#0080FF", settings.FollowingColor);
            Assert.AreEqual(0.5, settings.FollowingCenterOpacity);
            Assert.AreEqual(0.0, settings.FollowingEdgeOpacity);
            Assert.AreEqual(30.0, settings.FollowingSize);
            Assert.AreEqual(0.0, settings.FollowingDelayMilliseconds);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("Red")]
        [DataRow("#123")]
        [DataRow("#800080FF")]
        [DataRow("#GG0080")]
        [DataRow("0080FF")]
        public void InvalidColorsFallBackToDefault(string? color)
        {
            Assert.IsFalse(MouseEffectSetting.IsValidFollowingColor(color!));
            var settings = new MouseEffectSetting { FollowingColor = color! };
            Assert.AreEqual(MouseEffectSetting.DefaultFollowingColor, settings.FollowingColor);
        }

        [TestMethod]
        public void ColorAndGradientSurviveJsonRoundTrip()
        {
            var settings = new MouseEffectSetting
            {
                FollowingColor = "  #aa11ff  ",
                FollowingCenterOpacity = 0.8,
                FollowingEdgeOpacity = 0.2,
                FollowingSize = 80,
                FollowingDelayMilliseconds = 250
            };
            Assert.IsTrue(MouseEffectSetting.IsValidFollowingColor(settings.FollowingColor));
            var restored = JsonConvert.DeserializeObject<MouseEffectSetting>(JsonConvert.SerializeObject(settings))!;
            Assert.AreEqual("#AA11FF", restored.FollowingColor);
            Assert.AreEqual(0.8, restored.FollowingCenterOpacity);
            Assert.AreEqual(0.2, restored.FollowingEdgeOpacity);
            Assert.AreEqual(80.0, restored.FollowingSize);
            Assert.AreEqual(250.0, restored.FollowingDelayMilliseconds);
        }

        [DataTestMethod]
        [DataRow(-1.0, 10.0, 0.0)]
        [DataRow(5000.0, 120.0, 1000.0)]
        [DataRow(60.0, 60.0, 60.0)]
        [DataRow(double.NaN, 30.0, 0.0)]
        [DataRow(double.PositiveInfinity, 30.0, 0.0)]
        [DataRow(double.NegativeInfinity, 30.0, 0.0)]
        public void SizeAndDelayValuesAreNormalized(double value, double size, double delay)
        {
            var settings = new MouseEffectSetting { FollowingSize = value, FollowingDelayMilliseconds = value };
            Assert.AreEqual(size, settings.FollowingSize);
            Assert.AreEqual(delay, settings.FollowingDelayMilliseconds);
        }

        [DataTestMethod]
        [DataRow(10.0)]
        [DataRow(30.0)]
        [DataRow(120.0)]
        public void DrawingUsesConfiguredDiameter(double diameter)
        {
            RunSta(() =>
            {
                var effect = new FollowingDotEffect(new Point(100, 100), new MouseEffectSetting { FollowingSize = diameter });
                var ellipse = (EllipseGeometry)GetDrawing(effect).Geometry;
                Assert.AreEqual(diameter / 2, ellipse.RadiusX);
                Assert.AreEqual(diameter / 2, ellipse.RadiusY);
                Assert.AreEqual(new Point(100, 100), ellipse.Center);
            });
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0, 0.0)]
        [DataRow(2.0, 1.0, 1.0)]
        [DataRow(double.NaN, 0.5, 0.0)]
        [DataRow(double.PositiveInfinity, 0.5, 0.0)]
        [DataRow(double.NegativeInfinity, 0.5, 0.0)]
        public void InvalidOpacityValuesAreNormalized(double value, double center, double edge)
        {
            var settings = new MouseEffectSetting { FollowingCenterOpacity = value, FollowingEdgeOpacity = value };
            Assert.AreEqual(center, settings.FollowingCenterOpacity);
            Assert.AreEqual(edge, settings.FollowingEdgeOpacity);
        }

        [DataTestMethod]
        [DataRow(1.0, 0.0)]
        [DataRow(0.0, 1.0)]
        [DataRow(0.5, 0.5)]
        [DataRow(0.0, 0.0)]
        public void DrawingUsesConfiguredGradientWithoutChangingGeometry(double center, double edge)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting
                {
                    FollowingColor = "#32CD32",
                    FollowingCenterOpacity = center,
                    FollowingEdgeOpacity = edge
                };
                var effect = new FollowingDotEffect(new Point(32, 32), settings);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                Assert.IsFalse(effect.IsHitTestVisible);
                var drawing = GetDrawing(effect);
                var ellipse = (EllipseGeometry)drawing.Geometry;
                Assert.AreEqual(new Point(32, 32), ellipse.Center);
                Assert.AreEqual(15.0, ellipse.RadiusX);
                Assert.AreEqual(15.0, ellipse.RadiusY);
                var brush = (RadialGradientBrush)drawing.Brush;
                Assert.IsTrue(brush.IsFrozen);
                Assert.AreEqual(new Point(0.5, 0.5), brush.Center);
                Assert.AreEqual(brush.Center, brush.GradientOrigin);
                Assert.AreEqual(0.5, brush.RadiusX);
                Assert.AreEqual(0.5, brush.RadiusY);
                Assert.AreEqual(2, brush.GradientStops.Count);
                Assert.AreEqual(0.0, brush.GradientStops[0].Offset);
                Assert.AreEqual(1.0, brush.GradientStops[1].Offset);
                Assert.AreEqual(Color.FromArgb((byte)Math.Round(center * 255), 50, 205, 50), brush.GradientStops[0].Color);
                Assert.AreEqual(Color.FromArgb((byte)Math.Round(edge * 255), 50, 205, 50), brush.GradientStops[1].Color);

                effect.Measure(new Size(64, 64));
                effect.Arrange(new Rect(0, 0, 64, 64));
                var bitmap = new RenderTargetBitmap(64, 64, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(effect);
                var pixels = new byte[64 * 64 * 4];
                bitmap.CopyPixels(pixels, 64 * 4, 0);
                var centerAlpha = pixels[(32 * 64 + 32) * 4 + 3];
                var edgeAlpha = pixels[(32 * 64 + 44) * 4 + 3];
                if (center > edge) Assert.IsTrue(centerAlpha > edgeAlpha);
                else if (center < edge) Assert.IsTrue(centerAlpha < edgeAlpha);
                else Assert.AreEqual(centerAlpha, edgeAlpha);
                Assert.AreEqual((byte)0, pixels[3]);
            });
        }

        [TestMethod]
        public void WindowUsesUpdatedFollowingSettingsWithoutSwitchingEffect()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var window = new MouseEffectWindow(settings);
                try
                {
                    window.StartMouseEffect();
                    var create = typeof(MouseEffectWindow).GetMethod("CreateEffects", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    create.Invoke(window, null);
                    var canvas = (Canvas)window.FindName("canvas");
                    var effect = (FollowingDotEffect)canvas.Children[0];
                    ICursorEffect cursorEffect = effect;
                    cursorEffect.Update(new Point(20, 30), 0.016);
                    var visual = VisualTreeHelper.GetChild(effect, 0);
                    var brush = GetDrawing(effect).Brush;
                    for (var i = 0; i < 100; i++)
                    {
                        create.Invoke(window, null);
                        cursorEffect.Update(new Point(i, i), 0.016);
                    }
                    Assert.AreEqual(1, canvas.Children.Count);
                    Assert.AreSame(effect, canvas.Children[0]);
                    Assert.AreSame(visual, VisualTreeHelper.GetChild(effect, 0));
                    Assert.AreSame(brush, GetDrawing(effect).Brush);
                    settings.FollowingColor = "#32CD32";
                    settings.FollowingCenterOpacity = 0.8;
                    settings.FollowingEdgeOpacity = 0.2;
                    settings.FollowingSize = 80;
                    cursorEffect.Update(new Point(120, 140), 0.016);
                    var drawing = GetDrawing(effect);
                    var ellipse = (EllipseGeometry)drawing.Geometry;
                    Assert.AreEqual(new Point(120, 140), ellipse.Center);
                    Assert.AreEqual(40.0, ellipse.RadiusX);
                    var gradient = (RadialGradientBrush)drawing.Brush;
                    Assert.IsTrue(gradient.IsFrozen);
                    Assert.AreEqual(Color.FromArgb(204, 50, 205, 50), gradient.GradientStops[0].Color);
                    Assert.AreEqual(Color.FromArgb(51, 50, 205, 50), gradient.GradientStops[1].Color);
                    Assert.AreSame(visual, VisualTreeHelper.GetChild(effect, 0));
                }
                finally { window.Close(); }
            });
        }

        [TestMethod]
        public void ClearKeepsVisualAndAllowsRestart()
        {
            RunSta(() =>
            {
                var effect = new FollowingDotEffect();
                ICursorEffect cursorEffect = effect;
                var visual = (DrawingVisual)VisualTreeHelper.GetChild(effect, 0);
                Assert.IsTrue(visual.Drawing == null || visual.Drawing.Bounds.IsEmpty);
                cursorEffect.Update(new Point(20, 30), 0.016);
                Assert.IsFalse(visual.Drawing.Bounds.IsEmpty);
                cursorEffect.Clear();
                cursorEffect.Clear();
                Assert.IsTrue(visual.Drawing == null || visual.Drawing.Bounds.IsEmpty);
                cursorEffect.Update(new Point(40, 50), 0.016);
                Assert.AreEqual(new Point(40, 50), ((EllipseGeometry)GetDrawing(effect).Geometry).Center);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                Assert.AreSame(visual, VisualTreeHelper.GetChild(effect, 0));
            });
        }

        [TestMethod]
        public void InvalidUpdatesPreserveLastDrawing()
        {
            RunSta(() =>
            {
                var effect = new FollowingDotEffect(new Point(20, 30));
                foreach (var elapsed in new[] { 0.0, -1.0, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
                    effect.Update(new Point(100, 100), elapsed);
                foreach (var coordinate in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
                {
                    effect.Update(new Point(coordinate, 100), 0.016);
                    effect.Update(new Point(100, coordinate), 0.016);
                }
                Assert.AreEqual(new Point(20, 30), ((EllipseGeometry)GetDrawing(effect).Geometry).Center);
            });
        }

        [DataTestMethod]
        [DataRow("switch")]
        [DataRow("stop")]
        [DataRow("close")]
        public void WindowLifecycleClearsFollowingInstance(string action)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                try
                {
                    window.StartMouseEffect();
                    typeof(MouseEffectWindow).GetMethod("CreateEffects", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(window, null);
                    var canvas = (Canvas)window.FindName("canvas");
                    var effect = (FollowingDotEffect)canvas.Children[0];
                    effect.Update(new Point(20, 30), 0.016);
                    var visual = (DrawingVisual)VisualTreeHelper.GetChild(effect, 0);
                    if (action == "switch") window.UpdateMouseEffectType(MouseEffectType.NeonRibbon);
                    else if (action == "stop") window.StopMouseEffect();
                    else window.Close();
                    Assert.AreEqual(0, canvas.Children.Count);
                    Assert.IsTrue(visual.Drawing == null || visual.Drawing.Bounds.IsEmpty);
                    Assert.IsNull(typeof(MouseEffectWindow).GetField("cursorEffect", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(window));
                }
                finally { window.Close(); }
            });
        }

        [TestMethod]
        public void DialogValidatesAndPreviewsWithoutChangingSettings()
        {
            RunSta(() =>
            {
                var config = new MainConfig();
                var window = new MouseEffectSettingWindow(MouseEffectType.FollowingDot, config);
                try
                {
                    Assert.AreEqual("跟随设置", window.Title);
                    Assert.AreEqual(Visibility.Visible, ((StackPanel)window.FindName("panel_Following")).Visibility);
                    Assert.AreEqual(Visibility.Collapsed, ((StackPanel)window.FindName("panel_ParticleOpacity")).Visibility);
                    Assert.AreEqual(Visibility.Collapsed, ((StackPanel)window.FindName("panel_Expansion")).Visibility);
                    var color = (TextBox)window.FindName("text_FollowingColor");
                    var center = (Slider)window.FindName("slider_FollowingCenterOpacity");
                    var edge = (Slider)window.FindName("slider_FollowingEdgeOpacity");
                    var size = (Slider)window.FindName("slider_FollowingSize");
                    var delay = (Slider)window.FindName("slider_FollowingDelay");
                    var preview = (Ellipse)window.FindName("ellipse_FollowingPreview");
                    var save = (Button)window.FindName("btn_Save");
                    Assert.AreEqual("#0080FF", color.Text);
                    Assert.AreEqual(50.0, center.Value);
                    Assert.AreEqual(0.0, edge.Value);
                    Assert.AreEqual(30.0, size.Value);
                    Assert.AreEqual(0.0, delay.Value);
                    color.Text = "#invalid";
                    Assert.IsFalse(save.IsEnabled);
                    Assert.AreEqual(Visibility.Visible, ((TextBlock)window.FindName("text_ColorError")).Visibility);
                    Assert.IsNull(preview.Fill);
                    color.Text = "#BA55D3";
                    center.Value = 80;
                    edge.Value = 20;
                    Assert.IsTrue(save.IsEnabled);
                    var brush = (RadialGradientBrush)preview.Fill;
                    Assert.AreEqual(Color.FromArgb(204, 186, 85, 211), brush.GradientStops[0].Color);
                    Assert.AreEqual(Color.FromArgb(51, 186, 85, 211), brush.GradientStops[1].Color);
                    var preset = ((StackPanel)color.Parent).Children.OfType<Button>().Single(button => (string)button.Tag == "#32CD32");
                    preset.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual("#32CD32", color.Text);
                    size.Value = 100;
                    delay.Value = 700;
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual("#0080FF", color.Text);
                    Assert.AreEqual(50.0, center.Value);
                    Assert.AreEqual(0.0, edge.Value);
                    Assert.AreEqual(30.0, size.Value);
                    Assert.AreEqual(0.0, delay.Value);
                    color.Text = "#FFFFFF";
                    center.Value = 100;
                    edge.Value = 50;
                    size.Value = 80;
                    delay.Value = 250;
                }
                finally { window.Close(); }
                Assert.AreEqual("#0080FF", config.MouseEffectSetting.FollowingColor);
                Assert.AreEqual(0.5, config.MouseEffectSetting.FollowingCenterOpacity);
                Assert.AreEqual(0.0, config.MouseEffectSetting.FollowingEdgeOpacity);
                Assert.AreEqual(30.0, config.MouseEffectSetting.FollowingSize);
                Assert.AreEqual(0.0, config.MouseEffectSetting.FollowingDelayMilliseconds);
            });
        }

        [TestMethod]
        public void DialogSavesFollowingSettingsWithoutChangingOtherEffects()
        {
            RunSta(() =>
            {
                var originalDirectory = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-FollowingTests-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    config.MouseEffectSetting.FireworksOpacity = 0.42;
                    config.MouseEffectSetting.WillowCatkinsOpacity = 0.67;
                    config.MouseEffectSetting.FireworksExpansion = 1.75;
                    var window = new MouseEffectSettingWindow(MouseEffectType.FollowingDot, config);
                    Exception? callbackFailure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            ((TextBox)window.FindName("text_FollowingColor")).Text = "#ba55d3";
                            ((Slider)window.FindName("slider_FollowingCenterOpacity")).Value = 80;
                            ((Slider)window.FindName("slider_FollowingEdgeOpacity")).Value = 20;
                            ((Slider)window.FindName("slider_FollowingSize")).Value = 80;
                            ((Slider)window.FindName("slider_FollowingDelay")).Value = 250;
                            window.UpdateLayout();
                            var preview = (Ellipse)window.FindName("ellipse_FollowingPreview");
                            Assert.AreEqual(80.0, preview.Width);
                            Assert.AreEqual(80.0, preview.Height);
                            ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        catch (Exception ex) { callbackFailure = ex; }
                        finally { window.Close(); }
                    }, DispatcherPriority.Loaded);
                    Assert.AreEqual(true, window.ShowDialog());
                    if (callbackFailure != null) Assert.Fail(callbackFailure.ToString());
                    var restored = ConfigHelper.LoadMainConfig().MouseEffectSetting;
                    Assert.AreEqual("#BA55D3", restored.FollowingColor);
                    Assert.AreEqual(0.8, restored.FollowingCenterOpacity);
                    Assert.AreEqual(0.2, restored.FollowingEdgeOpacity);
                    Assert.AreEqual(0.42, restored.FireworksOpacity);
                    Assert.AreEqual(0.67, restored.WillowCatkinsOpacity);
                    Assert.AreEqual(1.75, restored.FireworksExpansion);
                    Assert.AreEqual(80.0, restored.FollowingSize);
                    Assert.AreEqual(250.0, restored.FollowingDelayMilliseconds);
                    Assert.AreEqual("#BA55D3", config.MouseEffectSetting.FollowingColor);
                    var reopened = new MouseEffectSettingWindow(MouseEffectType.FollowingDot, new MainConfig { MouseEffectSetting = restored });
                    try
                    {
                        Assert.AreEqual(80.0, ((Slider)reopened.FindName("slider_FollowingSize")).Value);
                        Assert.AreEqual(250.0, ((Slider)reopened.FindName("slider_FollowingDelay")).Value);
                    }
                    finally { reopened.Close(); }
                }
                finally
                {
                    Environment.CurrentDirectory = originalDirectory;
                    System.IO.Directory.Delete(directory, true);
                }
            });
        }

        private static GeometryDrawing GetDrawing(FollowingDotEffect effect)
        {
            var visual = (DrawingVisual)VisualTreeHelper.GetChild(effect, 0);
            return visual.Drawing.Children.OfType<GeometryDrawing>().Single();
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
            if (failure != null)
                Assert.Fail(failure.ToString());
        }
    }
}
