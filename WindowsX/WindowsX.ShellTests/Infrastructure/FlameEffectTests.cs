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
    public class FlameEffectTests
    {
        [TestMethod]
        public void DefaultsAndIdentifiersRemainCompatible()
        {
            Assert.AreEqual(29, (int)MouseEffectType.Drizzle);
            Assert.AreEqual(30, (int)MouseEffectType.Flame);
            foreach (var json in new[] { "{}", "{\"MouseEffectSetting\":null}" })
                Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.FlameOpacity);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { FlameOpacity = value }.FlameOpacity);
        }

        [TestMethod]
        public void FlamesOrbitWhileStationaryAndReuseFrozenResources()
        {
            RunSta(() =>
            {
                var effect = new FlameCursorEffect();
                var cursor = new Point(200, 200);
                effect.Update(cursor, 0.01);
                Assert.AreEqual(6, effect.ParticleCount);
                Assert.IsFalse(effect.IsHitTestVisible);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                var shapes = Shapes(effect);
                Assert.AreEqual(FlameCursorEffect.FlameCount * (FlameCursorEffect.TailCount * 2 + 4), shapes.Length);
                Assert.IsTrue(shapes.All(s => s.Brush.IsFrozen && (s.Pen == null || s.Pen.IsFrozen)));
                Assert.AreEqual(36, shapes.Count(s => s.Brush is RadialGradientBrush));
                var flame = shapes.First(s => s.Geometry is StreamGeometry);
                Assert.IsTrue(flame.Geometry.IsFrozen);
                var heads = Heads(effect);
                Assert.AreEqual(6, heads.Length);
                foreach (var head in heads)
                    Assert.AreEqual(FlameCursorEffect.OrbitRadius, Math.Sqrt(head.OffsetX * head.OffsetX + head.OffsetY * head.OffsetY), 0.000001);
                effect.Update(cursor, FlameCursorEffect.CycleDuration / 4);
                var moved = Heads(effect);
                for (var i = 0; i < heads.Length; i++)
                {
                    Assert.AreEqual(-heads[i].OffsetY, moved[i].OffsetX, 0.000001);
                    Assert.AreEqual(heads[i].OffsetX, moved[i].OffsetY, 0.000001);
                }
                var next = Shapes(effect).First(s => s.Geometry is StreamGeometry);
                Assert.AreSame(flame.Geometry, next.Geometry);
                Assert.AreSame(flame.Brush, next.Brush);
                var preview = FlameCursorEffect.PreviewImage;
                Assert.IsTrue(preview.IsFrozen);
                Assert.AreEqual(new Rect(0, 0, 260, 170), preview.Drawing.Bounds);
            });
        }

        [TestMethod]
        public void CursorMovementAndLongFramesNeverIncreaseDrawCount()
        {
            RunSta(() =>
            {
                var effect = new FlameCursorEffect();
                effect.Update(new Point(100, 100), 0.01);
                var before = Heads(effect);
                effect.Update(new Point(10000, -2000), double.Epsilon);
                CollectionAssert.AreEqual(before, Heads(effect));
                var root = Groups(Drawing(effect)).Single(g => g.Transform is TranslateTransform).Transform.Value;
                Assert.AreEqual(10000.0, root.OffsetX);
                Assert.AreEqual(-2000.0, root.OffsetY);
                for (var i = 0; i < 100; i++)
                {
                    effect.Update(new Point(i * 100, 50), i % 2 == 0 ? double.MaxValue : 0.02);
                    Assert.AreEqual(6, effect.ParticleCount);
                    Assert.AreEqual(84, Shapes(effect).Length);
                    Assert.IsTrue(Heads(effect).All(m => double.IsFinite(m.OffsetX) && double.IsFinite(m.OffsetY) && double.IsFinite(m.M11)));
                }
            });
        }

        [TestMethod]
        public void InvalidInputsClearAndRestartAreSafe()
        {
            RunSta(() =>
            {
                var effect = new FlameCursorEffect();
                foreach (var dt in new[] { 0, -1, double.NaN, double.PositiveInfinity }) effect.Update(new Point(), dt);
                effect.Update(new Point(double.NaN, 0), 0.01);
                effect.Update(new Point(0, double.PositiveInfinity), 0.01);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(new Point(), 0.01);
                var first = Heads(effect);
                var visual = VisualTreeHelper.GetChild(effect, 0);
                effect.Clear();
                effect.Clear();
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.IsTrue(Drawing(effect) == null || Drawing(effect).Bounds.IsEmpty);
                effect.Update(new Point(), 0.01);
                CollectionAssert.AreEqual(first, Heads(effect));
                Assert.AreSame(visual, VisualTreeHelper.GetChild(effect, 0));
            });
        }

        [TestMethod]
        public void OpacityControlsFlamesGlowAndTailsLive()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new FlameCursorEffect(settings);
                effect.Measure(new Size(400, 400));
                effect.Arrange(new Rect(0, 0, 400, 400));
                effect.Update(new Point(200, 200), 0.01);
                var full = Alpha(effect);
                Assert.IsTrue(full > 0);
                settings.FlameOpacity = 0;
                effect.Update(new Point(200, 200), double.Epsilon);
                Assert.AreEqual(0L, Alpha(effect));
                settings.FlameOpacity = 0.5;
                effect.Update(new Point(200, 200), double.Epsilon);
                Assert.IsTrue(Alpha(effect) > 0 && Alpha(effect) < full);
                Assert.AreEqual(1.0, settings.DrizzleOpacity);
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
                    window.UpdateMouseEffectType(MouseEffectType.Flame);
                    var create = typeof(MouseEffectWindow).GetMethod("CreateEffects", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    create.Invoke(window, null);
                    create.Invoke(window, null);
                    var canvas = (Canvas)window.FindName("canvas");
                    Assert.AreEqual(1, canvas.Children.Count);
                    var effect = (FlameCursorEffect)canvas.Children[0];
                    effect.Update(new Point(100, 100), 0.01);
                    if (switchEffect) window.UpdateMouseEffectType(MouseEffectType.Drizzle);
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
                config.MouseEffectSetting.FlameOpacity = 0.6;
                config.MouseEffectSetting.FlameSizeScale = 2;
                config.MouseEffectSetting.FlameSpeed = 0.5;
                config.MouseEffectSetting.FlameRadius = 60;
                var window = new MouseEffectSettingWindow(MouseEffectType.Flame, config);
                try
                {
                    Assert.AreEqual("火焰设置", window.Title);
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_Flame")).Visibility);
                    var size = (Slider)window.FindName("slider_FlameSize");
                    var speed = (Slider)window.FindName("slider_FlameSpeed");
                    var radius = (Slider)window.FindName("slider_FlameRadius");
                    Assert.AreEqual(60.0, radius.Value);
                    Assert.AreEqual(200.0, size.Value);
                    Assert.AreEqual(50.0, speed.Value);
                    Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName("panel_Catkins")).Visibility);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(60.0, slider.Value);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    Assert.AreEqual(100.0, size.Value);
                    Assert.AreEqual(100.0, speed.Value);
                    Assert.AreEqual(34.0, radius.Value);
                    radius.Value = 90;
                    size.Value = 150;
                    speed.Value = 250;
                    slider.Value = 25;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.6, config.MouseEffectSetting.FlameOpacity);
                Assert.AreEqual(2.0, config.MouseEffectSetting.FlameSizeScale);
                Assert.AreEqual(0.5, config.MouseEffectSetting.FlameSpeed);
                Assert.AreEqual(60.0, config.MouseEffectSetting.FlameRadius);
            });
        }

        [TestMethod]
        public void DialogPersistsOnlyFlameAndKeepsSelection()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-Flame-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.DrizzleOpacity = 0.4;
                    settings.WillowCatkinsOpacity = 0.7;
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)MouseEffectType.Flame;
                    var window = new MouseEffectSettingWindow(MouseEffectType.Flame, config);
                    Exception? failure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            ((Slider)window.FindName("slider_Opacity")).Value = 25;
                            ((Slider)window.FindName("slider_FlameSize")).Value = 175;
                            ((Slider)window.FindName("slider_FlameSpeed")).Value = 225;
                            ((Slider)window.FindName("slider_FlameRadius")).Value = 85;
                            ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        catch (Exception ex) { failure = ex; }
                        finally { window.Close(); }
                    }, DispatcherPriority.Loaded);
                    Assert.AreEqual(true, window.ShowDialog());
                    if (failure != null) Assert.Fail(failure.ToString());
                    Assert.AreSame(settings, config.MouseEffectSetting);
                    var restored = ConfigHelper.LoadMainConfig().MouseEffectSetting;
                    Assert.AreEqual(0.25, restored.FlameOpacity);
                    Assert.AreEqual(1.75, restored.FlameSizeScale);
                    Assert.AreEqual(2.25, restored.FlameSpeed);
                    Assert.AreEqual(85.0, restored.FlameRadius);
                    Assert.AreEqual(0.4, restored.DrizzleOpacity);
                    Assert.AreEqual(0.7, restored.WillowCatkinsOpacity);
                    Assert.AreEqual(30, restored.SelectedEffectType);
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
        public void AnimeFlamesHaveOutlinesLayeredColorsHighlightsAndGlowingTails()
        {
            RunSta(() =>
            {
                var effect = new FlameCursorEffect();
                effect.Update(new Point(200, 200), 0.01);
                var shapes = Shapes(effect);
                var outlines = shapes.Where(s => s.Pen != null).ToArray();
                Assert.AreEqual(6, outlines.Length);
                Assert.IsTrue(outlines.All(s => s.Geometry is StreamGeometry && s.Pen.IsFrozen));
                Assert.IsTrue(outlines.All(s => ReferenceEquals(outlines[0].Pen, s.Pen)));
                var colors = (LinearGradientBrush)outlines[0].Brush;
                Assert.AreEqual(6, colors.GradientStops.Count);
                Assert.IsTrue(colors.GradientStops.All(s => s.Color.A == 255));
                Assert.AreNotEqual(colors.GradientStops[1].Color, colors.GradientStops[2].Color);
                Assert.AreEqual(6, shapes.Count(s => s.Geometry is EllipseGeometry && ReferenceEquals(s.Brush, Brushes.White)));
                var glows = shapes.Where(s => s.Brush is RadialGradientBrush).ToArray();
                Assert.AreEqual(36, glows.Length);
                var glow = (RadialGradientBrush)glows[0].Brush;
                Assert.AreEqual((byte)210, glow.GradientStops[0].Color.A);
                Assert.AreEqual((byte)85, glow.GradientStops[1].Color.A);
                Assert.AreEqual((byte)0, glow.GradientStops.Last().Color.A);
                Assert.IsTrue(glows.All(s => ReferenceEquals(glow, s.Brush)));
                Assert.IsTrue(Groups(Drawing(effect)).Count(g => g.Opacity > 0 && g.Opacity < 0.5) >= FlameCursorEffect.FlameCount * FlameCursorEffect.TailCount);
                Assert.IsNull(effect.Effect);
                effect.Update(new Point(200, 200), 0.03);
                var next = Shapes(effect);
                Assert.AreSame(outlines[0].Pen, next.First(s => s.Pen != null).Pen);
                Assert.AreSame(glow, next.First(s => s.Brush is RadialGradientBrush).Brush);
                var preview = FlameCursorEffect.PreviewImage;
                var previewShapes = Groups(preview.Drawing).SelectMany(g => g.Children.OfType<GeometryDrawing>()).ToArray();
                Assert.AreEqual(36, previewShapes.Count(s => s.Brush is RadialGradientBrush));
                Assert.AreEqual(6, previewShapes.Count(s => s.Pen != null));
            });
        }

        [TestMethod]
        public void AppearanceDefaultsAndPanelIsolationRemainCompatible()
        {
            foreach (var json in new[] { "{}", "{\"MouseEffectSetting\":null}" })
            {
                var settings = JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting;
                Assert.AreEqual(1.0, settings.FlameSizeScale);
                Assert.AreEqual(1.0, settings.FlameSpeed);
            }
            RunSta(() =>
            {
                var window = new MouseEffectSettingWindow(MouseEffectType.Drizzle, new MainConfig());
                try { Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName("panel_Flame")).Visibility); }
                finally { window.Close(); }
            });
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.5, 0.25)]
        [DataRow(10.0, 3.0, 3.0)]
        [DataRow(double.NaN, 1.0, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0, 1.0)]
        public void SizeAndSpeedAreNormalized(double value, double size, double speed)
        {
            var settings = new MouseEffectSetting { FlameSizeScale = value, FlameSpeed = value };
            Assert.AreEqual(size, settings.FlameSizeScale);
            Assert.AreEqual(speed, settings.FlameSpeed);
        }

        [DataTestMethod]
        [DataRow(0.5)]
        [DataRow(1.0)]
        [DataRow(3.0)]
        public void SizeScalesAllFlameLayersWithoutChangingOrbit(double size)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new FlameCursorEffect(settings);
                effect.Update(new Point(200, 200), 0.12);
                Matrix[] LocalTransforms() => Groups(Drawing(effect)).Where(g => g.Transform is MatrixTransform).Select(g => g.Transform.Value).ToArray();
                var before = LocalTransforms();
                Assert.AreEqual(36, before.Length);
                var shapes = Shapes(effect);
                settings.FlameSizeScale = size;
                effect.Update(new Point(200, 200), double.Epsilon);
                var after = LocalTransforms();
                for (var i = 0; i < before.Length; i++)
                {
                    Assert.AreEqual(before[i].OffsetX, after[i].OffsetX);
                    Assert.AreEqual(before[i].OffsetY, after[i].OffsetY);
                    Assert.AreEqual(before[i].M11 * size, after[i].M11, 0.000001);
                    Assert.AreEqual(before[i].M12 * size, after[i].M12, 0.000001);
                    Assert.AreEqual(before[i].M21 * size, after[i].M21, 0.000001);
                    Assert.AreEqual(before[i].M22 * size, after[i].M22, 0.000001);
                }
                var resized = Shapes(effect);
                Assert.AreEqual(84, resized.Length);
                for (var i = 0; i < shapes.Length; i++)
                {
                    Assert.AreEqual(shapes[i].Geometry.Bounds, resized[i].Geometry.Bounds);
                    Assert.AreSame(shapes[i].Brush, resized[i].Brush);
                    Assert.AreSame(shapes[i].Pen, resized[i].Pen);
                }
                settings.FlameSizeScale = 1;
                effect.Update(new Point(200, 200), double.Epsilon);
                CollectionAssert.AreEqual(before, LocalTransforms());
            });
        }

        [DataTestMethod]
        [DataRow(0.25)]
        [DataRow(1.0)]
        [DataRow(3.0)]
        public void SpeedChangesOnlyRotationAndPreservesPhase(double speed)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new FlameCursorEffect(settings);
                var baseline = new FlameCursorEffect();
                var cursor = new Point(200, 200);
                effect.Update(cursor, 0.1);
                baseline.Update(cursor, 0.1);
                var before = Heads(effect);
                settings.FlameSpeed = speed;
                effect.Update(cursor, double.Epsilon);
                CollectionAssert.AreEqual(before, Heads(effect));
                effect.Update(cursor, 0.2);
                baseline.Update(cursor, 0.2);
                var angle = (0.1 + 0.2 * speed) * Math.PI * 2 / FlameCursorEffect.CycleDuration;
                Assert.AreEqual(Math.Cos(angle) * FlameCursorEffect.OrbitRadius, Heads(effect)[0].OffsetX, 0.000001);
                Assert.AreEqual(Math.Sin(angle) * FlameCursorEffect.OrbitRadius, Heads(effect)[0].OffsetY, 0.000001);
                var actual = Heads(effect);
                var normal = Heads(baseline);
                for (var i = 0; i < actual.Length; i++)
                {
                    Assert.AreEqual(Math.Sqrt(normal[i].M11 * normal[i].M11 + normal[i].M12 * normal[i].M12),
                        Math.Sqrt(actual[i].M11 * actual[i].M11 + actual[i].M12 * actual[i].M12), 0.000001);
                }
                CollectionAssert.AreEqual(Groups(Drawing(baseline)).Select(g => g.Opacity).ToArray(), Groups(Drawing(effect)).Select(g => g.Opacity).ToArray());
                effect.Update(cursor, double.MaxValue);
                Assert.IsTrue(Heads(effect).All(m => double.IsFinite(m.OffsetX) && double.IsFinite(m.OffsetY)));
                effect.Clear();
                effect.Update(cursor, 0.1);
                var fresh = new FlameCursorEffect(settings);
                fresh.Update(cursor, 0.1);
                CollectionAssert.AreEqual(Heads(fresh), Heads(effect));
            });
        }

        [DataTestMethod]
        [DataRow(-1.0, 10.0)]
        [DataRow(200.0, 120.0)]
        [DataRow(double.NaN, 34.0)]
        [DataRow(double.PositiveInfinity, 34.0)]
        [DataRow(double.NegativeInfinity, 34.0)]
        public void RadiusIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { FlameRadius = value }.FlameRadius);
        }

        [DataTestMethod]
        [DataRow(10.0)]
        [DataRow(34.0)]
        [DataRow(120.0)]
        public void RadiusChangesAllOrbitPositionsWithoutChangingAppearance(double radius)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting { FlameSizeScale = 1.75, FlameSpeed = 2.25 };
                var effect = new FlameCursorEffect(settings);
                var cursor = new Point(200, 200);
                effect.Update(cursor, 0.12);
                Matrix[] LocalTransforms() => Groups(Drawing(effect)).Where(g => g.Transform is MatrixTransform).Select(g => g.Transform.Value).ToArray();
                var before = LocalTransforms();
                var shapes = Shapes(effect);
                var opacity = Groups(Drawing(effect)).Select(g => g.Opacity).ToArray();
                settings.FlameRadius = radius;
                effect.Update(cursor, double.Epsilon);
                var after = LocalTransforms();
                Assert.AreEqual(36, after.Length);
                for (var i = 0; i < after.Length; i++)
                {
                    Assert.AreEqual(before[i].OffsetX * radius / 34, after[i].OffsetX, 0.000001);
                    Assert.AreEqual(before[i].OffsetY * radius / 34, after[i].OffsetY, 0.000001);
                    Assert.AreEqual(before[i].M11, after[i].M11);
                    Assert.AreEqual(before[i].M12, after[i].M12);
                    Assert.AreEqual(before[i].M21, after[i].M21);
                    Assert.AreEqual(before[i].M22, after[i].M22);
                }
                var resized = Shapes(effect);
                Assert.AreEqual(84, resized.Length);
                for (var i = 0; i < shapes.Length; i++)
                {
                    Assert.AreEqual(shapes[i].Geometry.Bounds, resized[i].Geometry.Bounds);
                    Assert.AreSame(shapes[i].Brush, resized[i].Brush);
                    Assert.AreSame(shapes[i].Pen, resized[i].Pen);
                }
                CollectionAssert.AreEqual(opacity, Groups(Drawing(effect)).Select(g => g.Opacity).ToArray());
                settings.FlameRadius = MouseEffectSetting.DefaultFlameRadius;
                effect.Update(cursor, double.Epsilon);
                CollectionAssert.AreEqual(before, LocalTransforms());
                var baseline = new FlameCursorEffect(new MouseEffectSetting { FlameSizeScale = 1.75, FlameSpeed = 2.25 });
                baseline.Update(cursor, 0.12);
                effect.Update(cursor, 0.2);
                baseline.Update(cursor, 0.2);
                CollectionAssert.AreEqual(Heads(baseline), Heads(effect));
            });
        }

        [TestMethod]
        public void MissingRadiusKeepsDefaultAppearance()
        {
            foreach (var json in new[] { "{}", "{\"MouseEffectSetting\":{}}", "{\"MouseEffectSetting\":null}" })
                Assert.AreEqual(34.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.FlameRadius);
            RunSta(() =>
            {
                var preview = FlameCursorEffect.PreviewImage;
                Assert.IsTrue(preview.IsFrozen);
                Assert.AreEqual(new Rect(0, 0, 260, 170), preview.Drawing.Bounds);
            });
        }

        private static DrawingGroup Drawing(FlameCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
        private static IEnumerable<DrawingGroup> Groups(Drawing drawing)
        {
            if (drawing is DrawingGroup group)
            {
                yield return group;
                foreach (var child in group.Children)
                    foreach (var nested in Groups(child)) yield return nested;
            }
        }
        private static GeometryDrawing[] Shapes(FlameCursorEffect effect) => Groups(Drawing(effect)).SelectMany(g => g.Children.OfType<GeometryDrawing>()).ToArray();
        private static Matrix[] Heads(FlameCursorEffect effect) => Groups(Drawing(effect))
            .Where(g => g.Transform != null && g.Children.OfType<GeometryDrawing>().Any(s => s.Pen != null))
            .Select(g => g.Transform.Value).ToArray();
        private static long Alpha(FlameCursorEffect effect)
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
