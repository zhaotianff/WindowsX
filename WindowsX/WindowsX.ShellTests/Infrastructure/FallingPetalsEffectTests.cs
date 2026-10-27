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
    public class FallingPetalsEffectTests
    {
        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{\"StardustOpacity\":0.4}}")]
        public void OldConfigurationAndTypesRemainCompatible(string json)
        {
            Assert.AreEqual(11, (int)MouseEffectType.Stardust);
            Assert.AreEqual(12, (int)MouseEffectType.FallingPetals);
            Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.FallingPetalsOpacity);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { FallingPetalsOpacity = value }.FallingPetalsOpacity);
        }

        [TestMethod]
        public void PetalsHavePinkCurvedGeometryAndFallRotateAndFade()
        {
            RunSta(() =>
            {
                var effect = new FallingPetalsCursorEffect();
                var point = new Point(200, 200);
                effect.Update(point, 0.016);
                var initial = Drawing(effect);
                var shape = Shapes(initial).Single();
                Assert.IsTrue(shape.Geometry.IsFrozen && shape.Brush.IsFrozen);
                Assert.IsNull(shape.Pen);
                var figure = PathGeometry.CreateFromGeometry(shape.Geometry).Figures.Single();
                Assert.IsTrue(figure.IsFilled && figure.IsClosed);
                Assert.IsTrue(figure.Segments.Any(s => s is BezierSegment || s is PolyBezierSegment));
                var brush = (LinearGradientBrush)shape.Brush;
                Assert.AreEqual(Color.FromRgb(255, 182, 193), brush.GradientStops[1].Color);
                Assert.IsTrue(brush.GradientStops.All(s => s.Color.R > s.Color.B && s.Color.B > s.Color.G));
                var before = Groups(initial).Single(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value;
                effect.Update(point, 0.8);
                var afterDrawing = Drawing(effect);
                var after = Groups(afterDrawing).Single(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value;
                Assert.IsTrue(after.OffsetY > before.OffsetY);
                Assert.AreNotEqual(before.M11, after.M11);
                Assert.IsTrue(Groups(afterDrawing).Any(g => g.Opacity < 1));
                Assert.AreEqual(1, effect.ParticleCount);
                Assert.IsFalse(effect.IsHitTestVisible);
                Assert.IsTrue(FallingPetalsCursorEffect.PreviewImage.IsFrozen);
            });
        }

        [TestMethod]
        public void EmissionFollowsPathIsBoundedAndStopsWhenStationary()
        {
            RunSta(() =>
            {
                var effect = new FallingPetalsCursorEffect();
                effect.Update(new Point(100, 200), 0.016);
                effect.Update(new Point(244, 200), 0.05);
                Assert.AreEqual(7, effect.ParticleCount);
                var centers = Groups(Drawing(effect)).Where(g => g.Transform != null && !g.Transform.Value.IsIdentity).Select(g => g.Transform.Value.OffsetX).ToArray();
                Assert.IsTrue(centers.Any(x => x > 130 && x < 210));
                effect.Update(new Point(10000, 200), 0.05);
                Assert.AreEqual(7 + FallingPetalsCursorEffect.MaxEmissionPerUpdate, effect.ParticleCount);
                for (var i = 0; i < 200; i++)
                {
                    effect.Update(new Point(i * 200, 200), 0.05);
                    Assert.IsTrue(effect.ParticleCount <= FallingPetalsCursorEffect.MaxParticles);
                }
                Assert.AreEqual(FallingPetalsCursorEffect.MaxParticles, effect.ParticleCount);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                var last = new Point(39800, 200);
                effect.Update(last, 5);
                effect.Update(last, 0.016);
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.IsFalse(Shapes(Drawing(effect)).Any());
                effect.Update(new Point(100, 200), 0.016);
                Assert.AreEqual(1, effect.ParticleCount);
                effect.Clear();
                effect.Clear();
                effect.Update(new Point(100, 200), 0.016);
                Assert.AreEqual(1, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void InvalidUpdatesDoNotEmit()
        {
            RunSta(() =>
            {
                var effect = new FallingPetalsCursorEffect();
                foreach (var elapsed in new[] { 0.0, -1, double.NaN, double.PositiveInfinity }) effect.Update(new Point(200, 200), elapsed);
                effect.Update(new Point(double.NaN, 200), 0.016);
                effect.Update(new Point(200, double.PositiveInfinity), 0.016);
                Assert.AreEqual(0, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void RunningPetalsUseIndependentOpacity()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new FallingPetalsCursorEffect(settings);
                effect.Measure(new Size(400, 400));
                effect.Arrange(new Rect(0, 0, 400, 400));
                var point = new Point(200, 200);
                effect.Update(point, 0.016);
                var original = Alpha(effect);
                Assert.IsTrue(original > 0);
                settings.FallingPetalsOpacity = 0;
                effect.Update(point, 0.000001);
                Assert.AreEqual(0L, Alpha(effect));
                settings.FallingPetalsOpacity = 0.25;
                effect.Update(point, 0.000001);
                Assert.IsTrue(Alpha(effect) > 0 && Alpha(effect) < original);
                Assert.AreEqual(1.0, settings.StardustOpacity);
            });
        }

        [DataTestMethod]
        [DataRow("switch")]
        [DataRow("start")]
        [DataRow("stop")]
        [DataRow("close")]
        public void WindowClearsPetals(string action)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                try
                {
                    window.UpdateMouseEffectType(MouseEffectType.FallingPetals);
                    var effect = new FallingPetalsCursorEffect();
                    effect.Update(new Point(200, 200), 0.016);
                    var field = typeof(MouseEffectWindow).GetField("fallingPetalsEffect", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    field.SetValue(window, effect);
                    var canvas = (Canvas)window.FindName("canvas");
                    canvas.Children.Add(effect);
                    if (action == "switch") window.UpdateMouseEffectType(MouseEffectType.Stardust);
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
                config.MouseEffectSetting.FallingPetalsOpacity = 0.6;
                var window = new MouseEffectSettingWindow(MouseEffectType.FallingPetals, config);
                try
                {
                    Assert.AreEqual("花瓣飘落设置", window.Title);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(60.0, slider.Value);
                    Assert.AreEqual(Visibility.Visible, ((StackPanel)window.FindName("panel_ParticleOpacity")).Visibility);
                    foreach (var name in new[] { "panel_StardustSize", "panel_Expansion", "panel_NeonRibbon", "panel_InkFluid", "panel_Following", "panel_Trail" })
                        Assert.AreEqual(Visibility.Collapsed, ((StackPanel)window.FindName(name)).Visibility);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    slider.Value = 20;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.6, config.MouseEffectSetting.FallingPetalsOpacity);
            });
        }

        [TestMethod]
        public void DialogPersistsOnlyPetalsAndKeepsSelection()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-PetalTests-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    config.MouseEffectSetting.StardustOpacity = 0.6;
                    config.MouseEffectSetting.StardustSizeScale = 2;
                    config.MouseEffectSetting.IsEnabled = true;
                    config.MouseEffectSetting.SelectedEffectType = (int)MouseEffectType.FallingPetals;
                    var window = new MouseEffectSettingWindow(MouseEffectType.FallingPetals, config);
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
                    Assert.AreEqual(0.25, restored.MouseEffectSetting.FallingPetalsOpacity);
                    Assert.AreEqual(0.6, restored.MouseEffectSetting.StardustOpacity);
                    Assert.AreEqual(2.0, restored.MouseEffectSetting.StardustSizeScale);
                    Assert.IsTrue(restored.MouseEffectSetting.IsEnabled);
                    Assert.AreEqual(12, restored.MouseEffectSetting.SelectedEffectType);
                    var reopened = new MouseEffectSettingWindow(MouseEffectType.FallingPetals, restored);
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

        [TestMethod]
        public void PetalKindsAndSizesVaryAndRemainStableWithinOriginalMaximum()
        {
            RunSta(() =>
            {
                var effect = new FallingPetalsCursorEffect();
                var point = new Point(200, 200);
                effect.Update(point, 0.016);
                for (var i = 1; i <= 20; i++)
                {
                    point = new Point(200 + i * 200, 200);
                    effect.Update(point, 0.05);
                }
                var field = typeof(FallingPetalsCursorEffect).GetField("particles", BindingFlags.NonPublic | BindingFlags.Instance)!;
                var particles = (System.Collections.IEnumerable)field.GetValue(effect)!;
                var before = particles.Cast<object>().ToArray();
                var sizes = before.Select(p => (double)p.GetType().GetField("Size")!.GetValue(p)!).ToArray();
                var kinds = before.Select(p => (int)p.GetType().GetField("Kind")!.GetValue(p)!).ToArray();
                Assert.IsTrue(sizes.All(s => s >= 3 && s < 9));
                Assert.IsTrue(sizes.Any(s => s < 5));
                Assert.IsTrue(sizes.Distinct().Count() > 1);
                CollectionAssert.AreEquivalent(new[] { 0, 1, 2 }, kinds.Distinct().ToArray());
                effect.Update(point, 0.016);
                var after = particles.Cast<object>().ToArray();
                CollectionAssert.AreEqual(sizes, after.Select(p => (double)p.GetType().GetField("Size")!.GetValue(p)!).ToArray());
                CollectionAssert.AreEqual(kinds, after.Select(p => (int)p.GetType().GetField("Kind")!.GetValue(p)!).ToArray());
            });
        }

        [TestMethod]
        public void PreviewShowsThreePinkCurvedKindsWithoutLargerGeometry()
        {
            RunSta(() =>
            {
                var preview = FallingPetalsCursorEffect.PreviewImage;
                var shapes = Shapes(preview.Drawing).Where(s => s.Brush is LinearGradientBrush).ToArray();
                Assert.AreEqual(9, shapes.Length);
                var geometries = shapes.Select(s => s.Geometry).Distinct().ToArray();
                Assert.AreEqual(3, geometries.Length);
                Assert.AreEqual(3, geometries.Select(g => g.ToString()).Distinct().Count());
                var originalBounds = geometries[0].Bounds;
                foreach (var geometry in geometries)
                {
                    Assert.IsTrue(geometry.IsFrozen);
                    Assert.IsTrue(geometry.Bounds.Width <= originalBounds.Width);
                    Assert.IsTrue(geometry.Bounds.Height <= originalBounds.Height);
                    var figure = PathGeometry.CreateFromGeometry(geometry).Figures.Single();
                    Assert.IsTrue(figure.IsFilled && figure.IsClosed);
                    Assert.IsTrue(figure.Segments.Any(s => s is BezierSegment || s is PolyBezierSegment));
                }
                foreach (var group in Groups(preview.Drawing).Where(g => g.Transform != null && !g.Transform.Value.IsIdentity))
                {
                    var matrix = group.Transform.Value;
                    Assert.IsTrue(Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12) <= 9.000001);
                    Assert.IsTrue(Math.Sqrt(matrix.M21 * matrix.M21 + matrix.M22 * matrix.M22) <= 9.000001);
                }
            });
        }

        private static DrawingGroup Drawing(FallingPetalsCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
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
        private static long Alpha(FallingPetalsCursorEffect effect)
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
