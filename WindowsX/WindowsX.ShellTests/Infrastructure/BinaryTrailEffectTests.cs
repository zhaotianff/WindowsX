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
    public class BinaryTrailEffectTests
    {
        [TestMethod]
        public void DefaultsAndTypesRemainCompatible()
        {
            Assert.AreEqual(24, (int)MouseEffectType.MusicNotes);
            Assert.AreEqual(25, (int)MouseEffectType.BinaryTrail);
            foreach (var json in new[] { "{}", "{\"MouseEffectSetting\":null}" })
                Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.BinaryTrailOpacity);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { BinaryTrailOpacity = value }.BinaryTrailOpacity);
        }

        [TestMethod]
        public void DigitsUseFrozenPathsAndLayeredGlowAlongMovement()
        {
            RunSta(() =>
            {
                var effect = new BinaryTrailCursorEffect();
                effect.Update(new Point(100, 100), 0.01);
                effect.Update(new Point(156, 100), 0.04);
                Assert.AreEqual(3, effect.ParticleCount);
                var shapes = Groups(Drawing(effect)).SelectMany(g => g.Children.OfType<GeometryDrawing>()).ToArray();
                Assert.AreEqual(15, shapes.Length);
                var strokes = shapes.Where(s => s.Pen != null).ToArray();
                var halos = shapes.Where(s => s.Brush is RadialGradientBrush).ToArray();
                Assert.AreEqual(12, strokes.Length);
                Assert.AreEqual(3, halos.Length);
                Assert.AreEqual(2, strokes.Select(s => s.Geometry).Distinct().Count());
                Assert.IsTrue(strokes.All(s => s.Brush == null && s.Pen.IsFrozen && s.Geometry.IsFrozen));
                Assert.IsTrue(halos.All(s => s.Brush.IsFrozen && s.Geometry is EllipseGeometry && ((RadialGradientBrush)s.Brush).GradientStops.Last().Color.A == 0));
                CollectionAssert.AreEqual(new[] { 7.0, 3.5, 1.8, 0.6 }, strokes.Take(4).Select(s => s.Pen.Thickness).ToArray());
                Assert.AreEqual(Color.FromRgb(225, 255, 242), ((SolidColorBrush)strokes[3].Pen.Brush).Color);
                var zero = PathGeometry.CreateFromGeometry(strokes[0].Geometry);
                var one = PathGeometry.CreateFromGeometry(strokes[4].Geometry);
                Assert.AreEqual(1, zero.Figures.Count);
                Assert.IsTrue(zero.Figures[0].IsClosed);
                Assert.AreEqual(2, one.Figures.Count);
                var positions = Groups(Drawing(effect)).Where(g => g.Transform != null && !g.Transform.Value.IsIdentity).Select(g => g.Transform.Value).ToArray();
                CollectionAssert.AreEqual(new[] { 132.0, 156.0 }, positions.Skip(1).Select(m => m.OffsetX).ToArray());
                Assert.IsTrue(positions.All(m => m.M12 == 0 && m.M21 == 0));
                Assert.IsFalse(effect.IsHitTestVisible);
                Assert.IsTrue(BinaryTrailCursorEffect.PreviewImage.IsFrozen);
                Assert.AreEqual(new Rect(0, 0, 260, 170), BinaryTrailCursorEffect.PreviewImage.Drawing.Bounds);
            });
        }

        [TestMethod]
        public void HalosPulseAndReuseBrushesWithoutChangingParticleCount()
        {
            RunSta(() =>
            {
                var effect = new BinaryTrailCursorEffect();
                var cursor = new Point(200, 200);
                effect.Update(cursor, 0.01);
                DrawingGroup Halo() => Groups(Drawing(effect)).Single(g => g.Children.OfType<GeometryDrawing>().Any(s => s.Brush is RadialGradientBrush));
                var first = Halo();
                var brush = first.Children.OfType<GeometryDrawing>().Single().Brush;
                effect.Update(cursor, 0.2);
                var second = Halo();
                Assert.AreNotEqual(first.Opacity, second.Opacity);
                Assert.IsTrue(second.Opacity >= 0.7 && second.Opacity <= 1);
                Assert.AreSame(brush, second.Children.OfType<GeometryDrawing>().Single().Brush);
                Assert.AreEqual(1, effect.ParticleCount);
                var previewHalos = Groups(BinaryTrailCursorEffect.PreviewImage.Drawing).SelectMany(g => g.Children.OfType<GeometryDrawing>()).Count(s => s.Brush is RadialGradientBrush);
                Assert.AreEqual(9, previewHalos);
            });
        }

        [TestMethod]
        public void DigitsDriftFadeAndExpireWithoutIdleEmission()
        {
            RunSta(() =>
            {
                var effect = new BinaryTrailCursorEffect();
                var cursor = new Point(200, 200);
                effect.Update(cursor, 0.01);
                effect.Update(cursor, 0.5);
                Assert.AreEqual(1, effect.ParticleCount);
                var transform = Groups(Drawing(effect)).First(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value;
                Assert.IsTrue(transform.OffsetY > 200);
                Assert.IsTrue(Groups(Drawing(effect)).Any(g => g.Opacity > 0 && g.Opacity < 1));
                effect.Update(cursor, 2);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(new Point(1000, 200), 0.01);
                Assert.AreEqual(1, effect.ParticleCount);
                Assert.AreEqual(1000.0, Groups(Drawing(effect)).First(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value.OffsetX);
            });
        }

        [TestMethod]
        public void FastMovementInvalidInputAndClearStayBounded()
        {
            RunSta(() =>
            {
                var effect = new BinaryTrailCursorEffect();
                foreach (var dt in new[] { 0.0, -1, double.NaN, double.PositiveInfinity }) effect.Update(new Point(100, 100), dt);
                effect.Update(new Point(double.NaN, 100), 0.01);
                effect.Update(new Point(100, double.PositiveInfinity), 0.01);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(new Point(100, 100), 0.01);
                effect.Update(new Point(10000, 100), 0.04);
                Assert.AreEqual(1 + BinaryTrailCursorEffect.MaxEmissionPerUpdate, effect.ParticleCount);
                for (var i = 0; i < 1000; i++) effect.Update(new Point(i % 2 * 10000, 100), 0.04);
                Assert.IsTrue(effect.ParticleCount <= BinaryTrailCursorEffect.MaxParticles);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                var visual = VisualTreeHelper.GetChild(effect, 0);
                effect.Update(new Point(100, 100), double.MaxValue);
                Assert.IsTrue(effect.ParticleCount <= 1);
                effect.Clear();
                effect.Clear();
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.IsTrue(Drawing(effect) == null || Drawing(effect).Bounds.IsEmpty);
                effect.Update(new Point(100, 100), 0.01);
                Assert.AreEqual(1, effect.ParticleCount);
                Assert.AreSame(visual, VisualTreeHelper.GetChild(effect, 0));
            });
        }

        [TestMethod]
        public void FastMovementKeepsOnlyLatest24DigitsAndAtMost120Shapes()
        {
            RunSta(() =>
            {
                Assert.AreEqual(24, BinaryTrailCursorEffect.MaxParticles);
                Assert.AreEqual(2, BinaryTrailCursorEffect.MaxEmissionPerUpdate);
                var effect = new BinaryTrailCursorEffect();
                effect.Update(new Point(100, 100), 0.01);
                for (var i = 1; i <= 12; i++)
                {
                    effect.Update(new Point(100 + i * 1000, 100), 0.04);
                    Assert.IsTrue(effect.ParticleCount <= 24);
                    var positions = Groups(Drawing(effect)).Where(g => g.Transform != null && !g.Transform.Value.IsIdentity).Select(g => g.Transform.Value.OffsetX).ToArray();
                    Assert.AreEqual(100 + i * 1000 - 24, positions[^2], 0.000001);
                    Assert.AreEqual(100 + i * 1000, positions[^1], 0.000001);
                    Assert.IsTrue(Groups(Drawing(effect)).SelectMany(g => g.Children.OfType<GeometryDrawing>()).Count() <= 120);
                }
                Assert.AreEqual(24, effect.ParticleCount);
                var remaining = Groups(Drawing(effect)).Where(g => g.Transform != null && !g.Transform.Value.IsIdentity).Select(g => g.Transform.Value.OffsetX).ToArray();
                Assert.IsTrue(remaining.All(x => x > 1000));
                effect.Update(new Point(12100, 100), 0.86);
                Assert.AreEqual(0, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void EmissionRequiresBothDistanceAndTimeAndDoesNotBurstAfterPause()
        {
            RunSta(() =>
            {
                var effect = new BinaryTrailCursorEffect();
                effect.Update(new Point(100, 100), 0.01);
                effect.Update(new Point(120, 100), 0.05);
                Assert.AreEqual(1, effect.ParticleCount);
                effect.Update(new Point(124, 100), 0.01);
                Assert.AreEqual(2, effect.ParticleCount);
                effect.Update(new Point(1000, 100), 0.01);
                Assert.AreEqual(2, effect.ParticleCount);
                effect.Update(new Point(1000, 100), 0.04);
                Assert.AreEqual(4, effect.ParticleCount);
                effect.Update(new Point(2000, 100), 2);
                Assert.AreEqual(1, effect.ParticleCount);
                Assert.AreEqual(2000.0, Groups(Drawing(effect)).First(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value.OffsetX);
            });
        }

        [TestMethod]
        public void OpacityHidesAllGlowLayersLive()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new BinaryTrailCursorEffect(settings);
                effect.Measure(new Size(400, 400));
                effect.Arrange(new Rect(0, 0, 400, 400));
                effect.Update(new Point(200, 200), 0.01);
                Assert.IsTrue(Alpha(effect) > 0);
                settings.BinaryTrailOpacity = 0;
                effect.Update(new Point(200, 200), 0.01);
                Assert.AreEqual(0L, Alpha(effect));
                settings.BinaryTrailOpacity = 0.5;
                effect.Update(new Point(200, 200), 0.01);
                Assert.IsTrue(Alpha(effect) > 0);
            });
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void WindowCreatesOneEffectAndClearsIt(bool switchEffect)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                try
                {
                    window.StartMouseEffect();
                    window.UpdateMouseEffectType(MouseEffectType.BinaryTrail);
                    var create = typeof(MouseEffectWindow).GetMethod("CreateEffects", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    create.Invoke(window, null);
                    create.Invoke(window, null);
                    var canvas = (Canvas)window.FindName("canvas");
                    Assert.AreEqual(1, canvas.Children.Count);
                    var effect = (BinaryTrailCursorEffect)canvas.Children[0];
                    effect.Update(new Point(100, 100), 0.01);
                    if (switchEffect) window.UpdateMouseEffectType(MouseEffectType.MusicNotes);
                    else window.StopMouseEffect();
                    Assert.AreEqual(0, effect.ParticleCount);
                    Assert.AreEqual(0, canvas.Children.Count);
                }
                finally { window.StopMouseEffect(); window.Close(); }
            });
        }

        [TestMethod]
        public void DialogResetAndCancelPreserveSettings()
        {
            RunSta(() =>
            {
                var config = new MainConfig();
                config.MouseEffectSetting.BinaryTrailOpacity = 0.6;
                var window = new MouseEffectSettingWindow(MouseEffectType.BinaryTrail, config);
                try
                {
                    Assert.AreEqual("二进制字符光轨设置", window.Title);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(60.0, slider.Value);
                    Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName("panel_Cloud")).Visibility);
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_ParticleOpacity")).Visibility);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    slider.Value = 20;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.6, config.MouseEffectSetting.BinaryTrailOpacity);
            });
        }

        [TestMethod]
        public void DialogPersistsOnlyBinaryTrailAndKeepsSelection()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-BinaryTrail-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.MusicNotesOpacity = 0.4;
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)MouseEffectType.BinaryTrail;
                    var window = new MouseEffectSettingWindow(MouseEffectType.BinaryTrail, config);
                    Exception? failure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            ((Slider)window.FindName("slider_Opacity")).Value = 25;
                            ((CheckBox)window.FindName("check_BinaryTrailDefaultColors")).IsChecked = false;
                            ((TextBox)window.FindName("text_BinaryTrailColor")).Text = "#1234ab";
                            ((Slider)window.FindName("slider_BinaryTrailSize")).Value = 175;
                            ((ComboBox)window.FindName("combo_BinaryTrailFont")).SelectedValue = "Courier New";
                            ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        catch (Exception ex) { failure = ex; }
                        finally { window.Close(); }
                    }, DispatcherPriority.Loaded);
                    Assert.AreEqual(true, window.ShowDialog());
                    if (failure != null) Assert.Fail(failure.ToString());
                    Assert.AreSame(settings, config.MouseEffectSetting);
                    var restored = ConfigHelper.LoadMainConfig().MouseEffectSetting;
                    Assert.AreEqual(0.25, restored.BinaryTrailOpacity);
                    Assert.IsFalse(restored.BinaryTrailUseDefaultColors);
                    Assert.AreEqual("#1234AB", restored.BinaryTrailColor);
                    Assert.AreEqual(1.75, restored.BinaryTrailSizeScale);
                    Assert.AreEqual("Courier New", restored.BinaryTrailFontFamily);
                    Assert.AreEqual(0.4, restored.MusicNotesOpacity);
                    Assert.AreEqual(25, restored.SelectedEffectType);
                    Assert.IsTrue(restored.IsEnabled);
                }
                finally
                {
                    Environment.CurrentDirectory = original;
                    System.IO.Directory.Delete(directory, true);
                }
            });
        }

        private static DrawingGroup Drawing(BinaryTrailCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
        [TestMethod]
        public void AppearanceDefaultsAndValidationRemainCompatible()
        {
            var settings = JsonConvert.DeserializeObject<MouseEffectSetting>("{}")!;
            Assert.IsTrue(settings.BinaryTrailUseDefaultColors);
            Assert.AreEqual("Vector", settings.BinaryTrailFontFamily);
            Assert.AreEqual("#5CFF8F", settings.BinaryTrailColor);
            Assert.AreEqual(1.0, settings.BinaryTrailSizeScale);
            foreach (var value in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
            {
                settings.BinaryTrailSizeScale = value;
                Assert.AreEqual(1.0, settings.BinaryTrailSizeScale);
            }
            settings.BinaryTrailSizeScale = -1;
            Assert.AreEqual(0.5, settings.BinaryTrailSizeScale);
            settings.BinaryTrailSizeScale = 10;
            Assert.AreEqual(3.0, settings.BinaryTrailSizeScale);
            settings.BinaryTrailColor = " #ab1234 ";
            Assert.AreEqual("#AB1234", settings.BinaryTrailColor);
            settings.BinaryTrailColor = "invalid";
            Assert.AreEqual(MouseEffectSetting.DefaultBinaryTrailColor, settings.BinaryTrailColor);
            settings.BinaryTrailFontFamily = " consolas ";
            Assert.AreEqual("Consolas", settings.BinaryTrailFontFamily);
            settings.BinaryTrailFontFamily = "MissingFont";
            Assert.AreEqual("Vector", settings.BinaryTrailFontFamily);
        }

        [TestMethod]
        public void AppearanceResourcesAreFrozenCachedAndRestored()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new BinaryTrailCursorEffect(settings);
                var cursor = new Point(200, 200);
                GeometryDrawing[] Shapes() => Groups(Drawing(effect)).SelectMany(g => g.Children.OfType<GeometryDrawing>())
                    .OrderByDescending(s => s.Pen?.Thickness ?? double.PositiveInfinity).ToArray();
                effect.Update(cursor, 0.01);
                var defaults = Shapes();
                settings.BinaryTrailUseDefaultColors = false;
                settings.BinaryTrailColor = "#FF0080";
                settings.BinaryTrailFontFamily = "Consolas";
                effect.Update(cursor, 0.01);
                var custom = Shapes();
                Assert.AreEqual(5, custom.Length);
                Assert.AreEqual(Color.FromRgb(255, 0, 128), ((SolidColorBrush)custom[3].Pen.Brush).Color);
                Assert.AreEqual((byte)255, ((RadialGradientBrush)custom[0].Brush).GradientStops[0].Color.R);
                Assert.IsTrue(custom.Skip(1).All(s => s.Geometry.IsFrozen && s.Pen.IsFrozen && s.Brush != null));
                Assert.AreNotSame(defaults[1].Geometry, custom[1].Geometry);
                var bounds = custom[1].Geometry.Bounds;
                Assert.AreEqual(0, bounds.X + bounds.Width / 2, 0.00001);
                Assert.AreEqual(0, bounds.Y + bounds.Height / 2, 0.00001);
                effect.Update(cursor, 0.01);
                var cached = Shapes();
                Assert.AreSame(custom[0].Brush, cached[0].Brush);
                Assert.AreSame(custom[1].Geometry, cached[1].Geometry);
                Assert.AreSame(custom[1].Pen, cached[1].Pen);
                settings.BinaryTrailUseDefaultColors = true;
                settings.BinaryTrailFontFamily = "Vector";
                effect.Update(cursor, 0.01);
                Assert.AreSame(defaults[1].Geometry, Shapes()[1].Geometry);
                Assert.AreSame(defaults[0].Brush, Shapes()[0].Brush);
            });
        }

        [TestMethod]
        public void AllFontsKeepSparseDrawingBudget()
        {
            RunSta(() =>
            {
                foreach (var font in MouseEffectSetting.BinaryTrailFontFamilies)
                {
                    var effect = new BinaryTrailCursorEffect(new MouseEffectSetting { BinaryTrailFontFamily = font });
                    for (var i = 0; i < 13; i++) effect.Update(new Point(100 + i * 1000, 100), 0.04);
                    Assert.AreEqual(24, effect.ParticleCount);
                    var shapes = Groups(Drawing(effect)).SelectMany(g => g.Children.OfType<GeometryDrawing>()).ToArray();
                    Assert.AreEqual(120, shapes.Length);
                    Assert.AreEqual(2, shapes.Where(s => s.Pen != null).Select(s => s.Geometry).Distinct().Count());
                }
            });
        }

        [TestMethod]
        public void SizeChangesScaleWithoutMovingParticleCenter()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new BinaryTrailCursorEffect(settings);
                var cursor = new Point(200, 200);
                Matrix Transform() => Groups(Drawing(effect)).First(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value;
                effect.Update(cursor, 0.01);
                var before = Transform();
                settings.BinaryTrailSizeScale = 3;
                effect.Update(cursor, double.Epsilon);
                var after = Transform();
                Assert.AreEqual(before.M11 * 3, after.M11, 0.000001);
                Assert.AreEqual(before.OffsetX, after.OffsetX);
                Assert.AreEqual(before.OffsetY, after.OffsetY);
                Assert.AreEqual(1, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void DialogValidatesColorAndResetsAppearanceDraftOnly()
        {
            RunSta(() =>
            {
                var config = new MainConfig();
                var settings = config.MouseEffectSetting;
                settings.BinaryTrailUseDefaultColors = false;
                settings.BinaryTrailColor = "#FF0080";
                settings.BinaryTrailSizeScale = 2;
                settings.BinaryTrailFontFamily = "Arial";
                var window = new MouseEffectSettingWindow(MouseEffectType.BinaryTrail, config);
                try
                {
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_BinaryTrail")).Visibility);
                    var color = (TextBox)window.FindName("text_BinaryTrailColor");
                    var size = (Slider)window.FindName("slider_BinaryTrailSize");
                    var font = (ComboBox)window.FindName("combo_BinaryTrailFont");
                    var mode = (CheckBox)window.FindName("check_BinaryTrailDefaultColors");
                    var save = (Button)window.FindName("btn_Save");
                    Assert.AreEqual(200.0, size.Value);
                    Assert.AreEqual("Arial", font.SelectedValue);
                    color.Text = "bad";
                    Assert.IsFalse(save.IsEnabled);
                    mode.IsChecked = true;
                    Assert.IsTrue(save.IsEnabled);
                    Assert.IsFalse(color.IsEnabled);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, size.Value);
                    Assert.AreEqual("Vector", font.SelectedValue);
                    Assert.AreEqual(MouseEffectSetting.DefaultBinaryTrailColor, color.Text);
                }
                finally { window.Close(); }
                Assert.AreEqual(2.0, settings.BinaryTrailSizeScale);
                Assert.AreEqual("Arial", settings.BinaryTrailFontFamily);
                Assert.AreEqual("#FF0080", settings.BinaryTrailColor);
                Assert.IsFalse(settings.BinaryTrailUseDefaultColors);
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
        private static long Alpha(BinaryTrailCursorEffect effect)
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
