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
    public class FallingLeavesEffectTests
    {
        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{\"FallingPetalsOpacity\":0.4}}")]
        public void DefaultsAndTypesRemainCompatible(string json)
        {
            Assert.AreEqual(14, (int)MouseEffectType.FireworksTrail);
            Assert.AreEqual(15, (int)MouseEffectType.FallingLeaves);
            Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.FallingLeavesOpacity);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { FallingLeavesOpacity = value }.FallingLeavesOpacity);
        }

        [TestMethod]
        public void LeavesHaveVeinsAndStemAndFallRotateAndFade()
        {
            RunSta(() =>
            {
                var effect = new FallingLeavesCursorEffect();
                var point = new Point(200, 200);
                effect.Update(point, 0.016);
                var drawing = Drawing(effect);
                var shapes = Shapes(drawing).ToArray();
                var body = shapes.Single(s => s.Brush is LinearGradientBrush);
                Assert.IsTrue(body.Geometry.IsFrozen && body.Brush.IsFrozen);
                var figure = PathGeometry.CreateFromGeometry(body.Geometry).Figures.Single();
                Assert.IsTrue(figure.IsFilled && figure.IsClosed);
                Assert.IsTrue(figure.Segments.Any(s => s is BezierSegment || s is PolyBezierSegment));
                var veins = shapes.Where(s => s.Geometry is LineGeometry).ToArray();
                Assert.AreEqual(7, veins.Length);
                Assert.IsTrue(veins.All(s => s.Pen.IsFrozen));
                Assert.IsTrue(((LineGeometry)veins[0].Geometry).EndPoint.Y > body.Geometry.Bounds.Bottom);
                var before = Groups(drawing).Single(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value;
                effect.Update(point, 0.7);
                var after = Groups(Drawing(effect)).Single(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value;
                Assert.IsTrue(after.OffsetY > before.OffsetY);
                Assert.AreNotEqual(before.M11, after.M11);
                Assert.IsTrue(Groups(Drawing(effect)).Any(g => g.Opacity < 1));
                Assert.AreEqual(1, effect.ParticleCount);
                Assert.IsFalse(effect.IsHitTestVisible);
                var preview = FallingLeavesCursorEffect.PreviewImage;
                Assert.IsTrue(preview.IsFrozen);
                var colors = Shapes(preview.Drawing).Select(s => s.Brush).OfType<LinearGradientBrush>().Select(b => b.GradientStops[0].Color).Distinct().ToArray();
                Assert.AreEqual(4, colors.Length);
                Assert.IsTrue(colors.All(c => c.R > c.G && c.G > c.B));
            });
        }

        [TestMethod]
        public void EmissionIsInterpolatedThrottledBoundedAndStopsWhenStationary()
        {
            RunSta(() =>
            {
                var effect = new FallingLeavesCursorEffect();
                effect.Update(new Point(100, 200), 0.016);
                effect.Update(new Point(240, 200), 0.01);
                Assert.AreEqual(1, effect.ParticleCount);
                effect.Update(new Point(240, 200), 0.06);
                Assert.AreEqual(6, effect.ParticleCount);
                Assert.IsTrue(Groups(Drawing(effect)).Any(g => g.Transform != null && g.Transform.Value.OffsetX > 140 && g.Transform.Value.OffsetX < 200));
                effect.Update(new Point(10000, 200), 0.06);
                Assert.AreEqual(12, effect.ParticleCount);
                for (var i = 0; i < 100; i++)
                {
                    effect.Update(new Point(i * 200, 200), 0.06);
                    Assert.IsTrue(effect.ParticleCount <= FallingLeavesCursorEffect.MaxParticles);
                }
                Assert.AreEqual(120, effect.ParticleCount);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                effect.Update(new Point(19800, 200), 5);
                effect.Update(new Point(19800, 200), 0.016);
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
                var effect = new FallingLeavesCursorEffect(settings);
                foreach (var elapsed in new[] { 0.0, -1, double.NaN, double.PositiveInfinity }) effect.Update(new Point(200, 200), elapsed);
                effect.Update(new Point(double.NaN, 200), 0.016);
                effect.Update(new Point(200, double.PositiveInfinity), 0.016);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Measure(new Size(400, 400));
                effect.Arrange(new Rect(0, 0, 400, 400));
                effect.Update(new Point(200, 200), 0.016);
                var initial = Alpha(effect);
                Assert.IsTrue(initial > 0);
                settings.FallingLeavesOpacity = 0;
                effect.Update(new Point(200, 200), 0.000001);
                Assert.AreEqual(0L, Alpha(effect));
                settings.FallingLeavesOpacity = 0.25;
                effect.Update(new Point(200, 200), 0.000001);
                Assert.IsTrue(Alpha(effect) > 0 && Alpha(effect) < initial);
                Assert.AreEqual(1.0, settings.FallingPetalsOpacity);
            });
        }

        [DataTestMethod]
        [DataRow("switch")]
        [DataRow("start")]
        [DataRow("stop")]
        [DataRow("close")]
        public void WindowClearsLeaves(string action)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                try
                {
                    window.UpdateMouseEffectType(MouseEffectType.FallingLeaves);
                    var effect = new FallingLeavesCursorEffect();
                    effect.Update(new Point(200, 200), 0.016);
                    var field = typeof(MouseEffectWindow).GetField("fallingLeavesEffect", BindingFlags.NonPublic | BindingFlags.Instance)!;
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
                config.MouseEffectSetting.FallingLeavesOpacity = 0.6;
                var window = new MouseEffectSettingWindow(MouseEffectType.FallingLeaves, config);
                try
                {
                    Assert.AreEqual("落叶设置", window.Title);
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
                Assert.AreEqual(0.6, config.MouseEffectSetting.FallingLeavesOpacity);
            });
        }

        [TestMethod]
        public void DialogPersistsOnlyLeavesAndKeepsSelection()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-LeavesTests-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.FallingPetalsOpacity = 0.6;
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)MouseEffectType.FallingLeaves;
                    var window = new MouseEffectSettingWindow(MouseEffectType.FallingLeaves, config);
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
                    Assert.AreEqual(0.25, settings.FallingLeavesOpacity);
                    var restored = ConfigHelper.LoadMainConfig();
                    Assert.AreEqual(0.25, restored.MouseEffectSetting.FallingLeavesOpacity);
                    Assert.AreEqual(0.6, restored.MouseEffectSetting.FallingPetalsOpacity);
                    Assert.IsTrue(restored.MouseEffectSetting.IsEnabled);
                    Assert.AreEqual(15, restored.MouseEffectSetting.SelectedEffectType);
                    var reopened = new MouseEffectSettingWindow(MouseEffectType.FallingLeaves, restored);
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

        private static DrawingGroup Drawing(FallingLeavesCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
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
        private static long Alpha(FallingLeavesCursorEffect effect)
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
