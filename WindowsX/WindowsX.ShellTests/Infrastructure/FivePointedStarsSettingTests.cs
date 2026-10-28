using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WindowsX.Config.Helper;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect;
using WindowsX.Shell.Windows;

namespace WindowsX.ShellTests.Infrastructure
{
    [TestClass]
    [DoNotParallelize]
    public class FivePointedStarsSettingTests
    {
        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{\"FivePointedStarsOpacity\":0.4}}")]
        public void OldConfigurationsKeepSizeAndRandomColors(string json)
        {
            var settings = JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting;
            Assert.AreEqual(1.0, settings.FivePointedStarsSizeScale);
            Assert.AreEqual("#FFD700", settings.FivePointedStarsColor);
            Assert.IsTrue(settings.FivePointedStarsUseRandomColors);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.5)]
        [DataRow(0.5, 0.5)]
        [DataRow(1.75, 1.75)]
        [DataRow(3.0, 3.0)]
        [DataRow(10.0, 3.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void SizeIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { FivePointedStarsSizeScale = value }.FivePointedStarsSizeScale);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("Red")]
        [DataRow("#123")]
        [DataRow("#12345Z")]
        [DataRow("#FF123456")]
        public void InvalidColorsUseDefault(string? value)
        {
            Assert.AreEqual(MouseEffectSetting.DefaultFivePointedStarsColor,
                new MouseEffectSetting { FivePointedStarsColor = value! }.FivePointedStarsColor);
        }

        [TestMethod]
        public void SettingsRoundTripPreservesCustomValues()
        {
            var settings = new MouseEffectSetting
            {
                FivePointedStarsSizeScale = 2.5, FivePointedStarsColor = " #aabbcc ", FivePointedStarsUseRandomColors = false
            };
            var restored = JsonConvert.DeserializeObject<MouseEffectSetting>(JsonConvert.SerializeObject(settings))!;
            Assert.AreEqual(2.5, restored.FivePointedStarsSizeScale);
            Assert.AreEqual("#AABBCC", restored.FivePointedStarsColor);
            Assert.IsFalse(restored.FivePointedStarsUseRandomColors);
        }

        [DataTestMethod]
        [DataRow(0.5)]
        [DataRow(1.0)]
        [DataRow(3.0)]
        public void ScalingKeepsGeometryRotationAndMovingCenter(double scale)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting { FivePointedStarsSizeScale = scale };
                var original = CreateEffect(new MouseEffectSetting());
                var resized = CreateEffect(settings);
                var cursor = new Point(200, 200);
                for (var frame = 0; frame < 6; frame++)
                {
                    if (frame == 3) settings.FivePointedStarsSizeScale = scale == 3 ? 0.5 : 3;
                    original.Update(cursor, 0.05);
                    resized.Update(cursor, 0.05);
                    var before = Stars(original);
                    var after = Stars(resized);
                    Assert.AreEqual(original.ParticleCount, resized.ParticleCount);
                    Assert.AreEqual(original.ParticleCount, before.Length);
                    Assert.AreEqual(before.Length, after.Length);
                    for (var i = 0; i < before.Length; i++)
                    {
                        Assert.AreSame(before[i].Drawing.Geometry, after[i].Drawing.Geometry);
                        Assert.AreEqual(before[i].Transform.OffsetX, after[i].Transform.OffsetX, 0.000001);
                        Assert.AreEqual(before[i].Transform.OffsetY, after[i].Transform.OffsetY, 0.000001);
                        var ratio = settings.FivePointedStarsSizeScale;
                        Assert.AreEqual(before[i].Transform.M11 * ratio, after[i].Transform.M11, 0.000001);
                        Assert.AreEqual(before[i].Transform.M12 * ratio, after[i].Transform.M12, 0.000001);
                        Assert.AreEqual(before[i].Transform.M21 * ratio, after[i].Transform.M21, 0.000001);
                        Assert.AreEqual(before[i].Transform.M22 * ratio, after[i].Transform.M22, 0.000001);
                    }
                }
            });
        }

        [TestMethod]
        public void ColorsSwitchLiveAndCustomBrushIsFrozenAndReused()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting { FivePointedStarsUseRandomColors = false, FivePointedStarsColor = "#123456" };
                var effect = CreateEffect(settings);
                var baseline = CreateEffect(new MouseEffectSetting());
                var cursor = new Point();
                for (var i = 0; i < 10; i++)
                {
                    cursor = new Point(200 + i * 10, 200);
                    effect.Update(cursor, 0.06);
                    baseline.Update(cursor, 0.06);
                }
                var brush = Stars(effect)[0].Drawing.Brush;
                Assert.IsTrue(brush.IsFrozen);
                Assert.IsTrue(Stars(effect).All(s => ((SolidColorBrush)s.Drawing.Brush).Color == Color.FromRgb(18, 52, 86)));
                effect.Update(cursor, 0.05);
                baseline.Update(cursor, 0.05);
                Assert.AreSame(brush, Stars(effect)[0].Drawing.Brush);
                settings.FivePointedStarsColor = "#FF0000";
                effect.Update(cursor, 0.05);
                baseline.Update(cursor, 0.05);
                Assert.IsTrue(Stars(effect).All(s => ((SolidColorBrush)s.Drawing.Brush).Color == Colors.Red));
                settings.FivePointedStarsUseRandomColors = true;
                effect.Update(cursor, 0.05);
                baseline.Update(cursor, 0.05);
                var expected = Stars(baseline).Select(s => ((SolidColorBrush)s.Drawing.Brush).Color).ToArray();
                var actual = Stars(effect).Select(s => ((SolidColorBrush)s.Drawing.Brush).Color).ToArray();
                Assert.IsTrue(expected.Distinct().Count() > 1);
                CollectionAssert.AreEqual(expected, actual);
                settings.FivePointedStarsUseRandomColors = false;
                effect.Update(cursor, 0.05);
                Assert.IsTrue(Stars(effect).All(s => ((SolidColorBrush)s.Drawing.Brush).Color == Colors.Red));
            });
        }

        [TestMethod]
        public void DialogValidatesResetsAndDiscardsDraft()
        {
            RunSta(() =>
            {
                var config = new MainConfig();
                config.MouseEffectSetting.FivePointedStarsSizeScale = 1.75;
                config.MouseEffectSetting.FivePointedStarsColor = "#123456";
                config.MouseEffectSetting.FivePointedStarsUseRandomColors = false;
                var original = JsonConvert.SerializeObject(config);
                var window = new MouseEffectSettingWindow(MouseEffectType.FivePointedStars, config);
                try
                {
                    var size = (Slider)window.FindName("slider_FivePointedStarsSize");
                    var color = (TextBox)window.FindName("text_FivePointedStarsColor");
                    var random = (CheckBox)window.FindName("check_FivePointedStarsRandomColors");
                    var save = (Button)window.FindName("btn_Save");
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_FivePointedStars")).Visibility);
                    Assert.AreEqual(175.0, size.Value);
                    Assert.AreEqual("#123456", color.Text);
                    Assert.AreEqual(false, random.IsChecked);
                    Assert.IsTrue(color.IsEnabled);
                    color.Text = "invalid";
                    Assert.IsFalse(save.IsEnabled);
                    Assert.AreEqual(Visibility.Visible, ((TextBlock)window.FindName("text_FivePointedStarsColorError")).Visibility);
                    save.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(original, JsonConvert.SerializeObject(config));
                    random.IsChecked = true;
                    Assert.IsTrue(save.IsEnabled);
                    Assert.IsFalse(color.IsEnabled);
                    random.IsChecked = false;
                    Assert.IsFalse(save.IsEnabled);
                    color.Text = " #abcdef ";
                    Assert.IsTrue(save.IsEnabled);
                    Assert.AreEqual(Color.FromRgb(171, 205, 239), ((SolidColorBrush)((Border)window.FindName("preview_FivePointedStarsColor")).Background).Color);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, size.Value);
                    Assert.AreEqual(MouseEffectSetting.DefaultFivePointedStarsColor, color.Text);
                    Assert.AreEqual(true, random.IsChecked);
                    Assert.IsFalse(color.IsEnabled);
                    size.Value = 300;
                }
                finally { window.Close(); }
                Assert.AreEqual(original, JsonConvert.SerializeObject(config));
            });
        }

        [DataTestMethod]
        [DataRow(MouseEffectType.FivePointedStars, false)]
        [DataRow(MouseEffectType.FivePointedStars, true)]
        [DataRow(MouseEffectType.Snowflakes, false)]
        public void DialogSavesSelectedEffectAndReloadsSettings(MouseEffectType type, bool randomColors)
        {
            RunSta(() =>
            {
                var originalDirectory = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-StarSettingsTests-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)type;
                    settings.FivePointedStarsSizeScale = 1.5;
                    settings.FivePointedStarsColor = "#123456";
                    settings.FivePointedStarsUseRandomColors = false;
                    settings.SnowflakesSizeScale = 1.75;
                    settings.FivePointedStarsOpacity = 0.7;
                    var window = new MouseEffectSettingWindow(type, config);
                    Exception? failure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            Assert.AreEqual(type == MouseEffectType.FivePointedStars ? Visibility.Visible : Visibility.Collapsed,
                                ((FrameworkElement)window.FindName("panel_FivePointedStars")).Visibility);
                            ((Slider)window.FindName("slider_FivePointedStarsSize")).Value = 250;
                            ((CheckBox)window.FindName("check_FivePointedStarsRandomColors")).IsChecked = randomColors;
                            ((TextBox)window.FindName("text_FivePointedStarsColor")).Text = randomColors ? "invalid" : " #abcdef ";
                            ((Slider)window.FindName("slider_Opacity")).Value = 40;
                            ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        catch (Exception ex) { failure = ex; }
                        finally { window.Close(); }
                    }, DispatcherPriority.Loaded);
                    Assert.AreEqual(true, window.ShowDialog());
                    if (failure != null) Assert.Fail(failure.ToString());
                    Assert.AreSame(settings, config.MouseEffectSetting);
                    var restored = ConfigHelper.LoadMainConfig();
                    Assert.AreEqual(JsonConvert.SerializeObject(settings), JsonConvert.SerializeObject(restored.MouseEffectSetting));
                    var stars = type == MouseEffectType.FivePointedStars;
                    Assert.AreEqual(stars ? 2.5 : 1.5, settings.FivePointedStarsSizeScale);
                    Assert.AreEqual(stars && !randomColors ? "#ABCDEF" : "#123456", settings.FivePointedStarsColor);
                    Assert.AreEqual(stars && randomColors, settings.FivePointedStarsUseRandomColors);
                    Assert.AreEqual(stars ? 0.4 : 0.7, settings.FivePointedStarsOpacity);
                    Assert.AreEqual(1.75, settings.SnowflakesSizeScale);
                    Assert.IsTrue(settings.IsEnabled);
                    Assert.AreEqual((int)type, settings.SelectedEffectType);
                    var reopened = new MouseEffectSettingWindow(MouseEffectType.FivePointedStars, restored);
                    try
                    {
                        Assert.AreEqual(settings.FivePointedStarsSizeScale * 100, ((Slider)reopened.FindName("slider_FivePointedStarsSize")).Value);
                        Assert.AreEqual(settings.FivePointedStarsColor, ((TextBox)reopened.FindName("text_FivePointedStarsColor")).Text);
                        Assert.AreEqual(settings.FivePointedStarsUseRandomColors, ((CheckBox)reopened.FindName("check_FivePointedStarsRandomColors")).IsChecked);
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

        private static ParticleCursorEffect CreateEffect(MouseEffectSetting settings)
        {
            var effect = new ParticleCursorEffect(MouseEffectType.FivePointedStars, settings);
            typeof(ParticleCursorEffect).GetField("random", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(effect, new Random(12345));
            return effect;
        }

        private static (GeometryDrawing Drawing, Matrix Transform)[] Stars(ParticleCursorEffect effect) =>
            Shapes(((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing, Matrix.Identity).ToArray();

        private static IEnumerable<(GeometryDrawing Drawing, Matrix Transform)> Shapes(Drawing drawing, Matrix parent)
        {
            if (drawing is DrawingGroup group)
            {
                var transform = group.Transform?.Value ?? Matrix.Identity;
                transform.Append(parent);
                foreach (var child in group.Children)
                    foreach (var shape in Shapes(child, transform)) yield return shape;
            }
            else if (drawing is GeometryDrawing geometry) yield return (geometry, parent);
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
