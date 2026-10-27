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
    public class CursorTrailEffectTests
    {
        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{\"FollowingSize\":80}}")]
        public void OldConfigurationsUseDefaultTrailSettings(string json)
        {
            var settings = JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting;
            Assert.AreEqual("#0080FF", settings.TrailColor);
            Assert.AreEqual(20.0, settings.TrailSize);
            Assert.AreEqual(0.6, settings.TrailHeadOpacity);
            Assert.AreEqual(0.0, settings.TrailTailOpacity);
        }

        [DataTestMethod]
        [DataRow(-1.0, 4.0, 0.0, 0.0)]
        [DataRow(999.0, 120.0, 1.0, 1.0)]
        [DataRow(double.NaN, 20.0, 0.6, 0.0)]
        [DataRow(double.PositiveInfinity, 20.0, 0.6, 0.0)]
        [DataRow(double.NegativeInfinity, 20.0, 0.6, 0.0)]
        public void InvalidValuesAreNormalized(double value, double size, double head, double tail)
        {
            var settings = new MouseEffectSetting { TrailSize = value, TrailHeadOpacity = value, TrailTailOpacity = value };
            Assert.AreEqual(size, settings.TrailSize);
            Assert.AreEqual(head, settings.TrailHeadOpacity);
            Assert.AreEqual(tail, settings.TrailTailOpacity);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("#GG0080")]
        [DataRow("#800080FF")]
        public void InvalidColorUsesDefault(string? color)
        {
            Assert.AreEqual("#0080FF", new MouseEffectSetting { TrailColor = color! }.TrailColor);
        }

        [DataTestMethod]
        [DataRow(0.8, 0.2, 4.0)]
        [DataRow(0.2, 0.8, 120.0)]
        [DataRow(0.0, 0.0, 20.0)]
        public void ConnectedCurvesUseConfiguredEndpointsColorAndSize(double head, double tail, double size)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting { TrailColor = "#32CD32", TrailSize = size, TrailHeadOpacity = head, TrailTailOpacity = tail };
                var effect = new CursorTrailEffect(new Point(20, 100), settings);
                effect.Update(new Point(100, 20), 0.016);
                effect.Update(new Point(180, 100), 0.016);
                var drawings = GetDrawings(effect);
                Assert.AreEqual(3, drawings.Length);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                Assert.IsFalse(effect.IsHitTestVisible);
                Point? previousEnd = null;
                foreach (var drawing in drawings)
                {
                    Assert.AreEqual(size, drawing.Pen.Thickness);
                    Assert.IsInstanceOfType(drawing.Geometry, typeof(StreamGeometry));
                    var figure = PathGeometry.CreateFromGeometry(drawing.Geometry).Figures.Single();
                    if (previousEnd.HasValue)
                        Assert.IsTrue((previousEnd.Value - figure.StartPoint).Length < 0.001);
                    var segment = figure.Segments.Last();
                    previousEnd = segment switch
                    {
                        QuadraticBezierSegment quadratic => quadratic.Point2,
                        PolyQuadraticBezierSegment quadratic => quadratic.Points.Last(),
                        BezierSegment bezier => bezier.Point3,
                        PolyBezierSegment bezier => bezier.Points.Last(),
                        _ => throw new InvalidOperationException("Expected a smooth curve segment.")
                    };
                }
                Assert.AreEqual(new Point(180, 100), previousEnd!.Value);
                var first = (LinearGradientBrush)drawings[0].Pen.Brush;
                var last = (LinearGradientBrush)drawings.Last().Pen.Brush;
                Assert.AreEqual(Color.FromArgb((byte)Math.Round(tail * 255), 50, 205, 50), first.GradientStops[0].Color);
                Assert.AreEqual(Color.FromArgb((byte)Math.Round(head * 255), 50, 205, 50), last.GradientStops[1].Color);
                Assert.AreEqual(PenLineCap.Round, drawings[0].Pen.StartLineCap);
                Assert.AreEqual(PenLineCap.Round, drawings.Last().Pen.EndLineCap);
            });
        }

        [TestMethod]
        public void FastMovementDrawsContinuousPixelsBetweenSamples()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting { TrailHeadOpacity = 1, TrailTailOpacity = 1, TrailSize = 20 };
                var effect = new CursorTrailEffect(new Point(20, 100), settings);
                effect.Update(new Point(400, 100), 0.016);
                effect.Measure(new Size(440, 200));
                effect.Arrange(new Rect(0, 0, 440, 200));
                var bitmap = new RenderTargetBitmap(440, 200, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(effect);
                var pixels = new byte[440 * 200 * 4];
                bitmap.CopyPixels(pixels, 440 * 4, 0);
                for (var x = 20; x <= 400; x++)
                    Assert.IsTrue(pixels[(100 * 440 + x) * 4 + 3] > 0, "Unexpected gap at x=" + x);
            });
        }

        [TestMethod]
        public void RunningTrailUsesChangedSettingsAndFadesWhenStationary()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new CursorTrailEffect(new Point(20, 100), settings);
                effect.Update(new Point(100, 100), 0.016);
                settings.TrailColor = "#FF4545";
                settings.TrailSize = 60;
                settings.TrailHeadOpacity = 1;
                settings.TrailTailOpacity = 0.4;
                effect.Update(new Point(100, 100), 0.1);
                var drawing = GetDrawings(effect).Last();
                Assert.AreEqual(60.0, drawing.Pen.Thickness);
                var color = ((LinearGradientBrush)drawing.Pen.Brush).GradientStops[1].Color;
                Assert.AreEqual((byte)204, color.A);
                Assert.AreEqual((byte)255, color.R);
                Assert.AreEqual((byte)69, color.G);
                effect.Update(new Point(100, 100), 1);
                Assert.AreEqual(0, effect.NodeCount);
                Assert.AreEqual(0, GetDrawings(effect).Length);
                effect.Update(new Point(100, 100), 0.016);
                Assert.AreEqual(0, effect.NodeCount);
                effect.Clear();
                effect.Update(new Point(100, 100), 0.016);
                Assert.AreEqual(1, effect.NodeCount);
            });
        }

        [TestMethod]
        public void NodesStayBoundedAndInvalidUpdatesDoNotAddNodes()
        {
            RunSta(() =>
            {
                var effect = new CursorTrailEffect(new Point(20, 20));
                for (var i = 0; i < CursorTrailEffect.MaxNodes + 20; i++)
                {
                    effect.Update(new Point(i, i % 2 == 0 ? 20 : 40), 0.0001);
                    Assert.IsTrue(effect.NodeCount <= CursorTrailEffect.MaxNodes);
                }
                effect.Clear();
                foreach (var elapsed in new[] { 0.0, -1.0, double.NaN, double.PositiveInfinity })
                    effect.Update(new Point(), elapsed);
                effect.Update(new Point(double.NaN, 0), 0.016);
                Assert.AreEqual(0, effect.NodeCount);
            });
        }

        [DataTestMethod]
        [DataRow("switch")]
        [DataRow("stop")]
        [DataRow("close")]
        public void WindowClearsRetainedTrail(string action)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                var effect = new CursorTrailEffect(new Point(20, 20));
                try
                {
                    window.UpdateMouseEffectType(MouseEffectType.CursorTrail);
                    var field = typeof(MouseEffectWindow).GetField("trailEffect", BindingFlags.Instance | BindingFlags.NonPublic)!;
                    field.SetValue(window, effect);
                    var canvas = (Canvas)window.FindName("canvas");
                    canvas.Children.Add(effect);
                    if (action == "switch") window.UpdateMouseEffectType(MouseEffectType.FollowingDot);
                    else if (action == "stop") window.StopMouseEffect();
                    else window.Close();
                    Assert.AreEqual(0, effect.NodeCount);
                    Assert.AreEqual(0, canvas.Children.Count);
                    Assert.IsNull(field.GetValue(window));
                }
                finally { window.Close(); }
            });
        }

        [TestMethod]
        public void DialogPreviewsValidatesResetsAndDiscardsChanges()
        {
            RunSta(() =>
            {
                var config = new MainConfig();
                var window = new MouseEffectSettingWindow(MouseEffectType.CursorTrail, config);
                try
                {
                    Assert.AreEqual("轨迹设置", window.Title);
                    Assert.AreEqual(Visibility.Visible, ((StackPanel)window.FindName("panel_Trail")).Visibility);
                    Assert.AreEqual(Visibility.Collapsed, ((StackPanel)window.FindName("panel_Following")).Visibility);
                    Assert.AreEqual(Visibility.Collapsed, ((StackPanel)window.FindName("panel_ParticleOpacity")).Visibility);
                    var color = (TextBox)window.FindName("text_TrailColor");
                    var head = (Slider)window.FindName("slider_TrailHeadOpacity");
                    var tail = (Slider)window.FindName("slider_TrailTailOpacity");
                    var size = (Slider)window.FindName("slider_TrailSize");
                    var preview = (Image)window.FindName("image_TrailPreview");
                    var save = (Button)window.FindName("btn_Save");
                    color.Text = "invalid";
                    Assert.IsFalse(save.IsEnabled);
                    Assert.IsNull(preview.Source);
                    color.Text = "#32CD32";
                    head.Value = 80;
                    tail.Value = 20;
                    size.Value = 60;
                    Assert.IsTrue(save.IsEnabled);
                    var drawings = ((DrawingGroup)((DrawingImage)preview.Source).Drawing).Children.OfType<GeometryDrawing>().ToArray();
                    Assert.AreEqual(60.0, drawings[0].Pen.Thickness);
                    Assert.AreEqual((byte)204, ((LinearGradientBrush)drawings.Last().Pen.Brush).GradientStops[1].Color.A);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual("#0080FF", color.Text);
                    Assert.AreEqual(60.0, head.Value);
                    Assert.AreEqual(0.0, tail.Value);
                    Assert.AreEqual(20.0, size.Value);
                    color.Text = "#FFFFFF";
                    head.Value = 100;
                    tail.Value = 50;
                    size.Value = 90;
                }
                finally { window.Close(); }
                Assert.AreEqual("#0080FF", config.MouseEffectSetting.TrailColor);
                Assert.AreEqual(0.6, config.MouseEffectSetting.TrailHeadOpacity);
                Assert.AreEqual(0.0, config.MouseEffectSetting.TrailTailOpacity);
                Assert.AreEqual(20.0, config.MouseEffectSetting.TrailSize);
            });
        }

        [TestMethod]
        public void DialogSavesTrailWithoutChangingOtherEffects()
        {
            RunSta(() =>
            {
                var originalDirectory = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-TrailTests-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    config.MouseEffectSetting.FollowingSize = 80;
                    config.MouseEffectSetting.FireworksOpacity = 0.42;
                    config.MouseEffectSetting.WillowCatkinsOpacity = 0.67;
                    var window = new MouseEffectSettingWindow(MouseEffectType.CursorTrail, config);
                    Exception? callbackFailure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            ((TextBox)window.FindName("text_TrailColor")).Text = "#ba55d3";
                            ((Slider)window.FindName("slider_TrailHeadOpacity")).Value = 80;
                            ((Slider)window.FindName("slider_TrailTailOpacity")).Value = 20;
                            ((Slider)window.FindName("slider_TrailSize")).Value = 60;
                            ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        catch (Exception ex) { callbackFailure = ex; }
                        finally { window.Close(); }
                    }, DispatcherPriority.Loaded);
                    var result = window.ShowDialog();
                    if (callbackFailure != null) Assert.Fail(callbackFailure.ToString());
                    Assert.AreEqual(true, result);
                    var restored = ConfigHelper.LoadMainConfig();
                    Assert.AreEqual("#BA55D3", restored.MouseEffectSetting.TrailColor);
                    Assert.AreEqual(0.8, restored.MouseEffectSetting.TrailHeadOpacity);
                    Assert.AreEqual(0.2, restored.MouseEffectSetting.TrailTailOpacity);
                    Assert.AreEqual(60.0, restored.MouseEffectSetting.TrailSize);
                    Assert.AreEqual(80.0, restored.MouseEffectSetting.FollowingSize);
                    Assert.AreEqual(0.42, restored.MouseEffectSetting.FireworksOpacity);
                    Assert.AreEqual(0.67, restored.MouseEffectSetting.WillowCatkinsOpacity);
                    var reopened = new MouseEffectSettingWindow(MouseEffectType.CursorTrail, restored);
                    try
                    {
                        Assert.AreEqual(80.0, ((Slider)reopened.FindName("slider_TrailHeadOpacity")).Value);
                        Assert.AreEqual(20.0, ((Slider)reopened.FindName("slider_TrailTailOpacity")).Value);
                        Assert.AreEqual(60.0, ((Slider)reopened.FindName("slider_TrailSize")).Value);
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

        private static GeometryDrawing[] GetDrawings(CursorTrailEffect effect)
        {
            var visual = (DrawingVisual)VisualTreeHelper.GetChild(effect, 0);
            return visual.Drawing?.Children.OfType<GeometryDrawing>().ToArray() ?? Array.Empty<GeometryDrawing>();
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
