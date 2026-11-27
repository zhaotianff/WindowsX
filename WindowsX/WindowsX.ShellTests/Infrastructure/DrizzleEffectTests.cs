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
    public class DrizzleEffectTests
    {
        [TestMethod]
        public void DefaultsAndIdentifiersRemainCompatible()
        {
            Assert.AreEqual(28, (int)MouseEffectType.SwordAura);
            Assert.AreEqual(29, (int)MouseEffectType.Drizzle);
            foreach (var json in new[] { "{}", "{\"MouseEffectSetting\":{}}", "{\"MouseEffectSetting\":null}" })
                Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.DrizzleOpacity);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { DrizzleOpacity = value }.DrizzleOpacity);
        }

        [TestMethod]
        public void FineRainFallsDiagonallyWhileStationaryUsingFrozenPens()
        {
            RunSta(() =>
            {
                var effect = new DrizzleCursorEffect();
                var cursor = new Point(200, 150);
                effect.Update(cursor, 0.01);
                Assert.AreEqual(4, effect.ParticleCount);
                var shapes = Shapes(effect);
                Assert.AreEqual(4, shapes.Length);
                foreach (var shape in shapes)
                {
                    var line = (LineGeometry)shape.Geometry;
                    Assert.IsTrue(line.EndPoint.X > line.StartPoint.X);
                    Assert.IsTrue(line.EndPoint.Y > line.StartPoint.Y);
                    Assert.IsTrue((line.EndPoint - line.StartPoint).Length >= 7);
                    Assert.IsTrue((line.EndPoint - line.StartPoint).Length <= 14);
                    Assert.IsNull(shape.Brush);
                    Assert.IsTrue(shape.Pen.IsFrozen && shape.Pen.Thickness <= 1);
                    Assert.IsInstanceOfType(shape.Pen.Brush, typeof(LinearGradientBrush));
                }
                effect.Update(cursor, 0.03);
                var moved = Shapes(effect);
                for (var i = 0; i < shapes.Length; i++)
                {
                    var before = ((LineGeometry)shapes[i].Geometry).EndPoint;
                    var after = ((LineGeometry)moved[i].Geometry).EndPoint;
                    Assert.IsTrue(after.X > before.X && after.Y > before.Y);
                    Assert.AreSame(shapes[i].Pen, moved[i].Pen);
                }
                effect.Update(cursor, 0.03);
                Assert.AreEqual(6, effect.ParticleCount);
                Assert.IsFalse(effect.IsHitTestVisible);
                var clip = Groups(Drawing(effect)).Single(g => g.ClipGeometry != null).ClipGeometry;
                Assert.IsTrue(clip.IsFrozen);
                Assert.AreEqual(new Rect(-120, -95, 240, 190), clip.Bounds);
                var preview = DrizzleCursorEffect.PreviewImage;
                Assert.IsTrue(preview.IsFrozen);
                Assert.AreEqual(new Rect(0, 0, 260, 170), preview.Drawing.Bounds);
                Assert.AreEqual(36, Groups(preview.Drawing).SelectMany(g => g.Children.OfType<GeometryDrawing>()).Count(s => s.Geometry is LineGeometry));
            });
        }

        [TestMethod]
        public void RainRegionFollowsCursorWithoutBridgingOrEmissionBursts()
        {
            RunSta(() =>
            {
                var effect = new DrizzleCursorEffect();
                effect.Update(new Point(100, 100), 0.01);
                var before = Shapes(effect).Select(s => s.Geometry.Bounds).ToArray();
                effect.Update(new Point(10000, -500), double.Epsilon);
                CollectionAssert.AreEqual(before, Shapes(effect).Select(s => s.Geometry.Bounds).ToArray());
                var transform = Groups(Drawing(effect)).Single(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value;
                Assert.AreEqual(10000.0, transform.OffsetX);
                Assert.AreEqual(-500.0, transform.OffsetY);
                for (var i = 0; i < 500; i++)
                {
                    var count = effect.ParticleCount;
                    effect.Update(new Point(i * 100, 100), 0.06);
                    Assert.IsTrue(effect.ParticleCount <= count + DrizzleCursorEffect.MaxEmissionPerUpdate);
                    Assert.IsTrue(effect.ParticleCount <= DrizzleCursorEffect.MaxParticles);
                    Assert.IsTrue(Shapes(effect).Length <= DrizzleCursorEffect.MaxParticles);
                    Assert.IsTrue(effect.SplashCount <= DrizzleCursorEffect.MaxSplashes);
                    Assert.IsTrue(AllShapes(effect).Length <= DrizzleCursorEffect.MaxParticles + 1 + DrizzleCursorEffect.MaxSplashes * 4);
                }
                effect.Update(new Point(), double.MaxValue);
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.AreEqual(0, effect.SplashCount);
                Assert.AreEqual(0.0, effect.WaterAmount);
                effect.Update(new Point(), 0.06);
                Assert.AreEqual(2, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void InvalidInputsAndClearAreSafe()
        {
            RunSta(() =>
            {
                var effect = new DrizzleCursorEffect();
                foreach (var dt in new[] { 0, -1, double.NaN, double.PositiveInfinity }) effect.Update(new Point(), dt);
                effect.Update(new Point(double.NaN, 0), 0.01);
                effect.Update(new Point(0, double.PositiveInfinity), 0.01);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(new Point(), 0.01);
                var visual = VisualTreeHelper.GetChild(effect, 0);
                effect.Clear();
                effect.Clear();
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.AreEqual(0, effect.SplashCount);
                Assert.AreEqual(0.0, effect.WaterAmount);
                Assert.IsTrue(Drawing(effect) == null || Drawing(effect).Bounds.IsEmpty);
                effect.Update(new Point(200, 100), 0.01);
                Assert.AreEqual(4, effect.ParticleCount);
                Assert.AreSame(visual, VisualTreeHelper.GetChild(effect, 0));
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
            });
        }

        [TestMethod]
        public void OpacityAppliesLiveToEntireRainRegion()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new DrizzleCursorEffect(settings);
                effect.Measure(new Size(400, 400));
                effect.Arrange(new Rect(0, 0, 400, 400));
                effect.Update(new Point(200, 200), 0.01);
                for (var i = 0; i < 150; i++) effect.Update(new Point(200, 200), 0.02);
                Assert.IsTrue(effect.WaterAmount > 0);
                var full = Alpha(effect);
                Assert.IsTrue(full > 0);
                settings.DrizzleOpacity = 0;
                effect.Update(new Point(200, 200), double.Epsilon);
                Assert.AreEqual(0L, Alpha(effect));
                settings.DrizzleOpacity = 0.5;
                effect.Update(new Point(200, 200), double.Epsilon);
                var half = Alpha(effect);
                Assert.IsTrue(half > 0 && half < full);
                Assert.AreEqual(1.0, settings.SwordAuraOpacity);
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
                    window.UpdateMouseEffectType(MouseEffectType.Drizzle);
                    var create = typeof(MouseEffectWindow).GetMethod("CreateEffects", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    create.Invoke(window, null);
                    create.Invoke(window, null);
                    var canvas = (Canvas)window.FindName("canvas");
                    Assert.AreEqual(1, canvas.Children.Count);
                    var effect = (DrizzleCursorEffect)canvas.Children[0];
                    effect.Update(new Point(100, 100), 0.01);
                    if (switchEffect) window.UpdateMouseEffectType(MouseEffectType.SwordAura);
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
                config.MouseEffectSetting.DrizzleOpacity = 0.6;
                var window = new MouseEffectSettingWindow(MouseEffectType.Drizzle, config);
                try
                {
                    Assert.AreEqual("斜风细雨设置", window.Title);
                    Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName("panel_SwordAura")).Visibility);
                    Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName("panel_Catkins")).Visibility);
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_ParticleOpacity")).Visibility);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(60.0, slider.Value);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    slider.Value = 25;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.6, config.MouseEffectSetting.DrizzleOpacity);
            });
        }

        [TestMethod]
        public void DialogPersistsOnlyDrizzleAndKeepsSelection()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-Drizzle-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.SwordAuraOpacity = 0.4;
                    settings.WillowCatkinsOpacity = 0.7;
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)MouseEffectType.Drizzle;
                    var window = new MouseEffectSettingWindow(MouseEffectType.Drizzle, config);
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
                    var restored = ConfigHelper.LoadMainConfig().MouseEffectSetting;
                    Assert.AreEqual(0.25, restored.DrizzleOpacity);
                    Assert.AreEqual(0.4, restored.SwordAuraOpacity);
                    Assert.AreEqual(0.7, restored.WillowCatkinsOpacity);
                    Assert.AreEqual(29, restored.SelectedEffectType);
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
        public void ImpactCreatesWaterAtExactLandingPointAndRippleExpands()
        {
            RunSta(() =>
            {
                var effect = new DrizzleCursorEffect();
                effect.Update(new Point(200, 150), 0.01);
                SetLandingDrop(effect);
                effect.Update(new Point(200, 150), 0.05);
                Assert.AreEqual(0, effect.SplashCount);
                Assert.AreEqual(0.0, effect.WaterAmount);
                effect.Update(new Point(200, 150), 0.1);
                Assert.AreEqual(1, effect.SplashCount);
                Assert.IsTrue(effect.WaterAmount > 0);
                var water = AllShapes(effect).Single(s => s.Brush is RadialGradientBrush);
                Assert.IsTrue(water.Brush.IsFrozen);
                var waterGeometry = (EllipseGeometry)water.Geometry;
                Assert.AreEqual(new Point(0, DrizzleCursorEffect.WaterSurfaceY), waterGeometry.Center);
                Assert.IsTrue(waterGeometry.RadiusX > waterGeometry.RadiusY * 5);
                var ripple = AllShapes(effect).Single(s => s.Geometry is EllipseGeometry && s.Pen != null);
                var first = (EllipseGeometry)ripple.Geometry;
                Assert.AreEqual(15 + 4 * Math.Sin(1), first.Center.X, 0.000001);
                Assert.AreEqual(DrizzleCursorEffect.WaterSurfaceY, first.Center.Y);
                Assert.AreEqual(2 + 0.05 / DrizzleCursorEffect.SplashLifetime * 19, first.RadiusX, 0.000001);
                Assert.IsTrue(ripple.Pen.IsFrozen);
                Assert.AreEqual(3, AllShapes(effect).Count(s => s.Geometry is EllipseGeometry && s.Brush is SolidColorBrush));
                Assert.IsTrue(Shapes(effect).All(s => ((LineGeometry)s.Geometry).EndPoint.Y < DrizzleCursorEffect.WaterSurfaceY));
                var firstOpacity = Groups(Drawing(effect)).Single(g => g.Children.OfType<GeometryDrawing>().Any(s => s.Geometry is EllipseGeometry && s.Pen != null)).Opacity;
                effect.Update(new Point(200, 150), 0.2);
                var next = AllShapes(effect).Single(s => s.Geometry is EllipseGeometry && s.Pen != null);
                Assert.AreEqual(first.Center, ((EllipseGeometry)next.Geometry).Center);
                Assert.IsTrue(((EllipseGeometry)next.Geometry).RadiusX > first.RadiusX);
                Assert.IsTrue(Groups(Drawing(effect)).Single(g => g.Children.OfType<GeometryDrawing>().Any(s => s.Geometry is EllipseGeometry && s.Pen != null)).Opacity < firstOpacity);
                Assert.AreSame(ripple.Pen, next.Pen);
                Assert.AreSame(water.Brush, AllShapes(effect).Single(s => s.Brush is RadialGradientBrush).Brush);
                var bounds = AllShapes(effect).Select(s => s.Geometry.Bounds).ToArray();
                effect.Update(new Point(-200, 800), double.Epsilon);
                CollectionAssert.AreEqual(bounds, AllShapes(effect).Select(s => s.Geometry.Bounds).ToArray());
                var transform = Groups(Drawing(effect)).Single(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value;
                Assert.AreEqual(-200.0, transform.OffsetX);
                Assert.AreEqual(800.0, transform.OffsetY);
                effect.Clear();
                Assert.AreEqual(0, effect.SplashCount);
                Assert.AreEqual(0.0, effect.WaterAmount);
                Assert.IsTrue(Drawing(effect) == null || Drawing(effect).Bounds.IsEmpty);
            });
        }

        [TestMethod]
        public void LongFramesDoNotReplayOldImpactsAndWaterEvaporates()
        {
            RunSta(() =>
            {
                var effect = new DrizzleCursorEffect();
                effect.Update(new Point(), 0.01);
                SetLandingDrop(effect);
                effect.Update(new Point(), 1);
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.AreEqual(0, effect.SplashCount);
                Assert.AreEqual(0.0, effect.WaterAmount);
                SetLandingDrop(effect);
                effect.Update(new Point(), 0.15);
                Assert.AreEqual(1, effect.SplashCount);
                effect.Update(new Point(), double.MaxValue);
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.AreEqual(0, effect.SplashCount);
                Assert.AreEqual(0.0, effect.WaterAmount);
                Assert.AreEqual(0, AllShapes(effect).Length);
                var preview = DrizzleCursorEffect.PreviewImage;
                var shapes = Groups(preview.Drawing).SelectMany(g => g.Children.OfType<GeometryDrawing>()).ToArray();
                Assert.IsTrue(preview.IsFrozen);
                Assert.AreEqual(new Rect(0, 0, 260, 170), preview.Drawing.Bounds);
                Assert.AreEqual(1, shapes.Count(s => s.Brush is RadialGradientBrush));
                Assert.AreEqual(5, shapes.Count(s => s.Geometry is EllipseGeometry && s.Pen != null));
            });
        }

        private static void SetLandingDrop(DrizzleCursorEffect effect)
        {
            var type = typeof(DrizzleCursorEffect).GetNestedType("Drop", BindingFlags.NonPublic)!;
            var drop = Activator.CreateInstance(type)!;
            type.GetField("Origin")!.SetValue(drop, new Point(0, 35));
            type.GetField("Velocity")!.SetValue(drop, new Vector(30, 60));
            type.GetField("Age")!.SetValue(drop, 0.4);
            type.GetField("Lifetime")!.SetValue(drop, 2.0);
            type.GetField("Length")!.SetValue(drop, 10.0);
            var particles = (System.Collections.IList)typeof(DrizzleCursorEffect).GetField("particles", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(effect)!;
            particles.Clear();
            particles.Add(drop);
        }

        private static DrawingGroup Drawing(DrizzleCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
        private static IEnumerable<DrawingGroup> Groups(Drawing drawing)
        {
            if (drawing is DrawingGroup group)
            {
                yield return group;
                foreach (var child in group.Children)
                    foreach (var nested in Groups(child)) yield return nested;
            }
        }
        private static GeometryDrawing[] AllShapes(DrizzleCursorEffect effect) => Groups(Drawing(effect)).SelectMany(g => g.Children.OfType<GeometryDrawing>()).ToArray();
        private static GeometryDrawing[] Shapes(DrizzleCursorEffect effect) => AllShapes(effect).Where(s => s.Geometry is LineGeometry).ToArray();
        private static long Alpha(DrizzleCursorEffect effect)
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
