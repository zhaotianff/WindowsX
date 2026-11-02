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
    public class SnowflakeSizeSettingTests
    {
        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{\"SnowflakesOpacity\":0.4}}")]
        public void OldConfigurationsKeepOriginalSize(string json)
        {
            Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.SnowflakesSizeScale);
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
        public void SizeIsNormalized(double input, double expected)
        {
            var settings = new MouseEffectSetting { SnowflakesSizeScale = input };
            Assert.AreEqual(expected, settings.SnowflakesSizeScale);
            var restored = JsonConvert.DeserializeObject<MouseEffectSetting>(JsonConvert.SerializeObject(settings))!;
            Assert.AreEqual(expected, restored.SnowflakesSizeScale);
        }

        [DataTestMethod]
        [DataRow(0.5)]
        [DataRow(1.0)]
        [DataRow(3.0)]
        public void SizeScalesEveryBranchAndPenAroundEachMovingSnowflake(double scale)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting { SnowflakesSizeScale = scale };
                var original = CreateEffect(new MouseEffectSetting());
                var resized = CreateEffect(settings);
                var cursor = new Point(200, 200);
                original.Update(cursor, 0.016);
                resized.Update(cursor, 0.016);
                for (var i = 0; i < 5; i++)
                {
                    original.Update(cursor, 0.05);
                    resized.Update(cursor, 0.05);
                    AssertScaled(original, resized, settings.SnowflakesSizeScale);
                }
                settings.SnowflakesSizeScale = scale == 3 ? 0.5 : 3;
                original.Update(cursor, 0.05);
                resized.Update(cursor, 0.05);
                AssertScaled(original, resized, settings.SnowflakesSizeScale);
            });
        }

        [TestMethod]
        public void DialogLoadsResetsAndDiscardsSizeDraft()
        {
            RunSta(() =>
            {
                var config = new MainConfig();
                config.MouseEffectSetting.SnowflakesSizeScale = 1.75;
                config.MouseEffectSetting.SnowflakesOpacity = 0.6;
                var original = JsonConvert.SerializeObject(config);
                var window = new MouseEffectSettingWindow(MouseEffectType.Snowflakes, config);
                try
                {
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_SnowflakesSize")).Visibility);
                    var slider = (Slider)window.FindName("slider_SnowflakesSize");
                    Assert.AreEqual(175.0, slider.Value);
                    Assert.AreEqual(50.0, slider.Minimum);
                    Assert.AreEqual(300.0, slider.Maximum);
                    slider.Value = 300;
                    Assert.AreEqual(original, JsonConvert.SerializeObject(config));
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    Assert.AreEqual(100.0, ((Slider)window.FindName("slider_Opacity")).Value);
                    slider.Value = 50;
                }
                finally { window.Close(); }
                Assert.AreEqual(original, JsonConvert.SerializeObject(config));
            });
        }

        [DataTestMethod]
        [DataRow(MouseEffectType.Snowflakes)]
        [DataRow(MouseEffectType.FivePointedStars)]
        public void DialogPersistsOnlySelectedEffectAndReloadsSize(MouseEffectType type)
        {
            RunSta(() =>
            {
                var originalDirectory = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-SnowflakeSizeTests-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)type;
                    settings.SnowflakesSizeScale = 1.5;
                    settings.SnowflakesOpacity = 0.6;
                    settings.FivePointedStarsOpacity = 0.7;
                    settings.WillowCatkinsSizeScale = 2;
                    settings.StardustSizeScale = 1.75;
                    var window = new MouseEffectSettingWindow(type, config);
                    Exception? failure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            Assert.AreEqual(type == MouseEffectType.Snowflakes ? Visibility.Visible : Visibility.Collapsed,
                                ((FrameworkElement)window.FindName("panel_SnowflakesSize")).Visibility);
                            ((Slider)window.FindName("slider_SnowflakesSize")).Value = 250;
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
                    Assert.AreEqual(type == MouseEffectType.Snowflakes ? 2.5 : 1.5, settings.SnowflakesSizeScale);
                    Assert.AreEqual(type == MouseEffectType.Snowflakes ? 0.4 : 0.6, settings.SnowflakesOpacity);
                    Assert.AreEqual(type == MouseEffectType.FivePointedStars ? 0.4 : 0.7, settings.FivePointedStarsOpacity);
                    Assert.AreEqual(2.0, settings.WillowCatkinsSizeScale);
                    Assert.AreEqual(1.75, settings.StardustSizeScale);
                    Assert.IsTrue(settings.IsEnabled);
                    Assert.AreEqual((int)type, settings.SelectedEffectType);
                    var reopened = new MouseEffectSettingWindow(MouseEffectType.Snowflakes, restored);
                    try { Assert.AreEqual(settings.SnowflakesSizeScale * 100, ((Slider)reopened.FindName("slider_SnowflakesSize")).Value); }
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
            var effect = new ParticleCursorEffect(MouseEffectType.Snowflakes, settings);
            typeof(ParticleCursorEffect).GetField("random", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(effect, new Random(12345));
            return effect;
        }

        private static void AssertScaled(ParticleCursorEffect original, ParticleCursorEffect resized, double scale)
        {
            var before = Lines(((DrawingVisual)VisualTreeHelper.GetChild(original, 0)).Drawing, Matrix.Identity).ToArray();
            var after = Lines(((DrawingVisual)VisualTreeHelper.GetChild(resized, 0)).Drawing, Matrix.Identity).ToArray();
            Assert.AreEqual(original.ParticleCount, resized.ParticleCount);
            Assert.AreEqual(original.ParticleCount * 18, before.Length);
            Assert.AreEqual(before.Length, after.Length);
            for (var i = 0; i < before.Length; i++)
            {
                var center = before[i / 18 * 18].From;
                var expectedFrom = center + (before[i].From - center) * scale;
                var expectedTo = center + (before[i].To - center) * scale;
                Assert.AreEqual(0, (expectedFrom - after[i].From).Length, 0.000001);
                Assert.AreEqual(0, (expectedTo - after[i].To).Length, 0.000001);
                Assert.AreEqual(before[i].Thickness * scale, after[i].Thickness, 0.000001);
            }
        }

        private static IEnumerable<(Point From, Point To, double Thickness)> Lines(Drawing drawing, Matrix parent)
        {
            if (drawing is DrawingGroup group)
            {
                var transform = group.Transform?.Value ?? Matrix.Identity;
                transform.Append(parent);
                foreach (var child in group.Children)
                    foreach (var line in Lines(child, transform)) yield return line;
            }
            else if (drawing is GeometryDrawing geometry && geometry.Geometry is LineGeometry line)
            {
                var transform = line.Transform?.Value ?? Matrix.Identity;
                transform.Append(parent);
                yield return (transform.Transform(line.StartPoint), transform.Transform(line.EndPoint),
                    transform.Transform(new Vector(geometry.Pen.Thickness, 0)).Length);
            }
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
