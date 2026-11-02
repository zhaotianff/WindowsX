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
    public class FireworksTrailEffectTests
    {
        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{\"FireworksOpacity\":0.4}}")]
        public void DefaultsAndEnumRemainCompatible(string json)
        {
            Assert.AreEqual(2, (int)MouseEffectType.Fireworks);
            Assert.AreEqual(13, (int)MouseEffectType.ColorfulMist);
            Assert.AreEqual(14, (int)MouseEffectType.FireworksTrail);
            Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.FireworksTrailOpacity);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { FireworksTrailOpacity = value }.FireworksTrailOpacity);
        }

        [TestMethod]
        public void SparksBurstInAllDirectionsThenAccelerateDownward()
        {
            RunSta(() =>
            {
                var effect = new FireworksTrailCursorEffect();
                var origin = new Point(200, 200);
                effect.Update(origin, 0.016);
                Assert.AreEqual(20, effect.ParticleCount);
                effect.Update(origin, 0.1);
                var first = Heads(effect);
                Assert.AreEqual(20, first.Length);
                Assert.IsTrue(first.Any(p => p.X < 195) && first.Any(p => p.X > 205));
                Assert.IsTrue(first.Any(p => p.Y < 195) && first.Any(p => p.Y > 205));
                effect.Update(origin, 0.1);
                var second = Heads(effect);
                effect.Update(origin, 0.1);
                var third = Heads(effect);
                for (var i = 0; i < first.Length; i++)
                {
                    Assert.AreEqual(0, third[i].X - 2 * second[i].X + first[i].X, 0.000001);
                    Assert.AreEqual(2.4, third[i].Y - 2 * second[i].Y + first[i].Y, 0.000001);
                }
                effect.Update(origin, 0.4);
                var falling = Heads(effect);
                effect.Update(origin, 0.1);
                var later = Heads(effect);
                for (var i = 0; i < falling.Length; i++) Assert.IsTrue(later[i].Y > falling[i].Y);
            });
        }

        [TestMethod]
        public void TrailsFollowRecentPathAndFadeTowardTail()
        {
            RunSta(() =>
            {
                var effect = new FireworksTrailCursorEffect();
                var point = new Point(200, 200);
                effect.Update(point, 0.016);
                effect.Update(point, 0.4);
                var drawing = Drawing(effect);
                var lines = Shapes(drawing).Where(s => s.Geometry is LineGeometry && s.Pen.Thickness == 1.5).ToArray();
                Assert.AreEqual(20 * 6, lines.Length);
                Assert.IsTrue(lines.All(s => s.Pen.IsFrozen));
                var first = (LineGeometry)lines[0].Geometry;
                var last = (LineGeometry)lines[5].Geometry;
                Assert.AreEqual(Heads(effect)[0], last.EndPoint);
                Assert.AreNotEqual(point, first.StartPoint);
                for (var i = 0; i < 5; i++)
                    Assert.AreEqual(((LineGeometry)lines[i].Geometry).EndPoint, ((LineGeometry)lines[i + 1].Geometry).StartPoint);
                var tailGroups = Groups(drawing).Where(g => g.Children.OfType<GeometryDrawing>().Any(s => s.Geometry is LineGeometry && s.Pen.Thickness == 1.5)).Take(6).ToArray();
                Assert.AreEqual(6, tailGroups.Length);
                for (var i = 1; i < 6; i++) Assert.IsTrue(tailGroups[i].Opacity > tailGroups[i - 1].Opacity);
                var sparkOpacity = Groups(drawing).First(g => g.Children.OfType<GeometryDrawing>().Any(s => s.Geometry is EllipseGeometry)).Opacity;
                effect.Update(point, 0.2);
                Assert.IsTrue(Groups(Drawing(effect)).First(g => g.Children.OfType<GeometryDrawing>().Any(s => s.Geometry is EllipseGeometry)).Opacity < sparkOpacity);
                Assert.IsTrue(FireworksTrailCursorEffect.PreviewImage.IsFrozen);
                Assert.IsTrue(Shapes(FireworksTrailCursorEffect.PreviewImage.Drawing).Any(s => s.Geometry is LineGeometry));
            });
        }

        [TestMethod]
        public void EmissionIsThrottledBoundedAndStopsWhenStationary()
        {
            RunSta(() =>
            {
                var effect = new FireworksTrailCursorEffect();
                effect.Update(new Point(100, 100), 0.016);
                effect.Update(new Point(200, 100), 0.01);
                Assert.AreEqual(20, effect.ParticleCount);
                effect.Update(new Point(200, 100), 0.09);
                Assert.AreEqual(40, effect.ParticleCount);
                for (var i = 0; i < 100; i++)
                {
                    effect.Update(new Point(i * 100, 100), 0.1);
                    Assert.IsTrue(effect.ParticleCount <= FireworksTrailCursorEffect.MaxParticles);
                }
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                effect.Update(new Point(9900, 100), 5);
                effect.Update(new Point(9900, 100), 0.1);
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.IsFalse(Shapes(Drawing(effect)).Any());
                effect.Update(new Point(100, 100), 10);
                Assert.AreEqual(20, effect.ParticleCount);
                effect.Clear();
                effect.Clear();
                Assert.IsFalse(Shapes(Drawing(effect)).Any());
                effect.Update(new Point(100, 100), 0.016);
                Assert.AreEqual(20, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void InvalidInputIsIgnoredAndOpacityUpdatesLive()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new FireworksTrailCursorEffect(settings);
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
                settings.FireworksTrailOpacity = 0;
                effect.Update(new Point(200, 200), 0.000001);
                Assert.AreEqual(0L, Alpha(effect));
                settings.FireworksTrailOpacity = 0.25;
                effect.Update(new Point(200, 200), 0.000001);
                Assert.IsTrue(Alpha(effect) > 0 && Alpha(effect) < initial);
                Assert.AreEqual(1.0, settings.FireworksOpacity);
                Assert.IsFalse(effect.IsHitTestVisible);
            });
        }

        [DataTestMethod]
        [DataRow("switch")]
        [DataRow("start")]
        [DataRow("stop")]
        [DataRow("close")]
        public void WindowClearsTrails(string action)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                try
                {
                    window.UpdateMouseEffectType(MouseEffectType.FireworksTrail);
                    var effect = new FireworksTrailCursorEffect();
                    effect.Update(new Point(200, 200), 0.016);
                    var field = typeof(MouseEffectWindow).GetField("fireworksTrailEffect", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    field.SetValue(window, effect);
                    var canvas = (Canvas)window.FindName("canvas");
                    canvas.Children.Add(effect);
                    if (action == "switch") window.UpdateMouseEffectType(MouseEffectType.Fireworks);
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
                config.MouseEffectSetting.FireworksTrailOpacity = 0.6;
                var window = new MouseEffectSettingWindow(MouseEffectType.FireworksTrail, config);
                try
                {
                    Assert.AreEqual("烟花尾迹设置", window.Title);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(60.0, slider.Value);
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_ParticleOpacity")).Visibility);
                    foreach (var name in new[] { "panel_Expansion", "panel_StardustSize", "panel_InkFluid", "panel_NeonRibbon" })
                        Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName(name)).Visibility);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    slider.Value = 20;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.6, config.MouseEffectSetting.FireworksTrailOpacity);
            });
        }

        [TestMethod]
        public void DialogPersistsOnlyTrailAndKeepsSelection()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-FireworksTrailTests-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.FireworksOpacity = 0.6;
                    settings.FireworksExpansion = 2;
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)MouseEffectType.FireworksTrail;
                    var window = new MouseEffectSettingWindow(MouseEffectType.FireworksTrail, config);
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
                    Assert.AreEqual(0.25, settings.FireworksTrailOpacity);
                    var restored = ConfigHelper.LoadMainConfig();
                    Assert.AreEqual(0.25, restored.MouseEffectSetting.FireworksTrailOpacity);
                    Assert.AreEqual(0.6, restored.MouseEffectSetting.FireworksOpacity);
                    Assert.AreEqual(2.0, restored.MouseEffectSetting.FireworksExpansion);
                    Assert.IsTrue(restored.MouseEffectSetting.IsEnabled);
                    Assert.AreEqual(14, restored.MouseEffectSetting.SelectedEffectType);
                }
                finally
                {
                    Environment.CurrentDirectory = original;
                    System.IO.Directory.Delete(directory, true);
                }
            });
        }

        private static DrawingGroup Drawing(FireworksTrailCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
        private static Point[] Heads(FireworksTrailCursorEffect effect) => Shapes(Drawing(effect)).Select(s => s.Geometry)
            .OfType<EllipseGeometry>().Where(g => g.RadiusX > 1).Select(g => g.Center).ToArray();
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
        private static long Alpha(FireworksTrailCursorEffect effect)
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
