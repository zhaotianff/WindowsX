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
    public class SwordAuraEffectTests
    {
        [TestMethod]
        public void DefaultsAndIdentifiersRemainCompatible()
        {
            Assert.AreEqual(27, (int)MouseEffectType.StarlightArrow);
            Assert.AreEqual(28, (int)MouseEffectType.SwordAura);
            foreach (var json in new[] { "{}", "{\"MouseEffectSetting\":null}" })
                Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.SwordAuraOpacity);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { SwordAuraOpacity = value }.SwordAuraOpacity);
        }

        [DataTestMethod]
        [DataRow(56.0, 0.0)]
        [DataRow(0.0, 56.0)]
        [DataRow(-56.0, 0.0)]
        public void AuraFollowsMovementWithoutSwordGeometry(double dx, double dy)
        {
            RunSta(() =>
            {
                var effect = new SwordAuraCursorEffect();
                var start = new Point(200, 200);
                effect.Update(start, 0.01);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(start + new Vector(dx, dy), 0.03);
                Assert.AreEqual(4, effect.ParticleCount);
                var shapes = Shapes(effect);
                Assert.AreEqual(8, shapes.Length);
                Assert.IsTrue(shapes.All(s => s.Geometry is EllipseGeometry && s.Brush is RadialGradientBrush && s.Brush.IsFrozen && s.Pen == null));
                var transform = Transforms(effect).Last();
                Assert.AreEqual(start.X + dx, transform.OffsetX, 0.000001);
                Assert.AreEqual(start.Y + dy, transform.OffsetY, 0.000001);
                var direction = new Vector(transform.M11, transform.M12);
                direction.Normalize();
                Assert.AreEqual(dx / 56, direction.X, 0.000001);
                Assert.AreEqual(dy / 56, direction.Y, 0.000001);
                var leadingMist = (EllipseGeometry)shapes[0].Geometry;
                Assert.IsTrue(leadingMist.RadiusX > leadingMist.RadiusY * 2);
                Assert.IsFalse(effect.IsHitTestVisible);
                Assert.IsTrue(SwordAuraCursorEffect.PreviewImage.IsFrozen);
                Assert.AreEqual(new Rect(0, 0, 260, 170), SwordAuraCursorEffect.PreviewImage.Drawing.Bounds);
                var previewShapes = Groups(SwordAuraCursorEffect.PreviewImage.Drawing).SelectMany(g => g.Children.OfType<GeometryDrawing>()).ToArray();
                Assert.IsFalse(previewShapes.Any(s => s.Geometry is StreamGeometry || s.Pen != null));
                Assert.AreEqual(18, previewShapes.Count(s => s.Geometry is EllipseGeometry));
            });
        }

        [TestMethod]
        public void AuraDriftsExpandsFadesAndExpiresWithoutIdleEmission()
        {
            RunSta(() =>
            {
                var effect = new SwordAuraCursorEffect();
                effect.Update(new Point(100, 100), 0.01);
                effect.Update(new Point(110, 100), 0.03);
                var before = Transforms(effect).Single();
                var shapes = Shapes(effect);
                double Fade() => Groups(Drawing(effect)).Min(g => g.Opacity);
                var initialFade = Fade();
                effect.Update(new Point(110, 100), 0.2);
                var after = Transforms(effect).Single();
                Assert.AreEqual(1, effect.ParticleCount);
                Assert.IsTrue(after.OffsetX > before.OffsetX);
                Assert.IsTrue(((EllipseGeometry)Shapes(effect)[0].Geometry).RadiusY > ((EllipseGeometry)shapes[0].Geometry).RadiusY);
                Assert.IsTrue(Fade() > 0 && Fade() < initialFade);
                Assert.AreSame(shapes[0].Brush, Shapes(effect)[0].Brush);
                effect.Update(new Point(110, 100), 1);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(new Point(200, 100), 0.03);
                Assert.AreEqual(1, effect.ParticleCount);
                Assert.AreEqual(200.0, Transforms(effect).Single().OffsetX);
            });
        }

        [TestMethod]
        public void InvalidInputRapidMovementAndClearRemainBounded()
        {
            RunSta(() =>
            {
                var effect = new SwordAuraCursorEffect();
                foreach (var dt in new[] { 0.0, -1, double.NaN, double.PositiveInfinity }) effect.Update(new Point(100, 100), dt);
                effect.Update(new Point(double.NaN, 100), 0.01);
                effect.Update(new Point(100, double.PositiveInfinity), 0.01);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(new Point(100, 100), 0.01);
                effect.Update(new Point(102, 100), 0.03);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(new Point(300, 100), 0.03);
                Assert.AreEqual(SwordAuraCursorEffect.MaxEmissionPerUpdate, effect.ParticleCount);
                for (var i = 0; i < 100; i++) effect.Update(new Point(300 + i * 200, 100), 0.02);
                Assert.AreEqual(SwordAuraCursorEffect.MaxParticles, effect.ParticleCount);
                Assert.IsTrue(Shapes(effect).Length <= 64 * 2);
                var count = effect.ParticleCount;
                effect.Update(new Point(100000, 100), 0.02);
                Assert.IsTrue(effect.ParticleCount <= count + 1);
                Assert.AreEqual(100000.0, Transforms(effect).Last().OffsetX);
                effect.Update(new Point(100000, 100), double.MaxValue);
                Assert.AreEqual(0, effect.ParticleCount);
                var visual = VisualTreeHelper.GetChild(effect, 0);
                effect.Clear();
                effect.Clear();
                Assert.IsTrue(Drawing(effect) == null || Drawing(effect).Bounds.IsEmpty);
                effect.Update(new Point(100, 100), 0.01);
                effect.Update(new Point(120, 100), 0.03);
                Assert.AreSame(visual, VisualTreeHelper.GetChild(effect, 0));
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
            });
        }

        [TestMethod]
        public void OpacityAppliesToAllLayersLive()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new SwordAuraCursorEffect(settings);
                effect.Measure(new Size(400, 400));
                effect.Arrange(new Rect(0, 0, 400, 400));
                effect.Update(new Point(180, 200), 0.01);
                effect.Update(new Point(220, 200), 0.03);
                Assert.IsTrue(Alpha(effect) > 0);
                settings.SwordAuraOpacity = 0;
                effect.Update(new Point(220, 200), 0.01);
                Assert.AreEqual(0L, Alpha(effect));
                settings.SwordAuraOpacity = 0.5;
                effect.Update(new Point(220, 200), 0.01);
                Assert.IsTrue(Alpha(effect) > 0);
                Assert.AreEqual(1.0, settings.InkFluidOpacity);
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
                    window.UpdateMouseEffectType(MouseEffectType.SwordAura);
                    var create = typeof(MouseEffectWindow).GetMethod("CreateEffects", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    create.Invoke(window, null);
                    create.Invoke(window, null);
                    var canvas = (Canvas)window.FindName("canvas");
                    Assert.AreEqual(1, canvas.Children.Count);
                    var effect = (SwordAuraCursorEffect)canvas.Children[0];
                    effect.Update(new Point(100, 100), 0.01);
                    effect.Update(new Point(120, 100), 0.03);
                    if (switchEffect) window.UpdateMouseEffectType(MouseEffectType.InkFluid);
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
                config.MouseEffectSetting.SwordAuraOpacity = 0.6;
                config.MouseEffectSetting.SwordAuraSizeScale = 2;
                config.MouseEffectSetting.SwordAuraColor1 = "#123456";
                var window = new MouseEffectSettingWindow(MouseEffectType.SwordAura, config);
                try
                {
                    Assert.AreEqual("剑意设置", window.Title);
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_SwordAura")).Visibility);
                    var size = (Slider)window.FindName("slider_SwordAuraSize");
                    Assert.AreEqual(200.0, size.Value);
                    Assert.AreEqual("#123456", ((TextBox)window.FindName("text_SwordAuraColor1")).Text);
                    foreach (var name in new[] { "text_SwordAuraColor1", "text_SwordAuraColor2", "text_SwordAuraColor3" })
                    {
                        var color = (TextBox)window.FindName(name);
                        color.Text = "bad";
                        var save = (Button)window.FindName("btn_Save");
                        Assert.IsFalse(save.IsEnabled);
                        save.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        Assert.AreEqual("#123456", config.MouseEffectSetting.SwordAuraColor1);
                        color.Text = "#112233";
                        Assert.IsTrue(save.IsEnabled);
                    }
                    Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName("panel_InkFluid")).Visibility);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(60.0, slider.Value);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    Assert.AreEqual(100.0, size.Value);
                    Assert.AreEqual(MouseEffectSetting.DefaultSwordAuraColor1, ((TextBox)window.FindName("text_SwordAuraColor1")).Text);
                    Assert.AreEqual(MouseEffectSetting.DefaultSwordAuraColor2, ((TextBox)window.FindName("text_SwordAuraColor2")).Text);
                    Assert.AreEqual(MouseEffectSetting.DefaultSwordAuraColor3, ((TextBox)window.FindName("text_SwordAuraColor3")).Text);
                    slider.Value = 25;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.6, config.MouseEffectSetting.SwordAuraOpacity);
                Assert.AreEqual(2.0, config.MouseEffectSetting.SwordAuraSizeScale);
                Assert.AreEqual("#123456", config.MouseEffectSetting.SwordAuraColor1);
            });
        }

        [TestMethod]
        public void DialogPersistsOnlySwordAuraAndKeepsSelection()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-SwordAura-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.InkFluidOpacity = 0.4;
                    settings.WillowCatkinsOpacity = 0.7;
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)MouseEffectType.SwordAura;
                    var window = new MouseEffectSettingWindow(MouseEffectType.SwordAura, config);
                    Exception? failure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            ((Slider)window.FindName("slider_Opacity")).Value = 25;
                            ((Slider)window.FindName("slider_SwordAuraSize")).Value = 175;
                            ((TextBox)window.FindName("text_SwordAuraColor1")).Text = " #ab1234 ";
                            ((TextBox)window.FindName("text_SwordAuraColor2")).Text = "#12ab34";
                            ((TextBox)window.FindName("text_SwordAuraColor3")).Text = "#1234ab";
                            ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        catch (Exception ex) { failure = ex; }
                        finally { window.Close(); }
                    }, DispatcherPriority.Loaded);
                    Assert.AreEqual(true, window.ShowDialog());
                    if (failure != null) Assert.Fail(failure.ToString());
                    Assert.AreSame(settings, config.MouseEffectSetting);
                    var restored = ConfigHelper.LoadMainConfig().MouseEffectSetting;
                    Assert.AreEqual(0.25, restored.SwordAuraOpacity);
                    Assert.AreEqual(1.75, restored.SwordAuraSizeScale);
                    Assert.AreEqual("#AB1234", restored.SwordAuraColor1);
                    Assert.AreEqual("#12AB34", restored.SwordAuraColor2);
                    Assert.AreEqual("#1234AB", restored.SwordAuraColor3);
                    Assert.AreEqual(MouseEffectSetting.DefaultInkFluidColor1, restored.InkFluidColor1);
                    Assert.AreEqual(0.4, restored.InkFluidOpacity);
                    Assert.AreEqual(0.7, restored.WillowCatkinsOpacity);
                    Assert.AreEqual(28, restored.SelectedEffectType);
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
        public void InkDiffusesAndCurlsWithoutSwordShape()
        {
            RunSta(() =>
            {
                var effect = new SwordAuraCursorEffect();
                var cursor = new Point(180, 160);
                effect.Update(new Point(170, 160), 0.01);
                effect.Update(cursor, 0.03);
                GeometryDrawing[] Ink() => Shapes(effect).Where(s => s.Brush is RadialGradientBrush).ToArray();
                var initial = Ink();
                Assert.AreEqual(2, initial.Length);
                var direction = Transforms(effect).Single();
                foreach (var shape in initial)
                {
                    var brush = (RadialGradientBrush)shape.Brush;
                    Assert.IsTrue(brush.IsFrozen);
                    Assert.AreEqual((byte)0, brush.GradientStops.Last().Color.A);
                    Assert.IsTrue(brush.GradientStops.First().Color.A > brush.GradientStops[2].Color.A);
                }
                effect.Update(cursor, 0.2);
                var expanded = Ink();
                for (var i = 0; i < initial.Length; i++)
                {
                    var before = (EllipseGeometry)initial[i].Geometry;
                    var after = (EllipseGeometry)expanded[i].Geometry;
                    Assert.IsTrue(after.RadiusX > before.RadiusX && after.RadiusY > before.RadiusY);
                    Assert.AreNotEqual(before.Center, after.Center);
                    Assert.AreSame(initial[i].Brush, expanded[i].Brush);
                }
                Assert.AreEqual(direction.M11, Transforms(effect).Single().M11);
                Assert.AreEqual(direction.M22, Transforms(effect).Single().M22);
                Assert.IsTrue(Shapes(effect).All(s => s.Geometry is EllipseGeometry));
                Assert.AreEqual(1, effect.ParticleCount);
                Assert.AreEqual(18, Groups(SwordAuraCursorEffect.PreviewImage.Drawing).SelectMany(g => g.Children.OfType<GeometryDrawing>()).Count(s => s.Brush is RadialGradientBrush));
                effect.Update(cursor, 1);
                Assert.AreEqual(0, Shapes(effect).Length);
            });
        }

        [TestMethod]
        public void AppearanceDefaultsAndColorsAreNormalized()
        {
            var settings = JsonConvert.DeserializeObject<MouseEffectSetting>("{}")!;
            Assert.AreEqual(1.0, settings.SwordAuraSizeScale);
            Assert.AreEqual("#18416E", settings.SwordAuraColor1);
            Assert.AreEqual("#293168", settings.SwordAuraColor2);
            Assert.AreEqual("#187085", settings.SwordAuraColor3);
            settings.SwordAuraColor1 = " #abcdef ";
            Assert.AreEqual("#ABCDEF", settings.SwordAuraColor1);
            settings.SwordAuraColor1 = "bad";
            settings.SwordAuraColor2 = null!;
            settings.SwordAuraColor3 = "#123";
            Assert.AreEqual(MouseEffectSetting.DefaultSwordAuraColor1, settings.SwordAuraColor1);
            Assert.AreEqual(MouseEffectSetting.DefaultSwordAuraColor2, settings.SwordAuraColor2);
            Assert.AreEqual(MouseEffectSetting.DefaultSwordAuraColor3, settings.SwordAuraColor3);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.5)]
        [DataRow(10.0, 3.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void SizeIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { SwordAuraSizeScale = value }.SwordAuraSizeScale);
        }

        [DataTestMethod]
        [DataRow(0.5)]
        [DataRow(1.0)]
        [DataRow(3.0)]
        public void SizeScalesInkAroundUnchangedAnchor(double scale)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new SwordAuraCursorEffect(settings);
                effect.Update(new Point(100, 100), 0.01);
                var cursor = new Point(108, 108);
                effect.Update(cursor, 0.03);
                effect.Update(cursor, 0.1);
                var before = Transforms(effect).Single();
                var shapes = Shapes(effect);
                var opacity = Groups(Drawing(effect)).Select(g => g.Opacity).ToArray();
                settings.SwordAuraSizeScale = scale;
                effect.Update(cursor, double.Epsilon);
                var after = Transforms(effect).Single();
                Assert.AreEqual(before.OffsetX, after.OffsetX);
                Assert.AreEqual(before.OffsetY, after.OffsetY);
                Assert.AreEqual(before.M11 * scale, after.M11, 0.000001);
                Assert.AreEqual(before.M12 * scale, after.M12, 0.000001);
                Assert.AreEqual(before.M21 * scale, after.M21, 0.000001);
                Assert.AreEqual(before.M22 * scale, after.M22, 0.000001);
                Assert.AreEqual(1, effect.ParticleCount);
                var resized = Shapes(effect);
                Assert.AreEqual(2, resized.Length);
                for (var i = 0; i < resized.Length; i++)
                {
                    Assert.AreEqual(shapes[i].Geometry.Bounds, resized[i].Geometry.Bounds);
                    Assert.AreSame(shapes[i].Brush, resized[i].Brush);
                    Assert.IsInstanceOfType(resized[i].Geometry, typeof(EllipseGeometry));
                }
                CollectionAssert.AreEqual(opacity, Groups(Drawing(effect)).Select(g => g.Opacity).ToArray());
                effect.Update(cursor, 1);
                Assert.AreEqual(0, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void ColorsUpdateLiveReuseBrushesAndRestoreDefaults()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new SwordAuraCursorEffect(settings);
                effect.Update(new Point(100, 100), 0.01);
                var cursor = new Point(142, 100);
                effect.Update(cursor, 0.03);
                var defaults = Shapes(effect);
                var matrices = Transforms(effect);
                settings.SwordAuraColor1 = "#FF1234";
                settings.SwordAuraColor2 = "#12FF34";
                settings.SwordAuraColor3 = "#1234FF";
                effect.Update(cursor, double.Epsilon);
                var colored = Shapes(effect);
                var colors = new[] { Color.FromRgb(255, 18, 52), Color.FromRgb(18, 255, 52), Color.FromRgb(18, 52, 255) };
                for (var i = 0; i < colored.Length; i++)
                {
                    var expected = colors[(i / 2 + i % 2) % 3];
                    var brush = (RadialGradientBrush)colored[i].Brush;
                    Assert.AreEqual(Color.FromArgb(180, expected.R, expected.G, expected.B), brush.GradientStops[0].Color);
                    Assert.IsTrue(brush.IsFrozen);
                    Assert.AreNotSame(defaults[i].Brush, brush);
                }
                CollectionAssert.AreEqual(matrices, Transforms(effect));
                effect.Update(cursor, double.Epsilon);
                for (var i = 0; i < colored.Length; i++) Assert.AreSame(colored[i].Brush, Shapes(effect)[i].Brush);
                settings.SwordAuraColor1 = settings.SwordAuraColor2 = settings.SwordAuraColor3 = "#ABCDEF";
                effect.Update(cursor, double.Epsilon);
                Assert.IsTrue(Shapes(effect).All(s => ((RadialGradientBrush)s.Brush).GradientStops[0].Color == Color.FromArgb(180, 171, 205, 239)));
                settings.SwordAuraColor1 = MouseEffectSetting.DefaultSwordAuraColor1;
                settings.SwordAuraColor2 = MouseEffectSetting.DefaultSwordAuraColor2;
                settings.SwordAuraColor3 = MouseEffectSetting.DefaultSwordAuraColor3;
                effect.Update(cursor, double.Epsilon);
                for (var i = 0; i < defaults.Length; i++) Assert.AreSame(defaults[i].Brush, Shapes(effect)[i].Brush);
            });
        }

        [TestMethod]
        public void AppearancePanelDoesNotAffectOtherEffects()
        {
            RunSta(() =>
            {
                var window = new MouseEffectSettingWindow(MouseEffectType.StarlightArrow, new MainConfig());
                try
                {
                    Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName("panel_SwordAura")).Visibility);
                    ((TextBox)window.FindName("text_SwordAuraColor1")).Text = "bad";
                    Assert.IsTrue(((Button)window.FindName("btn_Save")).IsEnabled);
                }
                finally { window.Close(); }
            });
        }

        private static DrawingGroup Drawing(SwordAuraCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
        private static IEnumerable<DrawingGroup> Groups(Drawing drawing)
        {
            if (drawing is DrawingGroup group)
            {
                yield return group;
                foreach (var child in group.Children)
                    foreach (var nested in Groups(child)) yield return nested;
            }
        }
        private static GeometryDrawing[] Shapes(SwordAuraCursorEffect effect) => Groups(Drawing(effect)).SelectMany(g => g.Children.OfType<GeometryDrawing>()).ToArray();
        private static Matrix[] Transforms(SwordAuraCursorEffect effect) => Groups(Drawing(effect))
            .Where(g => g.Transform != null && !g.Transform.Value.IsIdentity && g.Children.OfType<GeometryDrawing>().Any(s => s.Geometry is EllipseGeometry))
            .Select(g => g.Transform.Value).ToArray();
        private static long Alpha(SwordAuraCursorEffect effect)
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
