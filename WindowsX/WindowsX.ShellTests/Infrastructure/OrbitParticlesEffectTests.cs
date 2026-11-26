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
    public class OrbitParticlesEffectTests
    {
        [TestMethod]
        public void DefaultsAndIdentifiersRemainCompatible()
        {
            Assert.AreEqual(25, (int)MouseEffectType.BinaryTrail);
            Assert.AreEqual(26, (int)MouseEffectType.OrbitParticles);
            foreach (var json in new[] { "{}", "{\"MouseEffectSetting\":null}" })
                Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.OrbitParticlesOpacity);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { OrbitParticlesOpacity = value }.OrbitParticlesOpacity);
        }

        [TestMethod]
        public void StationaryParticlesOrbitInOppositeDirectionsAndReuseBrushes()
        {
            RunSta(() =>
            {
                var effect = new OrbitParticlesCursorEffect();
                var cursor = new Point(200, 200);
                effect.Update(cursor, 0.01);
                var first = Halos(effect);
                Assert.AreEqual(12, effect.ParticleCount);
                Assert.AreEqual(12, first.Length);
                Assert.AreEqual(36, Shapes(Drawing(effect)).Count());
                Assert.IsTrue(Shapes(Drawing(effect)).All(s => s.Brush.IsFrozen));
                Assert.AreEqual(3, first.Select(s => s.Brush).Distinct().Count());
                var before = first.Select(s => ((EllipseGeometry)s.Geometry).Center - cursor).ToArray();
                effect.Update(cursor, 0.1);
                var second = Halos(effect);
                for (var i = 0; i < 12; i++)
                {
                    var after = ((EllipseGeometry)second[i].Geometry).Center - cursor;
                    Assert.AreEqual(i < 6 ? 28 : 43, after.Length, 0.000001);
                    var cross = Vector.CrossProduct(before[i], after);
                    Assert.IsTrue(i < 6 ? cross > 0 : cross < 0);
                    Assert.AreSame(first[i].Brush, second[i].Brush);
                }
                Assert.IsFalse(effect.IsHitTestVisible);
                Assert.IsTrue(OrbitParticlesCursorEffect.PreviewImage.IsFrozen);
                Assert.AreEqual(new Rect(0, 0, 260, 170), OrbitParticlesCursorEffect.PreviewImage.Drawing.Bounds);
                Assert.AreEqual(12, Shapes(OrbitParticlesCursorEffect.PreviewImage.Drawing).Count(s => s.Brush is RadialGradientBrush));
            });
        }

        [TestMethod]
        public void CenterFollowsSmoothlyAndIsFrameRateIndependent()
        {
            RunSta(() =>
            {
                var single = new OrbitParticlesCursorEffect();
                var split = new OrbitParticlesCursorEffect();
                single.Update(new Point(100, 100), 0.01);
                split.Update(new Point(100, 100), 0.01);
                single.Update(new Point(300, 100), 0.1);
                for (var i = 0; i < 10; i++) split.Update(new Point(300, 100), 0.01);
                var center = Center(single);
                Assert.IsTrue(center.X > 100 && center.X < 300);
                Assert.AreEqual(100, center.Y, 0.000001);
                Assert.AreEqual(center.X, Center(split).X, 0.000001);
                single.Update(new Point(300, 100), 10);
                Assert.AreEqual(300, Center(single).X, 0.000001);
            });
        }

        [TestMethod]
        public void InvalidInputRapidMovementAndLongFramesStayBounded()
        {
            RunSta(() =>
            {
                var effect = new OrbitParticlesCursorEffect();
                foreach (var time in new[] { 0.0, -1, double.NaN, double.PositiveInfinity })
                    effect.Update(new Point(100, 100), time);
                effect.Update(new Point(double.NaN, 100), 0.01);
                effect.Update(new Point(100, double.PositiveInfinity), 0.01);
                Assert.AreEqual(0, effect.ParticleCount);
                for (var i = 0; i < 1000; i++) effect.Update(new Point(i % 2 * 10000, 100), 0.01);
                effect.Update(new Point(100, 100), double.MaxValue);
                Assert.AreEqual(12, effect.ParticleCount);
                Assert.AreEqual(36, Shapes(Drawing(effect)).Count());
                Assert.AreEqual(1, effect.TrailSampleCount);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                Assert.IsTrue(Halos(effect).All(s => double.IsFinite(((EllipseGeometry)s.Geometry).Center.X)));
                var visual = VisualTreeHelper.GetChild(effect, 0);
                effect.Clear();
                effect.Clear();
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.AreEqual(0, effect.TrailSampleCount);
                Assert.IsTrue(Drawing(effect) == null || Drawing(effect).Bounds.IsEmpty);
                effect.Update(new Point(500, 500), 0.01);
                Assert.AreEqual(12, effect.ParticleCount);
                Assert.AreEqual(500, Center(effect).X, 0.000001);
                Assert.AreSame(visual, VisualTreeHelper.GetChild(effect, 0));
            });
        }

        [TestMethod]
        public void OpacityControlsCoresHalosAndTailsLive()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new OrbitParticlesCursorEffect(settings);
                effect.Measure(new Size(400, 400));
                effect.Arrange(new Rect(0, 0, 400, 400));
                effect.Update(new Point(200, 200), 0.01);
                Assert.IsTrue(Alpha(effect) > 0);
                settings.OrbitParticlesOpacity = 0;
                effect.Update(new Point(200, 200), 0.01);
                Assert.AreEqual(0L, Alpha(effect));
                settings.OrbitParticlesOpacity = 0.5;
                effect.Update(new Point(200, 200), 0.01);
                Assert.IsTrue(Alpha(effect) > 0);
            });
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void WindowCreatesOneEffectAndClearsOnStopOrSwitch(bool switchEffect)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                try
                {
                    window.StartMouseEffect();
                    window.UpdateMouseEffectType(MouseEffectType.OrbitParticles);
                    var create = typeof(MouseEffectWindow).GetMethod("CreateEffects", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    create.Invoke(window, null);
                    create.Invoke(window, null);
                    var canvas = (Canvas)window.FindName("canvas");
                    Assert.AreEqual(1, canvas.Children.Count);
                    var effect = (OrbitParticlesCursorEffect)canvas.Children[0];
                    effect.Update(new Point(100, 100), 0.01);
                    if (switchEffect) window.UpdateMouseEffectType(MouseEffectType.BinaryTrail);
                    else window.StopMouseEffect();
                    Assert.AreEqual(0, effect.ParticleCount);
                    Assert.AreEqual(0, canvas.Children.Count);
                }
                finally { window.StopMouseEffect(); window.Close(); }
            });
        }

        [TestMethod]
        public void DialogResetAndCancelOnlyChangeDraft()
        {
            RunSta(() =>
            {
                var config = new MainConfig();
                config.MouseEffectSetting.OrbitParticlesOpacity = 0.6;
                var window = new MouseEffectSettingWindow(MouseEffectType.OrbitParticles, config);
                try
                {
                    Assert.AreEqual("环绕粒子设置", window.Title);
                    Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName("panel_BinaryTrail")).Visibility);
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_ParticleOpacity")).Visibility);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(60.0, slider.Value);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    slider.Value = 25;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.6, config.MouseEffectSetting.OrbitParticlesOpacity);
            });
        }

        [TestMethod]
        public void DialogPersistsOnlyOrbitOpacityAndKeepsSelection()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-Orbit-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.BinaryTrailOpacity = 0.4;
                    settings.WillowCatkinsOpacity = 0.7;
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)MouseEffectType.OrbitParticles;
                    var window = new MouseEffectSettingWindow(MouseEffectType.OrbitParticles, config);
                    Exception? failure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            ((Slider)window.FindName("slider_Opacity")).Value = 25;
                            ((Slider)window.FindName("slider_OrbitParticlesLayers")).Value = 3;
                            ((Slider)window.FindName("slider_OrbitParticlesRadius")).Value = 60;
                            ((Slider)window.FindName("slider_OrbitParticlesSpeed")).Value = 175;
                            ((TextBox)window.FindName("text_OrbitParticlesColor1")).Text = "#ff1234";
                            ((TextBox)window.FindName("text_OrbitParticlesColor2")).Text = "#12ff34";
                            ((TextBox)window.FindName("text_OrbitParticlesColor3")).Text = "#1234ff";
                            ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        catch (Exception ex) { failure = ex; }
                        finally { window.Close(); }
                    }, DispatcherPriority.Loaded);
                    Assert.AreEqual(true, window.ShowDialog());
                    if (failure != null) Assert.Fail(failure.ToString());
                    Assert.AreSame(settings, config.MouseEffectSetting);
                    var restored = ConfigHelper.LoadMainConfig().MouseEffectSetting;
                    Assert.AreEqual(0.25, restored.OrbitParticlesOpacity);
                    Assert.AreEqual(3, restored.OrbitParticlesLayers);
                    Assert.AreEqual(60.0, restored.OrbitParticlesRadius);
                    Assert.AreEqual(1.75, restored.OrbitParticlesSpeed);
                    Assert.AreEqual("#FF1234", restored.OrbitParticlesColor1);
                    Assert.AreEqual("#12FF34", restored.OrbitParticlesColor2);
                    Assert.AreEqual("#1234FF", restored.OrbitParticlesColor3);
                    Assert.AreEqual(0.4, restored.BinaryTrailOpacity);
                    Assert.AreEqual(0.7, restored.WillowCatkinsOpacity);
                    Assert.AreEqual(26, restored.SelectedEffectType);
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
        public void GlowHasLargerHaloAndWhiteCoreAndPreviewHasTrails()
        {
            RunSta(() =>
            {
                var effect = new OrbitParticlesCursorEffect();
                effect.Update(new Point(200, 200), 0.01);
                var shapes = Shapes(Drawing(effect)).ToArray();
                var halo = Halos(effect)[0];
                Assert.AreEqual(1.8 * 6, ((EllipseGeometry)halo.Geometry).RadiusX, 0.000001);
                Assert.AreEqual((byte)210, ((RadialGradientBrush)halo.Brush).GradientStops[0].Color.A);
                Assert.AreEqual(12, shapes.Count(s => s.Brush is SolidColorBrush brush && brush.Color == Colors.White));
                var preview = Shapes(OrbitParticlesCursorEffect.PreviewImage.Drawing).ToArray();
                Assert.IsTrue(preview.Any(s => s.Pen != null));
                Assert.IsTrue(preview.Where(s => s.Pen != null).All(s => s.Pen.IsFrozen));
            });
        }

        [TestMethod]
        public void TrailsConnectActualPastPositionsAndReuseFrozenPens()
        {
            RunSta(() =>
            {
                var effect = new OrbitParticlesCursorEffect();
                effect.Update(new Point(100, 100), 0.01);
                var past = Halos(effect).Select(s => ((EllipseGeometry)s.Geometry).Center).ToArray();
                effect.Update(new Point(250, 140), 0.03);
                var current = Halos(effect).Select(s => ((EllipseGeometry)s.Geometry).Center).ToArray();
                var strokes = Shapes(Drawing(effect)).Where(s => s.Pen != null).ToArray();
                Assert.AreEqual(24, strokes.Length);
                for (var i = 0; i < 12; i++)
                {
                    var line = (LineGeometry)strokes[i * 2].Geometry;
                    Assert.AreEqual(current[i], line.StartPoint);
                    Assert.AreEqual(past[i], line.EndPoint);
                    Assert.IsTrue(strokes[i * 2].Pen.Thickness > strokes[i * 2 + 1].Pen.Thickness);
                    Assert.IsTrue(strokes[i * 2].Pen.IsFrozen);
                    Assert.AreEqual(PenLineCap.Round, strokes[i * 2].Pen.EndLineCap);
                }
                effect.Update(new Point(250, 140), 0.0001);
                var next = Shapes(Drawing(effect)).Where(s => s.Pen != null).ToArray();
                Assert.IsTrue(next.Any(s => ReferenceEquals(s.Pen, strokes[0].Pen)));
            });
        }

        [TestMethod]
        public void TrailsFadeTaperAndExpireWithoutJoiningAcrossPause()
        {
            RunSta(() =>
            {
                var effect = new OrbitParticlesCursorEffect();
                var cursor = new Point(200, 200);
                effect.Update(cursor, 0.01);
                var initial = ((EllipseGeometry)Halos(effect)[0].Geometry).Center;
                for (var i = 0; i < 10; i++) effect.Update(cursor, 0.03);
                var groups = Groups(Drawing(effect)).Where(g => g.Children.OfType<GeometryDrawing>().Any(s => s.Pen != null)).ToArray();
                Assert.IsTrue(groups.Length > 12);
                Assert.IsTrue(groups.Min(g => g.Opacity) < groups.Max(g => g.Opacity));
                var widths = Shapes(Drawing(effect)).Where(s => s.Pen != null).Select(s => s.Pen.Thickness).Distinct().ToArray();
                Assert.IsTrue(widths.Length > 2);
                for (var i = 0; i < 8; i++) effect.Update(cursor, 0.03);
                Assert.IsFalse(Shapes(Drawing(effect)).Where(s => s.Pen != null).Any(s => ((LineGeometry)s.Geometry).EndPoint == initial));
                effect.Update(new Point(2000, 100), 0.5);
                Assert.IsFalse(Shapes(Drawing(effect)).Any(s => s.Pen != null));
                effect.Clear();
                effect.Update(cursor, 0.01);
                Assert.IsFalse(Shapes(Drawing(effect)).Any(s => s.Pen != null));
            });
        }

        [TestMethod]
        public void TrailsStayWithinSampleLengthAndDrawingBudgets()
        {
            RunSta(() =>
            {
                var effect = new OrbitParticlesCursorEffect();
                for (var i = 0; i < 600; i++)
                {
                    effect.Update(new Point(200 + i * 3, 200 + Math.Sin(i * 0.1) * 50), 0.01);
                    Assert.IsTrue(effect.TrailSampleCount <= OrbitParticlesCursorEffect.MaxTrailSamples);
                    var shapes = Shapes(Drawing(effect)).ToArray();
                    Assert.IsTrue(shapes.Length <= 36 + 12 * OrbitParticlesCursorEffect.MaxTrailSamples * 2);
                    var trailLength = shapes.Where(s => s.Pen != null).Sum(s =>
                    {
                        var line = (LineGeometry)s.Geometry;
                        return (line.EndPoint - line.StartPoint).Length;
                    });
                    Assert.IsTrue(trailLength <= 12 * 2 * OrbitParticlesCursorEffect.MaxTrailLength + 0.000001);
                }
                Assert.AreEqual(12, effect.ParticleCount);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                effect.Update(new Point(100000, 100000), 0.03);
                Assert.IsFalse(Shapes(Drawing(effect)).Any(s => s.Pen != null));
            });
        }

        [TestMethod]
        public void AppearanceDefaultsAndInvalidValuesAreNormalized()
        {
            var settings = JsonConvert.DeserializeObject<MouseEffectSetting>("{}")!;
            Assert.AreEqual(2, settings.OrbitParticlesLayers);
            Assert.AreEqual(28.0, settings.OrbitParticlesRadius);
            Assert.AreEqual("#5CE1FF", settings.OrbitParticlesColor1);
            Assert.AreEqual("#B38BFF", settings.OrbitParticlesColor2);
            Assert.AreEqual("#FF91CD", settings.OrbitParticlesColor3);
            settings.OrbitParticlesLayers = -10;
            Assert.AreEqual(1, settings.OrbitParticlesLayers);
            settings.OrbitParticlesLayers = 100;
            Assert.AreEqual(4, settings.OrbitParticlesLayers);
            foreach (var value in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
            {
                settings.OrbitParticlesRadius = value;
                Assert.AreEqual(28.0, settings.OrbitParticlesRadius);
            }
            settings.OrbitParticlesRadius = -1;
            Assert.AreEqual(10.0, settings.OrbitParticlesRadius);
            settings.OrbitParticlesRadius = 1000;
            Assert.AreEqual(120.0, settings.OrbitParticlesRadius);
            settings.OrbitParticlesColor1 = " #ab1234 ";
            Assert.AreEqual("#AB1234", settings.OrbitParticlesColor1);
            settings.OrbitParticlesColor1 = "bad";
            settings.OrbitParticlesColor2 = null!;
            settings.OrbitParticlesColor3 = "#123";
            Assert.AreEqual(MouseEffectSetting.DefaultOrbitParticlesColor1, settings.OrbitParticlesColor1);
            Assert.AreEqual(MouseEffectSetting.DefaultOrbitParticlesColor2, settings.OrbitParticlesColor2);
            Assert.AreEqual(MouseEffectSetting.DefaultOrbitParticlesColor3, settings.OrbitParticlesColor3);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        public void LayerCountRadiusDirectionAndBudgetsFollowSettings(int layers)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting { OrbitParticlesLayers = layers, OrbitParticlesRadius = 60 };
                var effect = new OrbitParticlesCursorEffect(settings);
                var cursor = new Point(200, 200);
                effect.Update(cursor, 0.01);
                var before = Halos(effect).Select(s => ((EllipseGeometry)s.Geometry).Center - cursor).ToArray();
                effect.Update(cursor, 0.03);
                var after = Halos(effect).Select(s => ((EllipseGeometry)s.Geometry).Center - cursor).ToArray();
                Assert.AreEqual(layers * 6, effect.ParticleCount);
                for (var i = 0; i < after.Length; i++)
                {
                    Assert.AreEqual(60 + i / 6 * 15, after[i].Length, 0.000001);
                    Assert.IsTrue(i / 6 % 2 == 0 ? Vector.CrossProduct(before[i], after[i]) > 0 : Vector.CrossProduct(before[i], after[i]) < 0);
                }
                for (var i = 0; i < 100; i++) effect.Update(cursor, 0.03);
                Assert.IsTrue(effect.TrailSampleCount <= OrbitParticlesCursorEffect.MaxTrailSamples);
                Assert.IsTrue(Shapes(Drawing(effect)).Count() <= layers * 6 * (3 + OrbitParticlesCursorEffect.MaxTrailSamples * 2));
            });
        }

        [TestMethod]
        public void GeometryChangesResetHistoryWithoutMovingCenterOrResizingParticles()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new OrbitParticlesCursorEffect(settings);
                var cursor = new Point(200, 200);
                effect.Update(cursor, 0.01);
                effect.Update(cursor, 0.03);
                Assert.IsTrue(Shapes(Drawing(effect)).Any(s => s.Pen != null));
                var size = ((EllipseGeometry)Halos(effect)[0].Geometry).RadiusX;
                settings.OrbitParticlesRadius = 100;
                effect.Update(cursor, 0.01);
                Assert.AreEqual(1, effect.TrailSampleCount);
                Assert.IsFalse(Shapes(Drawing(effect)).Any(s => s.Pen != null));
                Assert.AreEqual(200, Center(effect).X, 0.000001);
                Assert.AreEqual(size, ((EllipseGeometry)Halos(effect)[0].Geometry).RadiusX);
                settings.OrbitParticlesLayers = 4;
                effect.Update(cursor, 0.03);
                Assert.AreEqual(24, effect.ParticleCount);
                Assert.IsFalse(Shapes(Drawing(effect)).Any(s => s.Pen != null));
                settings.OrbitParticlesLayers = 1;
                effect.Update(cursor, 0.03);
                Assert.AreEqual(6, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void CustomColorsAffectAllLayersAndReuseFrozenResources()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new OrbitParticlesCursorEffect(settings);
                var cursor = new Point(200, 200);
                effect.Update(cursor, 0.01);
                var defaultHalo = Halos(effect)[0].Brush;
                settings.OrbitParticlesColor1 = settings.OrbitParticlesColor2 = settings.OrbitParticlesColor3 = "#FF1234";
                effect.Update(cursor, 0.03);
                var halo = Halos(effect)[0].Brush;
                Assert.AreNotSame(defaultHalo, halo);
                var stops = ((RadialGradientBrush)halo).GradientStops;
                Assert.AreEqual((byte)255, stops[0].Color.R);
                Assert.AreEqual((byte)18, stops[0].Color.G);
                var shapes = Shapes(Drawing(effect)).ToArray();
                Assert.IsTrue(shapes.Where(s => s.Brush is SolidColorBrush b && b.Color != Colors.White)
                    .All(s => ((SolidColorBrush)s.Brush).Color == Color.FromRgb(255, 18, 52)));
                Assert.IsTrue(shapes.Where(s => s.Pen != null).All(s => s.Pen.IsFrozen && ((SolidColorBrush)s.Pen.Brush).Color.G == 18));
                var pen = shapes.First(s => s.Pen != null).Pen;
                effect.Update(cursor, 0.0001);
                Assert.AreSame(halo, Halos(effect)[0].Brush);
                Assert.IsTrue(Shapes(Drawing(effect)).Any(s => ReferenceEquals(s.Pen, pen)));
                settings.OrbitParticlesColor1 = MouseEffectSetting.DefaultOrbitParticlesColor1;
                settings.OrbitParticlesColor2 = MouseEffectSetting.DefaultOrbitParticlesColor2;
                settings.OrbitParticlesColor3 = MouseEffectSetting.DefaultOrbitParticlesColor3;
                effect.Update(cursor, 0.01);
                Assert.AreSame(defaultHalo, Halos(effect)[0].Brush);
            });
        }

        [TestMethod]
        public void AppearancePanelValidatesColorsAndResetOnlyChangesDraft()
        {
            RunSta(() =>
            {
                var config = new MainConfig();
                var settings = config.MouseEffectSetting;
                settings.OrbitParticlesLayers = 4;
                settings.OrbitParticlesRadius = 80;
                settings.OrbitParticlesSpeed = 2;
                settings.OrbitParticlesColor1 = "#123456";
                var window = new MouseEffectSettingWindow(MouseEffectType.OrbitParticles, config);
                try
                {
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_OrbitParticles")).Visibility);
                    var layers = (Slider)window.FindName("slider_OrbitParticlesLayers");
                    var radius = (Slider)window.FindName("slider_OrbitParticlesRadius");
                    Assert.AreEqual(4.0, layers.Value);
                    Assert.AreEqual(80.0, radius.Value);
                    Assert.AreEqual(200.0, ((Slider)window.FindName("slider_OrbitParticlesSpeed")).Value);
                    var save = (Button)window.FindName("btn_Save");
                    foreach (var name in new[] { "text_OrbitParticlesColor1", "text_OrbitParticlesColor2", "text_OrbitParticlesColor3" })
                    {
                        var color = (TextBox)window.FindName(name);
                        color.Text = "bad";
                        Assert.IsFalse(save.IsEnabled);
                        save.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        Assert.AreEqual(4, settings.OrbitParticlesLayers);
                        color.Text = "#112233";
                        Assert.IsTrue(save.IsEnabled);
                    }
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(2.0, layers.Value);
                    Assert.AreEqual(28.0, radius.Value);
                    Assert.AreEqual(100.0, ((Slider)window.FindName("slider_OrbitParticlesSpeed")).Value);
                    Assert.AreEqual(MouseEffectSetting.DefaultOrbitParticlesColor1, ((TextBox)window.FindName("text_OrbitParticlesColor1")).Text);
                }
                finally { window.Close(); }
                Assert.AreEqual(4, settings.OrbitParticlesLayers);
                Assert.AreEqual(80.0, settings.OrbitParticlesRadius);
                Assert.AreEqual(2.0, settings.OrbitParticlesSpeed);
                Assert.AreEqual("#123456", settings.OrbitParticlesColor1);
                var other = new MouseEffectSettingWindow(MouseEffectType.BinaryTrail, config);
                try { Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)other.FindName("panel_OrbitParticles")).Visibility); }
                finally { other.Close(); }
            });
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.25)]
        [DataRow(10.0, 3.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void SpeedIsNormalized(double value, double expected)
        {
            Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MouseEffectSetting>("{}")!.OrbitParticlesSpeed);
            Assert.AreEqual(expected, new MouseEffectSetting { OrbitParticlesSpeed = value }.OrbitParticlesSpeed);
        }

        [DataTestMethod]
        [DataRow(0.25)]
        [DataRow(1.0)]
        [DataRow(3.0)]
        public void SpeedOnlyChangesOrbitNotFollowingOrPulse(double speed)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting { OrbitParticlesSpeed = speed };
                var effect = new OrbitParticlesCursorEffect(settings);
                var baseline = new OrbitParticlesCursorEffect();
                effect.Update(new Point(200, 200), 0.01);
                baseline.Update(new Point(200, 200), 0.01);
                var before = ((EllipseGeometry)Halos(effect)[0].Geometry).Center - Center(effect);
                effect.Update(new Point(240, 210), 0.1);
                baseline.Update(new Point(240, 210), 0.1);
                var after = ((EllipseGeometry)Halos(effect)[0].Geometry).Center - Center(effect);
                var angle = Math.Atan2(Vector.CrossProduct(before, after), Vector.Multiply(before, after));
                Assert.AreEqual(0.1 * speed * Math.PI / 3, angle, 0.000001);
                Assert.AreEqual(Center(baseline).X, Center(effect).X, 0.000001);
                Assert.AreEqual(Center(baseline).Y, Center(effect).Y, 0.000001);
                double Pulse(OrbitParticlesCursorEffect e) => Groups(Drawing(e)).First(g => g.Children.OfType<GeometryDrawing>().Any(s => s.Brush is RadialGradientBrush)).Opacity;
                Assert.AreEqual(Pulse(baseline), Pulse(effect));
                effect.Update(new Point(240, 210), double.MaxValue);
                Assert.AreEqual(1, effect.TrailSampleCount);
                Assert.IsTrue(Halos(effect).All(s => double.IsFinite(((EllipseGeometry)s.Geometry).Center.X)));
            });
        }

        [TestMethod]
        public void LiveSpeedChangesPreservePhaseHistoryAndResources()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new OrbitParticlesCursorEffect(settings);
                var cursor = new Point(200, 200);
                effect.Update(cursor, 0.01);
                effect.Update(cursor, 0.03);
                var before = ((EllipseGeometry)Halos(effect)[0].Geometry).Center;
                var brush = Halos(effect)[0].Brush;
                var count = effect.TrailSampleCount;
                settings.OrbitParticlesSpeed = 3;
                effect.Update(cursor, double.Epsilon);
                Assert.AreEqual(before, ((EllipseGeometry)Halos(effect)[0].Geometry).Center);
                Assert.AreEqual(count, effect.TrailSampleCount);
                Assert.AreSame(brush, Halos(effect)[0].Brush);
                effect.Update(cursor, 0.03);
                var after = ((EllipseGeometry)Halos(effect)[0].Geometry).Center;
                var angle = Math.Atan2(Vector.CrossProduct(before - cursor, after - cursor), Vector.Multiply(before - cursor, after - cursor));
                Assert.AreEqual(0.03 * 3 * Math.PI / 3, angle, 0.000001);
                Assert.IsTrue(Shapes(Drawing(effect)).Any(s => s.Geometry is LineGeometry line && line.EndPoint == before));
                effect.Update(cursor, 0.5);
                Assert.IsFalse(Shapes(Drawing(effect)).Any(s => s.Pen != null));
                effect.Clear();
                effect.Update(cursor, 0.01);
                var fresh = new OrbitParticlesCursorEffect(settings);
                fresh.Update(cursor, 0.01);
                Assert.AreEqual(((EllipseGeometry)Halos(fresh)[0].Geometry).Center, ((EllipseGeometry)Halos(effect)[0].Geometry).Center);
            });
        }

        private static IEnumerable<DrawingGroup> Groups(Drawing drawing)
        {
            if (drawing is DrawingGroup group)
            {
                yield return group;
                foreach (var child in group.Children)
                    foreach (var nested in Groups(child)) yield return nested;
            }
        }

        private static DrawingGroup Drawing(OrbitParticlesCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
        private static IEnumerable<GeometryDrawing> Shapes(Drawing drawing)
        {
            if (drawing is GeometryDrawing geometry) yield return geometry;
            else if (drawing is DrawingGroup group)
                foreach (var child in group.Children)
                    foreach (var shape in Shapes(child)) yield return shape;
        }
        private static GeometryDrawing[] Halos(OrbitParticlesCursorEffect effect) => Shapes(Drawing(effect)).Where(s => s.Brush is RadialGradientBrush).ToArray();
        private static Point Center(OrbitParticlesCursorEffect effect)
        {
            var points = Halos(effect).Select(s => ((EllipseGeometry)s.Geometry).Center).ToArray();
            return new Point(points.Average(p => p.X), points.Average(p => p.Y));
        }
        private static long Alpha(OrbitParticlesCursorEffect effect)
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
