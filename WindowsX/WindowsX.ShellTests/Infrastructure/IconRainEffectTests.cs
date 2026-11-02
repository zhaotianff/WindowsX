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
    public class IconRainEffectTests
    {
        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{\"FallingLeavesOpacity\":0.4}}")]
        public void DefaultsAndTypesRemainCompatible(string json)
        {
            Assert.AreEqual(15, (int)MouseEffectType.FallingLeaves);
            Assert.AreEqual(16, (int)MouseEffectType.IconRain);
            Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.IconRainOpacity);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { IconRainOpacity = value }.IconRainOpacity);
        }

        [TestMethod]
        public void RainAndPreviewContainStarHeartAndFourToedPaw()
        {
            RunSta(() =>
            {
                var effect = new IconRainCursorEffect();
                effect.Update(new Point(100, 200), 0.016);
                effect.Update(new Point(148, 200), 0.06);
                var shapes = Shapes(Drawing(effect)).ToArray();
                Assert.AreEqual(3, shapes.Length);
                Assert.IsTrue(shapes.All(s => s.Geometry.IsFrozen && s.Brush.IsFrozen && s.Pen == null));
                var star = PathGeometry.CreateFromGeometry(shapes[0].Geometry).Figures.Single();
                Assert.IsTrue(star.IsClosed && star.IsFilled);
                var vertices = new List<Point> { star.StartPoint };
                foreach (var segment in star.Segments)
                {
                    if (segment is LineSegment line) vertices.Add(line.Point);
                    else if (segment is PolyLineSegment poly) vertices.AddRange(poly.Points);
                }
                Assert.AreEqual(10, vertices.Count);
                for (var i = 0; i < vertices.Count; i++)
                    Assert.AreEqual(i % 2 == 0 ? 1 : 0.45, ((Vector)vertices[i]).Length, 0.000001);
                var heart = PathGeometry.CreateFromGeometry(shapes[1].Geometry).Figures.Single();
                Assert.IsTrue(heart.IsFilled && heart.IsClosed);
                Assert.IsTrue(heart.Segments.All(s => s is BezierSegment || s is PolyBezierSegment));
                var paw = (GeometryGroup)shapes[2].Geometry;
                Assert.AreEqual(5, paw.Children.Count);
                Assert.AreEqual(4, paw.Children.OfType<EllipseGeometry>().Count());
                Assert.AreEqual(3, shapes.Select(s => ((LinearGradientBrush)s.Brush).GradientStops[1].Color).Distinct().Count());
                var preview = IconRainCursorEffect.PreviewImage;
                Assert.IsTrue(preview.IsFrozen);
                Assert.AreEqual(3, Shapes(preview.Drawing).Where(s => s.Brush is LinearGradientBrush).Select(s => s.Geometry).Distinct().Count());
            });
        }

        [TestMethod]
        public void IconsPopUpThenFallAndFade()
        {
            RunSta(() =>
            {
                var effect = new IconRainCursorEffect();
                var point = new Point(200, 200);
                effect.Update(point, 0.016);
                effect.Update(point, 0.15);
                var before = Groups(Drawing(effect)).Single(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value;
                Assert.IsTrue(before.OffsetY < point.Y);
                effect.Update(point, 1.1);
                var after = Groups(Drawing(effect)).Single(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value;
                Assert.IsTrue(after.OffsetY > point.Y);
                Assert.IsTrue(Groups(Drawing(effect)).Any(g => g.Opacity < 0.5));
                Assert.AreEqual(1, effect.ParticleCount);
                Assert.IsFalse(effect.IsHitTestVisible);
            });
        }

        [TestMethod]
        public void EmissionIsThrottledBoundedAndStopsWhenStationary()
        {
            RunSta(() =>
            {
                var effect = new IconRainCursorEffect();
                effect.Update(new Point(100, 200), 0.016);
                effect.Update(new Point(244, 200), 0.01);
                Assert.AreEqual(1, effect.ParticleCount);
                effect.Update(new Point(244, 200), 0.06);
                Assert.AreEqual(7, effect.ParticleCount);
                Assert.IsTrue(Groups(Drawing(effect)).Any(g => g.Transform != null && g.Transform.Value.OffsetX > 140 && g.Transform.Value.OffsetX < 200));
                for (var i = 0; i < 100; i++)
                {
                    effect.Update(new Point(i * 300, 200), 0.06);
                    Assert.IsTrue(effect.ParticleCount <= IconRainCursorEffect.MaxParticles);
                }
                Assert.AreEqual(120, effect.ParticleCount);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                effect.Update(new Point(29700, 200), 5);
                effect.Update(new Point(29700, 200), 0.1);
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.IsFalse(Shapes(Drawing(effect)).Any());
                effect.Update(new Point(100, 200), 0.06);
                Assert.AreEqual(1, effect.ParticleCount);
                effect.Clear();
                effect.Clear();
                Assert.IsFalse(Shapes(Drawing(effect)).Any());
                effect.Update(new Point(100, 200), 0.016);
                Assert.AreEqual(1, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void InvalidInputIsIgnoredAndOpacityUpdatesLive()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new IconRainCursorEffect(settings);
                foreach (var elapsed in new[] { 0.0, -1, double.NaN, double.PositiveInfinity }) effect.Update(new Point(200, 200), elapsed);
                effect.Update(new Point(double.NaN, 200), 0.016);
                effect.Update(new Point(200, double.PositiveInfinity), 0.016);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Measure(new Size(400, 400));
                effect.Arrange(new Rect(0, 0, 400, 400));
                effect.Update(new Point(200, 200), 0.016);
                effect.Update(new Point(200, 200), 0.2);
                var initial = Alpha(effect);
                Assert.IsTrue(initial > 0);
                settings.IconRainOpacity = 0;
                effect.Update(new Point(200, 200), 0.000001);
                Assert.AreEqual(0L, Alpha(effect));
                settings.IconRainOpacity = 0.25;
                effect.Update(new Point(200, 200), 0.000001);
                Assert.IsTrue(Alpha(effect) > 0 && Alpha(effect) < initial);
                Assert.AreEqual(1.0, settings.FallingLeavesOpacity);
            });
        }

        [DataTestMethod]
        [DataRow("switch")]
        [DataRow("start")]
        [DataRow("stop")]
        [DataRow("close")]
        public void WindowClearsIcons(string action)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                try
                {
                    window.UpdateMouseEffectType(MouseEffectType.IconRain);
                    var effect = new IconRainCursorEffect();
                    effect.Update(new Point(200, 200), 0.016);
                    var field = typeof(MouseEffectWindow).GetField("iconRainEffect", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    field.SetValue(window, effect);
                    var canvas = (Canvas)window.FindName("canvas");
                    canvas.Children.Add(effect);
                    if (action == "switch") window.UpdateMouseEffectType(MouseEffectType.FallingLeaves);
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
                config.MouseEffectSetting.IconRainOpacity = 0.6;
                var window = new MouseEffectSettingWindow(MouseEffectType.IconRain, config);
                try
                {
                    Assert.AreEqual("图标雨设置", window.Title);
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
                Assert.AreEqual(0.6, config.MouseEffectSetting.IconRainOpacity);
            });
        }

        [TestMethod]
        public void DialogPersistsOnlyIconsAndKeepsSelection()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-IconRainTests-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.FallingLeavesOpacity = 0.6;
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)MouseEffectType.IconRain;
                    var window = new MouseEffectSettingWindow(MouseEffectType.IconRain, config);
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
                    Assert.AreEqual(0.25, settings.IconRainOpacity);
                    var restored = ConfigHelper.LoadMainConfig();
                    Assert.AreEqual(0.25, restored.MouseEffectSetting.IconRainOpacity);
                    Assert.AreEqual(0.6, restored.MouseEffectSetting.FallingLeavesOpacity);
                    Assert.IsTrue(restored.MouseEffectSetting.IsEnabled);
                    Assert.AreEqual(16, restored.MouseEffectSetting.SelectedEffectType);
                }
                finally
                {
                    Environment.CurrentDirectory = original;
                    System.IO.Directory.Delete(directory, true);
                }
            });
        }

        private static DrawingGroup Drawing(IconRainCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
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
        private static long Alpha(IconRainCursorEffect effect)
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
