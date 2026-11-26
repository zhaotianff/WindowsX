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
    public class StarlightArrowEffectTests
    {
        [TestMethod]
        public void DefaultsAndIdentifiersRemainCompatible()
        {
            Assert.AreEqual(26, (int)MouseEffectType.OrbitParticles);
            Assert.AreEqual(27, (int)MouseEffectType.StarlightArrow);
            foreach (var json in new[] { "{}", "{\"MouseEffectSetting\":null}" })
                Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.StarlightArrowOpacity);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { StarlightArrowOpacity = value }.StarlightArrowOpacity);
        }

        [TestMethod]
        public void StarsStartAtTipAndFallWhileStationaryWithFrozenResources()
        {
            RunSta(() =>
            {
                var effect = new StarlightArrowCursorEffect();
                var cursor = new Point(200, 150);
                effect.Update(cursor, 0.01);
                Assert.AreEqual(1, effect.ParticleCount);
                var first = Transforms(effect).Single();
                Assert.AreEqual(cursor.X, first.OffsetX);
                Assert.AreEqual(cursor.Y, first.OffsetY);
                var shapes = Shapes(effect);
                Assert.AreEqual(3, shapes.Length);
                Assert.IsTrue(shapes.All(s => s.Brush.IsFrozen && s.Pen == null));
                var star = shapes.Single(s => s.Geometry is StreamGeometry);
                Assert.IsTrue(star.Geometry.IsFrozen);
                var halo = shapes.Single(s => s.Brush is RadialGradientBrush).Brush;
                effect.Update(cursor, 0.04);
                Assert.AreEqual(1, effect.ParticleCount);
                Assert.IsTrue(Transforms(effect).Single().OffsetY > cursor.Y);
                Assert.AreSame(star.Geometry, Shapes(effect).Single(s => s.Geometry is StreamGeometry).Geometry);
                Assert.AreSame(halo, Shapes(effect).Single(s => s.Brush is RadialGradientBrush).Brush);
                effect.Update(cursor, 0.04);
                Assert.AreEqual(2, effect.ParticleCount);
                var positions = Transforms(effect);
                Assert.IsTrue(positions[0].OffsetY > cursor.Y);
                Assert.AreEqual(cursor.Y, positions[1].OffsetY);
                Assert.IsFalse(effect.IsHitTestVisible);
                Assert.IsTrue(StarlightArrowCursorEffect.PreviewImage.IsFrozen);
                Assert.AreEqual(new Rect(0, 0, 260, 170), StarlightArrowCursorEffect.PreviewImage.Drawing.Bounds);
            });
        }

        [TestMethod]
        public void MovingCursorOnlyEmitsAtCurrentTipWithoutBridgingOrBursts()
        {
            RunSta(() =>
            {
                var effect = new StarlightArrowCursorEffect();
                effect.Update(new Point(100, 100), 0.01);
                effect.Update(new Point(10000, 100), 0.04);
                Assert.AreEqual(1, effect.ParticleCount);
                effect.Update(new Point(12000, 100), 0.04);
                Assert.AreEqual(2, effect.ParticleCount);
                Assert.AreEqual(12000.0, Transforms(effect).Last().OffsetX);
                for (var i = 0; i < 500; i++)
                {
                    var previous = effect.ParticleCount;
                    effect.Update(new Point(i * 100, 100), 0.08);
                    Assert.IsTrue(effect.ParticleCount <= previous + 1);
                    Assert.IsTrue(effect.ParticleCount <= StarlightArrowCursorEffect.MaxParticles);
                    Assert.IsTrue(Shapes(effect).Length <= StarlightArrowCursorEffect.MaxParticles * 3);
                }
                effect.Update(new Point(100, 100), double.MaxValue);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(new Point(100, 100), 0.08);
                Assert.AreEqual(1, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void InvalidInputClearAndRestartStaySafe()
        {
            RunSta(() =>
            {
                var effect = new StarlightArrowCursorEffect();
                foreach (var dt in new[] { 0.0, -1, double.NaN, double.PositiveInfinity }) effect.Update(new Point(100, 100), dt);
                effect.Update(new Point(double.NaN, 100), 0.01);
                effect.Update(new Point(100, double.PositiveInfinity), 0.01);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(new Point(100, 100), 0.01);
                var visual = VisualTreeHelper.GetChild(effect, 0);
                effect.Clear();
                effect.Clear();
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.IsTrue(Drawing(effect) == null || Drawing(effect).Bounds.IsEmpty);
                effect.Update(new Point(250, 150), 0.01);
                Assert.AreEqual(1, effect.ParticleCount);
                Assert.AreEqual(250.0, Transforms(effect).Single().OffsetX);
                Assert.AreSame(visual, VisualTreeHelper.GetChild(effect, 0));
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
            });
        }

        [TestMethod]
        public void OpacityHidesAllLayersLive()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new StarlightArrowCursorEffect(settings);
                effect.Measure(new Size(400, 400));
                effect.Arrange(new Rect(0, 0, 400, 400));
                effect.Update(new Point(200, 200), 0.01);
                Assert.IsTrue(Alpha(effect) > 0);
                settings.StarlightArrowOpacity = 0;
                effect.Update(new Point(200, 200), 0.01);
                Assert.AreEqual(0L, Alpha(effect));
                settings.StarlightArrowOpacity = 0.5;
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
                    window.UpdateMouseEffectType(MouseEffectType.StarlightArrow);
                    var create = typeof(MouseEffectWindow).GetMethod("CreateEffects", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    create.Invoke(window, null);
                    create.Invoke(window, null);
                    var canvas = (Canvas)window.FindName("canvas");
                    Assert.AreEqual(1, canvas.Children.Count);
                    var effect = (StarlightArrowCursorEffect)canvas.Children[0];
                    effect.Update(new Point(100, 100), 0.01);
                    if (switchEffect) window.UpdateMouseEffectType(MouseEffectType.OrbitParticles);
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
                config.MouseEffectSetting.StarlightArrowOpacity = 0.6;
                config.MouseEffectSetting.StarlightArrowColor = "#123456";
                var window = new MouseEffectSettingWindow(MouseEffectType.StarlightArrow, config);
                try
                {
                    Assert.AreEqual("星光箭头设置", window.Title);
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_StarlightArrowColor")).Visibility);
                    var color = (TextBox)window.FindName("text_StarlightArrowColor");
                    Assert.AreEqual("#123456", color.Text);
                    color.Text = "invalid";
                    Assert.IsFalse(((Button)window.FindName("btn_Save")).IsEnabled);
                    ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual("#123456", config.MouseEffectSetting.StarlightArrowColor);
                    Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName("panel_OrbitParticles")).Visibility);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(60.0, slider.Value);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    Assert.AreEqual(MouseEffectSetting.DefaultStarlightArrowColor, color.Text);
                    Assert.IsTrue(((Button)window.FindName("btn_Save")).IsEnabled);
                    slider.Value = 25;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.6, config.MouseEffectSetting.StarlightArrowOpacity);
                Assert.AreEqual("#123456", config.MouseEffectSetting.StarlightArrowColor);
            });
        }

        [TestMethod]
        public void DialogPersistsOnlyStarlightArrowAndKeepsSelection()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-StarlightArrow-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.OrbitParticlesOpacity = 0.4;
                    settings.WillowCatkinsOpacity = 0.7;
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)MouseEffectType.StarlightArrow;
                    var window = new MouseEffectSettingWindow(MouseEffectType.StarlightArrow, config);
                    Exception? failure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            ((Slider)window.FindName("slider_Opacity")).Value = 25;
                            ((TextBox)window.FindName("text_StarlightArrowColor")).Text = " #ab1234 ";
                            ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        catch (Exception ex) { failure = ex; }
                        finally { window.Close(); }
                    }, DispatcherPriority.Loaded);
                    Assert.AreEqual(true, window.ShowDialog());
                    if (failure != null) Assert.Fail(failure.ToString());
                    Assert.AreSame(settings, config.MouseEffectSetting);
                    var restored = ConfigHelper.LoadMainConfig().MouseEffectSetting;
                    Assert.AreEqual(0.25, restored.StarlightArrowOpacity);
                    Assert.AreEqual("#AB1234", restored.StarlightArrowColor);
                    Assert.AreEqual(0.4, restored.OrbitParticlesOpacity);
                    Assert.AreEqual(0.7, restored.WillowCatkinsOpacity);
                    Assert.AreEqual(27, restored.SelectedEffectType);
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
        public void ColorDefaultsAndValidationRemainCompatible()
        {
            var settings = JsonConvert.DeserializeObject<MouseEffectSetting>("{}")!;
            Assert.AreEqual("#FFD570", settings.StarlightArrowColor);
            settings.StarlightArrowColor = " #abcdef ";
            Assert.AreEqual("#ABCDEF", settings.StarlightArrowColor);
            foreach (var invalid in new[] { null, "", "red", "#123", "#GG1234", "#80FFFFFF" })
            {
                settings.StarlightArrowColor = invalid!;
                Assert.AreEqual("#FFD570", settings.StarlightArrowColor);
            }
        }

        [TestMethod]
        public void ColorUpdatesExistingStarsAndHalosWithCachedFrozenBrushes()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new StarlightArrowCursorEffect(settings);
                var cursor = new Point(200, 200);
                effect.Update(cursor, 0.01);
                Brush StarBrush() => Shapes(effect).Single(s => s.Geometry is StreamGeometry).Brush;
                Brush HaloBrush() => Shapes(effect).Single(s => s.Brush is RadialGradientBrush).Brush;
                var originalStar = StarBrush();
                var originalHalo = HaloBrush();
                var geometry = Shapes(effect).Single(s => s.Geometry is StreamGeometry).Geometry;
                var position = Transforms(effect).Single();
                settings.StarlightArrowColor = "#12ABEF";
                effect.Update(cursor, double.Epsilon);
                Assert.AreEqual(position, Transforms(effect).Single());
                Assert.AreEqual(1, effect.ParticleCount);
                var star = (LinearGradientBrush)StarBrush();
                var halo = (RadialGradientBrush)HaloBrush();
                Assert.AreEqual(Color.FromRgb(18, 171, 239), star.GradientStops.Last().Color);
                Assert.AreEqual(Color.FromArgb(55, 18, 171, 239), halo.GradientStops[1].Color);
                Assert.IsTrue(star.IsFrozen && halo.IsFrozen);
                Assert.AreSame(geometry, Shapes(effect).Single(s => s.Geometry is StreamGeometry).Geometry);
                effect.Update(cursor, 0.01);
                Assert.AreSame(star, StarBrush());
                Assert.AreSame(halo, HaloBrush());
                settings.StarlightArrowColor = MouseEffectSetting.DefaultStarlightArrowColor;
                effect.Update(cursor, 0.01);
                Assert.AreSame(originalStar, StarBrush());
                Assert.AreSame(originalHalo, HaloBrush());
            });
        }

        [TestMethod]
        public void ColorPanelIsHiddenForOtherEffects()
        {
            RunSta(() =>
            {
                var window = new MouseEffectSettingWindow(MouseEffectType.OrbitParticles, new MainConfig());
                try
                {
                    Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName("panel_StarlightArrowColor")).Visibility);
                    ((TextBox)window.FindName("text_StarlightArrowColor")).Text = "invalid";
                    Assert.IsTrue(((Button)window.FindName("btn_Save")).IsEnabled);
                }
                finally { window.Close(); }
            });
        }

        private static DrawingGroup Drawing(StarlightArrowCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
        private static IEnumerable<DrawingGroup> Groups(Drawing drawing)
        {
            if (drawing is DrawingGroup group)
            {
                yield return group;
                foreach (var child in group.Children)
                    foreach (var nested in Groups(child)) yield return nested;
            }
        }
        private static GeometryDrawing[] Shapes(StarlightArrowCursorEffect effect) => Groups(Drawing(effect)).SelectMany(g => g.Children.OfType<GeometryDrawing>()).ToArray();
        private static Matrix[] Transforms(StarlightArrowCursorEffect effect) => Groups(Drawing(effect)).Where(g => g.Transform != null && !g.Transform.Value.IsIdentity).Select(g => g.Transform.Value).ToArray();
        private static long Alpha(StarlightArrowCursorEffect effect)
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
