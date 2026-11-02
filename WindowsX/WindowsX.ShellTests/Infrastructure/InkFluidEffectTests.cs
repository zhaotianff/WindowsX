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
    public class InkFluidEffectTests
    {
        [TestMethod]
        public void InkFluidIsAppendedWithoutChangingExistingTypes()
        {
            Assert.AreEqual(7, (int)MouseEffectType.FivePointedStars);
            Assert.AreEqual(8, (int)MouseEffectType.InkFluid);
        }

        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{\"SnowflakesOpacity\":0.3}}")]
        public void OldConfigurationUsesDefaultOpacity(string json)
        {
            var settings = JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting;
            Assert.AreEqual(1.0, settings.InkFluidOpacity);
            Assert.AreEqual("#18416E", settings.InkFluidColor1);
            Assert.AreEqual("#293168", settings.InkFluidColor2);
            Assert.AreEqual("#187085", settings.InkFluidColor3);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("Red")]
        [DataRow("#123")]
        [DataRow("#GG0080")]
        [DataRow("#8018416E")]
        public void InvalidColorsUseOriginalPalette(string? color)
        {
            var settings = new MouseEffectSetting { InkFluidColor1 = color, InkFluidColor2 = color, InkFluidColor3 = color };
            Assert.AreEqual(MouseEffectSetting.DefaultInkFluidColor1, settings.InkFluidColor1);
            Assert.AreEqual(MouseEffectSetting.DefaultInkFluidColor2, settings.InkFluidColor2);
            Assert.AreEqual(MouseEffectSetting.DefaultInkFluidColor3, settings.InkFluidColor3);
        }

        [TestMethod]
        public void ColorsAreNormalizedAndPersisted()
        {
            var settings = new MouseEffectSetting
            {
                InkFluidColor1 = " #aabbcc ", InkFluidColor2 = "#000000", InkFluidColor3 = "#ffffff"
            };
            var restored = JsonConvert.DeserializeObject<MouseEffectSetting>(JsonConvert.SerializeObject(settings))!;
            Assert.AreEqual("#AABBCC", restored.InkFluidColor1);
            Assert.AreEqual("#000000", restored.InkFluidColor2);
            Assert.AreEqual("#FFFFFF", restored.InkFluidColor3);
        }

        [TestMethod]
        public void RunningInkUpdatesOnlyChangedColorBrushes()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new InkFluidCursorEffect(settings);
                var cursor = new Point(200, 200);
                effect.Update(cursor, 0.016);
                var field = typeof(InkFluidCursorEffect).GetField("inkBrushes", BindingFlags.Instance | BindingFlags.NonPublic)!;
                var brushes = (Brush[])field.GetValue(effect)!;
                var original = brushes.ToArray();
                effect.Update(cursor, 0.016);
                for (var i = 0; i < 3; i++) Assert.AreSame(original[i], brushes[i]);
                settings.InkFluidColor2 = "#FF0000";
                effect.Update(cursor, 0.016);
                Assert.AreSame(original[0], brushes[0]);
                Assert.AreNotSame(original[1], brushes[1]);
                Assert.AreSame(original[2], brushes[2]);
                settings.InkFluidColor1 = "#FF0000";
                settings.InkFluidColor3 = "#FF0000";
                effect.Update(cursor, 0.016);
                Assert.AreEqual(1, effect.ParticleCount);
                foreach (var drawing in GetGeometryDrawings(GetDrawing(effect)))
                {
                    var brush = (RadialGradientBrush)drawing.Brush;
                    Assert.IsTrue(brush.IsFrozen);
                    CollectionAssert.AreEqual(new byte[] { 200, 120, 35, 0 }, brush.GradientStops.Select(s => s.Color.A).ToArray());
                    foreach (var stop in brush.GradientStops)
                    {
                        Assert.AreEqual((byte)255, stop.Color.R);
                        Assert.AreEqual((byte)0, stop.Color.G);
                        Assert.AreEqual((byte)0, stop.Color.B);
                    }
                }
                Assert.AreEqual(MouseEffectSetting.DefaultFollowingColor, settings.FollowingColor);
                Assert.AreEqual(MouseEffectSetting.DefaultTrailColor, settings.TrailColor);
            });
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        public void DialogValidatesEachColorAndPreviewsWithoutSaving(int index)
        {
            RunSta(() =>
            {
                var config = new MainConfig();
                var window = new MouseEffectSettingWindow(MouseEffectType.InkFluid, config);
                try
                {
                    var input = (TextBox)window.FindName("text_InkFluidColor" + index);
                    var preview = (Border)window.FindName("preview_InkFluidColor" + index);
                    var save = (Button)window.FindName("btn_Save");
                    var error = (TextBlock)window.FindName("text_InkFluidColorError");
                    input.Text = "#invalid";
                    Assert.IsFalse(save.IsEnabled);
                    Assert.AreEqual(Visibility.Visible, error.Visibility);
                    Assert.IsNull(preview.Background);
                    input.Text = " #ff4545 ";
                    Assert.IsTrue(save.IsEnabled);
                    Assert.AreEqual(Visibility.Collapsed, error.Visibility);
                    Assert.AreEqual(Color.FromRgb(255, 69, 69), ((SolidColorBrush)preview.Background).Color);
                }
                finally { window.Close(); }
                Assert.AreEqual(MouseEffectSetting.DefaultInkFluidColor1, config.MouseEffectSetting.InkFluidColor1);
                Assert.AreEqual(MouseEffectSetting.DefaultInkFluidColor2, config.MouseEffectSetting.InkFluidColor2);
                Assert.AreEqual(MouseEffectSetting.DefaultInkFluidColor3, config.MouseEffectSetting.InkFluidColor3);
            });
        }

        [DataTestMethod]
        [DataRow(MouseEffectType.FollowingDot)]
        [DataRow(MouseEffectType.CursorTrail)]
        [DataRow(MouseEffectType.Fireworks)]
        [DataRow(MouseEffectType.Snowflakes)]
        public void OtherEffectsHideInkColorSettings(MouseEffectType type)
        {
            RunSta(() =>
            {
                var window = new MouseEffectSettingWindow(type, new MainConfig());
                try { Assert.AreEqual(Visibility.Collapsed, ((StackPanel)window.FindName("panel_InkFluid")).Visibility); }
                finally { window.Close(); }
            });
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { InkFluidOpacity = value }.InkFluidOpacity);
        }

        [TestMethod]
        public void InkDiffusesMovesAndUsesSoftTransparentEdges()
        {
            RunSta(() =>
            {
                var effect = new InkFluidCursorEffect();
                effect.Update(new Point(200, 200), 0.016);
                Assert.AreEqual(1, effect.ParticleCount);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                Assert.IsFalse(effect.IsHitTestVisible);
                var initial = GetGeometryDrawings(GetDrawing(effect)).ToArray();
                Assert.AreEqual(2, initial.Length);
                var initialEllipse = (EllipseGeometry)initial[0].Geometry;
                foreach (var drawing in initial)
                {
                    var brush = (RadialGradientBrush)drawing.Brush;
                    Assert.IsTrue(brush.IsFrozen);
                    Assert.IsTrue(brush.GradientStops[0].Color.A > 0);
                    Assert.AreEqual((byte)0, brush.GradientStops.Last().Color.A);
                }
                effect.Update(new Point(200, 200), 0.1);
                var expanded = (EllipseGeometry)GetGeometryDrawings(GetDrawing(effect)).First().Geometry;
                Assert.IsTrue(expanded.RadiusX > initialEllipse.RadiusX);
                Assert.IsTrue(expanded.RadiusY > initialEllipse.RadiusY);
                Assert.AreNotEqual(initialEllipse.Center, expanded.Center);
                Assert.AreEqual(1, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void MouseMovementIsInterpolatedIntoContinuousInk()
        {
            RunSta(() =>
            {
                var effect = new InkFluidCursorEffect();
                effect.Measure(new Size(640, 480));
                effect.Arrange(new Rect(0, 0, 640, 480));
                effect.Update(new Point(100, 200), 0.016);
                effect.Update(new Point(280, 200), 0.016);
                Assert.AreEqual(31, effect.ParticleCount);
                var pixels = RenderPixels(effect);
                for (var x = 100; x <= 280; x++)
                    Assert.IsTrue(pixels[(200 * 640 + x) * 4 + 3] > 0, "Ink must remain connected at x=" + x);
                Assert.AreEqual((byte)0, pixels[3]);
            });
        }

        [TestMethod]
        public void EmissionAndParticleCountsRemainBounded()
        {
            RunSta(() =>
            {
                var effect = new InkFluidCursorEffect();
                effect.Update(new Point(100, 100), 0.016);
                effect.Update(new Point(10000, 10000), 0.016);
                Assert.AreEqual(1 + InkFluidCursorEffect.MaxEmissionPerUpdate, effect.ParticleCount);
                for (var i = 0; i < 240; i++)
                {
                    effect.Update(new Point(i * 20, 200), 0.016);
                    Assert.IsTrue(effect.ParticleCount <= InkFluidCursorEffect.MaxParticles);
                    Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                }
            });
        }

        [TestMethod]
        public void StationaryInkExpiresAndCanRestartAfterClearing()
        {
            RunSta(() =>
            {
                var effect = new InkFluidCursorEffect();
                var cursor = new Point(200, 200);
                effect.Update(cursor, 0.016);
                effect.Update(cursor, 5);
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.IsFalse(GetGeometryDrawings(GetDrawing(effect)).Any());
                effect.Update(cursor, 0.016);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(new Point(300, 200), 0.016);
                Assert.AreEqual(1, effect.ParticleCount);
                effect.Clear();
                effect.Clear();
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(cursor, 0.016);
                Assert.AreEqual(1, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void InvalidInputDoesNotEmitInk()
        {
            RunSta(() =>
            {
                var effect = new InkFluidCursorEffect();
                foreach (var elapsed in new[] { 0.0, -1.0, double.NaN, double.PositiveInfinity })
                    effect.Update(new Point(100, 100), elapsed);
                effect.Update(new Point(double.NaN, 100), 0.016);
                effect.Update(new Point(100, double.PositiveInfinity), 0.016);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(new Point(100, 100), 0.016);
                Assert.AreEqual(1, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void RunningInkRespectsIndependentOpacity()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new InkFluidCursorEffect(settings);
                effect.Measure(new Size(640, 480));
                effect.Arrange(new Rect(0, 0, 640, 480));
                var cursor = new Point(320, 240);
                effect.Update(cursor, 0.016);
                var original = TotalAlpha(RenderPixels(effect));
                Assert.IsTrue(original > 0);
                settings.InkFluidOpacity = 0;
                effect.Update(cursor, 0.000001);
                Assert.AreEqual(0L, TotalAlpha(RenderPixels(effect)));
                settings.InkFluidOpacity = 0.25;
                effect.Update(cursor, 0.000001);
                var reduced = TotalAlpha(RenderPixels(effect));
                Assert.IsTrue(reduced > 0 && reduced < original);
                settings.InkFluidOpacity = 1;
                effect.Update(cursor, 0.000001);
                Assert.IsTrue(TotalAlpha(RenderPixels(effect)) > reduced);
                Assert.AreEqual(1.0, settings.SnowflakesOpacity);
                Assert.AreEqual(1.0, settings.FireworksOpacity);
            });
        }

        [DataTestMethod]
        [DataRow("start")]
        [DataRow("switch")]
        [DataRow("stop")]
        [DataRow("close")]
        public void WindowLifecycleClearsInk(string action)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                var effect = new InkFluidCursorEffect();
                try
                {
                    window.UpdateMouseEffectType(MouseEffectType.InkFluid);
                    effect.Update(new Point(200, 200), 0.016);
                    var field = typeof(MouseEffectWindow).GetField("inkFluidEffect", BindingFlags.Instance | BindingFlags.NonPublic)!;
                    field.SetValue(window, effect);
                    var canvas = (Canvas)window.FindName("canvas");
                    canvas.Children.Add(effect);
                    if (action == "start") window.StartMouseEffect();
                    else if (action == "switch") window.UpdateMouseEffectType(MouseEffectType.Snowflakes);
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
        public void DialogLoadsResetsAndDiscardsChanges()
        {
            RunSta(() =>
            {
                var config = new MainConfig();
                config.MouseEffectSetting.InkFluidOpacity = 0.65;
                config.MouseEffectSetting.InkFluidColor1 = "#FF0000";
                config.MouseEffectSetting.InkFluidColor2 = "#00FF00";
                config.MouseEffectSetting.InkFluidColor3 = "#0000FF";
                var window = new MouseEffectSettingWindow(MouseEffectType.InkFluid, config);
                try
                {
                    Assert.AreEqual("墨水流体设置", window.Title);
                    Assert.AreEqual(Visibility.Visible, ((StackPanel)window.FindName("panel_InkFluid")).Visibility);
                    Assert.AreEqual("#FF0000", ((TextBox)window.FindName("text_InkFluidColor1")).Text);
                    Assert.AreEqual("#00FF00", ((TextBox)window.FindName("text_InkFluidColor2")).Text);
                    Assert.AreEqual("#0000FF", ((TextBox)window.FindName("text_InkFluidColor3")).Text);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(65.0, slider.Value, 0.001);
                    Assert.AreEqual(Visibility.Visible, ((StackPanel)window.FindName("panel_ParticleOpacity")).Visibility);
                    Assert.AreEqual(Visibility.Collapsed, ((StackPanel)window.FindName("panel_Expansion")).Visibility);
                    Assert.AreEqual(Visibility.Collapsed, ((TextBlock)window.FindName("text_SharedSettings")).Visibility);
                    slider.Value = 25;
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    Assert.AreEqual(MouseEffectSetting.DefaultInkFluidColor1, ((TextBox)window.FindName("text_InkFluidColor1")).Text);
                    Assert.AreEqual(MouseEffectSetting.DefaultInkFluidColor2, ((TextBox)window.FindName("text_InkFluidColor2")).Text);
                    Assert.AreEqual(MouseEffectSetting.DefaultInkFluidColor3, ((TextBox)window.FindName("text_InkFluidColor3")).Text);
                    slider.Value = 40;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.65, config.MouseEffectSetting.InkFluidOpacity);
                Assert.AreEqual("#FF0000", config.MouseEffectSetting.InkFluidColor1);
                Assert.AreEqual("#00FF00", config.MouseEffectSetting.InkFluidColor2);
                Assert.AreEqual("#0000FF", config.MouseEffectSetting.InkFluidColor3);
            });
        }

        [TestMethod]
        public void DialogPersistsOnlyInkSettings()
        {
            RunSta(() =>
            {
                var originalDirectory = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-InkTests-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    config.MouseEffectSetting.SnowflakesOpacity = 0.7;
                    config.MouseEffectSetting.FivePointedStarsOpacity = 0.4;
                    config.MouseEffectSetting.WillowCatkinsOpacity = 0.6;
                    var window = new MouseEffectSettingWindow(MouseEffectType.InkFluid, config);
                    Exception? callbackFailure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            ((Slider)window.FindName("slider_Opacity")).Value = 25;
                            ((TextBox)window.FindName("text_InkFluidColor1")).Text = " #ff4545 ";
                            ((TextBox)window.FindName("text_InkFluidColor2")).Text = "#32cd32";
                            ((TextBox)window.FindName("text_InkFluidColor3")).Text = "#ffd700";
                            ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        catch (Exception ex) { callbackFailure = ex; }
                        finally { window.Close(); }
                    }, DispatcherPriority.Loaded);
                    var result = window.ShowDialog();
                    if (callbackFailure != null) Assert.Fail(callbackFailure.ToString());
                    Assert.AreEqual(true, result);
                    Assert.AreEqual("#FF4545", config.MouseEffectSetting.InkFluidColor1);
                    Assert.AreEqual("#32CD32", config.MouseEffectSetting.InkFluidColor2);
                    Assert.AreEqual("#FFD700", config.MouseEffectSetting.InkFluidColor3);
                    var restored = ConfigHelper.LoadMainConfig();
                    Assert.AreEqual("#FF4545", restored.MouseEffectSetting.InkFluidColor1);
                    Assert.AreEqual("#32CD32", restored.MouseEffectSetting.InkFluidColor2);
                    Assert.AreEqual("#FFD700", restored.MouseEffectSetting.InkFluidColor3);
                    Assert.AreEqual(MouseEffectSetting.DefaultFollowingColor, restored.MouseEffectSetting.FollowingColor);
                    Assert.AreEqual(MouseEffectSetting.DefaultTrailColor, restored.MouseEffectSetting.TrailColor);
                    Assert.AreEqual(0.25, restored.MouseEffectSetting.InkFluidOpacity);
                    Assert.AreEqual(0.7, restored.MouseEffectSetting.SnowflakesOpacity);
                    Assert.AreEqual(0.4, restored.MouseEffectSetting.FivePointedStarsOpacity);
                    Assert.AreEqual(0.6, restored.MouseEffectSetting.WillowCatkinsOpacity);
                    var reopened = new MouseEffectSettingWindow(MouseEffectType.InkFluid, restored);
                    try
                    {
                        Assert.AreEqual(25.0, ((Slider)reopened.FindName("slider_Opacity")).Value);
                        Assert.AreEqual("#FF4545", ((TextBox)reopened.FindName("text_InkFluidColor1")).Text);
                        Assert.AreEqual("#32CD32", ((TextBox)reopened.FindName("text_InkFluidColor2")).Text);
                        Assert.AreEqual("#FFD700", ((TextBox)reopened.FindName("text_InkFluidColor3")).Text);
                    }
                    finally { reopened.Close(); }
                }
                finally
                {
                    Environment.CurrentDirectory = originalDirectory;
                    System.IO.Directory.Delete(directory, true);
                }
            });
        }

        private static DrawingGroup GetDrawing(InkFluidCursorEffect effect)
        {
            return ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
        }

        private static IEnumerable<GeometryDrawing> GetGeometryDrawings(Drawing drawing)
        {
            if (drawing is GeometryDrawing geometry) yield return geometry;
            else if (drawing is DrawingGroup group)
            {
                foreach (var child in group.Children)
                    foreach (var result in GetGeometryDrawings(child))
                        yield return result;
            }
        }

        private static byte[] RenderPixels(InkFluidCursorEffect effect)
        {
            var bitmap = new RenderTargetBitmap(640, 480, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(effect);
            var pixels = new byte[640 * 480 * 4];
            bitmap.CopyPixels(pixels, 640 * 4, 0);
            return pixels;
        }

        private static long TotalAlpha(byte[] pixels)
        {
            long alpha = 0;
            for (var i = 3; i < pixels.Length; i += 4) alpha += pixels[i];
            return alpha;
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
