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
    public class CloudEffectTests
    {
        [TestMethod]
        public void DefaultsKeepOldConfigurationCompatible()
        {
            Assert.AreEqual(22, (int)MouseEffectType.Butterflies);
            Assert.AreEqual(23, (int)MouseEffectType.Cloud);
            foreach (var json in new[] { "{}", "{\"MouseEffectSetting\":null}" })
                Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.CloudOpacity);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { CloudOpacity = value }.CloudOpacity);
        }

        [TestMethod]
        public void OneCloudLagsBouncesAndSettles()
        {
            RunSta(() =>
            {
                var effect = new CloudCursorEffect();
                effect.Update(new Point(100, 100), 0.01);
                Assert.AreEqual(new Point(130, 126), (Point)Field(effect, "position"));
                effect.Update(new Point(300, 100), 0.1);
                var position = (Point)Field(effect, "position");
                Assert.IsTrue(position.X > 130 && position.X < 330);
                var cloud = Groups(Drawing(effect)).First(g => g.Transform != null && !g.Transform.Value.IsIdentity);
                Assert.AreEqual(1.0, cloud.Transform.Value.Determinant, 0.000001);
                Assert.IsTrue(cloud.Transform.Value.M11 > 1);
                var overshot = false;
                for (var i = 0; i < 300; i++)
                {
                    effect.Update(new Point(300, 100), 1.0 / 60);
                    overshot |= ((Point)Field(effect, "position")).X > 330;
                }
                Assert.IsTrue(overshot);
                Assert.AreEqual(330, ((Point)Field(effect, "position")).X, 0.001);
                Assert.AreEqual(126, ((Point)Field(effect, "position")).Y, 0.001);
                Assert.AreEqual(1, effect.ParticleCount);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                Assert.IsFalse(effect.IsHitTestVisible);
            });
        }

        [TestMethod]
        public void SpringPositionIsIndependentOfFrameInterval()
        {
            RunSta(() =>
            {
                var a = new CloudCursorEffect();
                var b = new CloudCursorEffect();
                a.Update(new Point(100, 100), 0.01);
                b.Update(new Point(100, 100), 0.01);
                a.Update(new Point(300, 200), 0.2);
                for (var i = 0; i < 20; i++) b.Update(new Point(300, 200), 0.01);
                Assert.IsTrue((((Point)Field(a, "position")) - ((Point)Field(b, "position"))).Length < 0.000001);
                Assert.IsTrue((((Vector)Field(a, "velocity")) - ((Vector)Field(b, "velocity"))).Length < 0.000001);
            });
        }

        [TestMethod]
        public void StarsDropOccasionallyFallFadeAndStayBounded()
        {
            RunSta(() =>
            {
                var effect = new CloudCursorEffect();
                var cursor = new Point(200, 200);
                for (var i = 0; i < 8; i++) effect.Update(cursor, 0.1);
                Assert.AreEqual(0, effect.StarCount);
                for (var i = 0; i < 3; i++) effect.Update(cursor, 0.1);
                Assert.AreEqual(1, effect.StarCount);
                var particle = ((System.Collections.IList)Field(effect, "stars"))[0]!;
                var ageField = particle.GetType().GetField("Age")!;
                var draw = typeof(CloudCursorEffect).GetMethod("DrawStar", BindingFlags.NonPublic | BindingFlags.Static)!;
                DrawingGroup Render(double age)
                {
                    ageField.SetValue(particle, age);
                    var group = new DrawingGroup();
                    using (var context = group.Open()) draw.Invoke(null, new object[] { context, particle });
                    return group;
                }
                var early = Render(0.1);
                var late = Render(1.0);
                var earlyY = Groups(early).First(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value.OffsetY;
                var lateY = Groups(late).First(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value.OffsetY;
                Assert.IsTrue(lateY > earlyY);
                Assert.IsTrue(Groups(late).Min(g => g.Opacity) < Groups(early).Min(g => g.Opacity));
                for (var i = 0; i < 2000; i++) effect.Update(cursor, 0.016);
                Assert.IsTrue(effect.StarCount <= CloudCursorEffect.MaxStars);
                effect.Update(cursor, 10);
                Assert.AreEqual(0, effect.StarCount);
                Assert.AreEqual(1, effect.ParticleCount);
            });
        }

        [TestMethod]
        public void InvalidInputClearAndPreviewAreSafe()
        {
            RunSta(() =>
            {
                var effect = new CloudCursorEffect();
                foreach (var elapsed in new[] { 0.0, -1, double.NaN, double.PositiveInfinity }) effect.Update(new Point(100, 100), elapsed);
                effect.Update(new Point(double.NaN, 100), 0.01);
                effect.Update(new Point(100, double.PositiveInfinity), 0.01);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(new Point(100, 100), 0.01);
                effect.Update(new Point(200, 200), double.MaxValue);
                Assert.IsTrue(double.IsFinite(((Point)Field(effect, "position")).X));
                var visual = VisualTreeHelper.GetChild(effect, 0);
                effect.Clear();
                effect.Clear();
                Assert.AreEqual(0, effect.ParticleCount);
                Assert.AreEqual(0, effect.StarCount);
                Assert.IsTrue(Drawing(effect) == null || Drawing(effect).Bounds.IsEmpty);
                effect.Update(new Point(100, 100), 0.01);
                Assert.AreSame(visual, VisualTreeHelper.GetChild(effect, 0));
                Assert.AreEqual(1, effect.ParticleCount);
                Assert.IsTrue(CloudCursorEffect.PreviewImage.IsFrozen);
                Assert.AreEqual(new Rect(0, 0, 260, 170), CloudCursorEffect.PreviewImage.Drawing.Bounds);
            });
        }

        [TestMethod]
        public void OpacityAffectsCloudAndStarsLive()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new CloudCursorEffect(settings);
                effect.Measure(new Size(400, 400));
                effect.Arrange(new Rect(0, 0, 400, 400));
                for (var i = 0; i < 11; i++) effect.Update(new Point(200, 200), 0.1);
                Assert.IsTrue(effect.StarCount > 0);
                Assert.IsTrue(Alpha(effect) > 0);
                settings.CloudOpacity = 0;
                effect.Update(new Point(200, 200), 0.01);
                Assert.AreEqual(0L, Alpha(effect));
                settings.CloudOpacity = 0.5;
                effect.Update(new Point(200, 200), 0.01);
                Assert.IsTrue(Alpha(effect) > 0);
            });
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void WindowUsesSharedEffectAndClearsIt(bool switchEffect)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                try
                {
                    window.StartMouseEffect();
                    window.UpdateMouseEffectType(MouseEffectType.Cloud);
                    var create = typeof(MouseEffectWindow).GetMethod("CreateEffects", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    create.Invoke(window, null);
                    create.Invoke(window, null);
                    var canvas = (Canvas)window.FindName("canvas");
                    Assert.AreEqual(1, canvas.Children.Count);
                    var effect = (CloudCursorEffect)canvas.Children[0];
                    effect.Update(new Point(100, 100), 0.01);
                    if (switchEffect) window.UpdateMouseEffectType(MouseEffectType.Butterflies);
                    else window.StopMouseEffect();
                    Assert.AreEqual(0, effect.ParticleCount);
                    Assert.AreEqual(0, effect.StarCount);
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
                config.MouseEffectSetting.CloudOpacity = 0.6;
                config.MouseEffectSetting.CloudSizeScale = 2;
                config.MouseEffectSetting.CloudPosition = "TopLeft";
                config.MouseEffectSetting.CloudHorizontalOffset = 80;
                config.MouseEffectSetting.CloudVerticalOffset = 60;
                var window = new MouseEffectSettingWindow(MouseEffectType.Cloud, config);
                try
                {
                    Assert.AreEqual("云朵设置", window.Title);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(60.0, slider.Value);
                    var size = (Slider)window.FindName("slider_CloudSize");
                    var direction = (ComboBox)window.FindName("combo_CloudPosition");
                    var horizontal = (Slider)window.FindName("slider_CloudHorizontalOffset");
                    var vertical = (Slider)window.FindName("slider_CloudVerticalOffset");
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_Cloud")).Visibility);
                    Assert.AreEqual(200.0, size.Value);
                    Assert.AreEqual(4, direction.Items.Count);
                    Assert.AreEqual("TopLeft", direction.SelectedValue);
                    Assert.AreEqual(80.0, horizontal.Value);
                    Assert.AreEqual(60.0, vertical.Value);
                    Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName("panel_PixelBlocksSize")).Visibility);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    Assert.AreEqual(100.0, size.Value);
                    Assert.AreEqual("BottomRight", direction.SelectedValue);
                    Assert.AreEqual(30.0, horizontal.Value);
                    Assert.AreEqual(26.0, vertical.Value);
                    size.Value = 300;
                    direction.SelectedValue = "BottomLeft";
                    horizontal.Value = 100;
                    vertical.Value = 120;
                    slider.Value = 20;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.6, config.MouseEffectSetting.CloudOpacity);
                Assert.AreEqual(2.0, config.MouseEffectSetting.CloudSizeScale);
                Assert.AreEqual("TopLeft", config.MouseEffectSetting.CloudPosition);
                Assert.AreEqual(80.0, config.MouseEffectSetting.CloudHorizontalOffset);
                Assert.AreEqual(60.0, config.MouseEffectSetting.CloudVerticalOffset);
            });
        }

        [TestMethod]
        public void DialogPersistsOnlyCloudAndKeepsSelection()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-Cloud-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.ButterfliesOpacity = 0.4;
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)MouseEffectType.Cloud;
                    var window = new MouseEffectSettingWindow(MouseEffectType.Cloud, config);
                    Exception? failure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            ((Slider)window.FindName("slider_Opacity")).Value = 25;
                            ((Slider)window.FindName("slider_CloudSize")).Value = 175;
                            ((ComboBox)window.FindName("combo_CloudPosition")).SelectedValue = "TopRight";
                            ((Slider)window.FindName("slider_CloudHorizontalOffset")).Value = 90;
                            ((Slider)window.FindName("slider_CloudVerticalOffset")).Value = 70;
                            ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        catch (Exception ex) { failure = ex; }
                        finally { window.Close(); }
                    }, DispatcherPriority.Loaded);
                    Assert.AreEqual(true, window.ShowDialog());
                    if (failure != null) Assert.Fail(failure.ToString());
                    Assert.AreSame(settings, config.MouseEffectSetting);
                    var restored = ConfigHelper.LoadMainConfig().MouseEffectSetting;
                    Assert.AreEqual(0.25, restored.CloudOpacity);
                    Assert.AreEqual(1.75, restored.CloudSizeScale);
                    Assert.AreEqual("TopRight", restored.CloudPosition);
                    Assert.AreEqual(90.0, restored.CloudHorizontalOffset);
                    Assert.AreEqual(70.0, restored.CloudVerticalOffset);
                    Assert.AreEqual(0.4, restored.ButterfliesOpacity);
                    Assert.AreEqual(23, restored.SelectedEffectType);
                    Assert.IsTrue(restored.IsEnabled);
                }
                finally
                {
                    Environment.CurrentDirectory = original;
                    System.IO.Directory.Delete(directory, true);
                }
            });
        }

        [DataTestMethod]
        [DataRow("TopLeft", -1, -1)]
        [DataRow("BottomLeft", -1, 1)]
        [DataRow("TopRight", 1, -1)]
        [DataRow("BottomRight", 1, 1)]
        public void DirectionAndOffsetsApplyLive(string direction, int xSign, int ySign)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting { CloudPosition = direction, CloudHorizontalOffset = 80, CloudVerticalOffset = 60 };
                var effect = new CloudCursorEffect(settings);
                var cursor = new Point(200, 200);
                effect.Update(cursor, 0.01);
                Assert.AreEqual(new Point(200 + xSign * 80, 200 + ySign * 60), (Point)Field(effect, "position"));
                settings.CloudPosition = "TopLeft";
                settings.CloudHorizontalOffset = 40;
                settings.CloudVerticalOffset = 50;
                for (var i = 0; i < 300; i++) effect.Update(cursor, 1.0 / 60);
                Assert.IsTrue((((Point)Field(effect, "position")) - new Point(160, 150)).Length < 0.001);
                settings.CloudHorizontalOffset = 0;
                settings.CloudVerticalOffset = 0;
                effect.Clear();
                effect.Update(cursor, 0.01);
                Assert.AreEqual(cursor, (Point)Field(effect, "position"));
            });
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.5, 0.0, 0.0)]
        [DataRow(500.0, 3.0, 300.0, 300.0)]
        [DataRow(double.NaN, 1.0, 30.0, 26.0)]
        [DataRow(double.PositiveInfinity, 1.0, 30.0, 26.0)]
        [DataRow(double.NegativeInfinity, 1.0, 30.0, 26.0)]
        public void SizeAndOffsetsAreNormalized(double value, double size, double horizontal, double vertical)
        {
            var settings = new MouseEffectSetting { CloudSizeScale = value, CloudHorizontalOffset = value, CloudVerticalOffset = value };
            Assert.AreEqual(size, settings.CloudSizeScale);
            Assert.AreEqual(horizontal, settings.CloudHorizontalOffset);
            Assert.AreEqual(vertical, settings.CloudVerticalOffset);
        }

        [TestMethod]
        public void OldAndInvalidPositionsUseDefaults()
        {
            foreach (var json in new[] { "{}", "{\"CloudPosition\":null}", "{\"CloudPosition\":\"invalid\"}" })
            {
                var settings = JsonConvert.DeserializeObject<MouseEffectSetting>(json)!;
                Assert.AreEqual("BottomRight", settings.CloudPosition);
                Assert.AreEqual(1.0, settings.CloudSizeScale);
                Assert.AreEqual(30.0, settings.CloudHorizontalOffset);
                Assert.AreEqual(26.0, settings.CloudVerticalOffset);
            }
        }

        [DataTestMethod]
        [DataRow(0.5)]
        [DataRow(1.75)]
        [DataRow(3.0)]
        public void SizeChangesLiveWithoutChangingSpringOrCloudCenter(double scale)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new CloudCursorEffect(settings);
                var reference = new CloudCursorEffect();
                effect.Update(new Point(100, 100), 0.01);
                reference.Update(new Point(100, 100), 0.01);
                settings.CloudSizeScale = scale;
                effect.Update(new Point(300, 100), 0.1);
                reference.Update(new Point(300, 100), 0.1);
                var a = Groups(Drawing(reference)).First(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value;
                var b = Groups(Drawing(effect)).First(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value;
                Assert.AreEqual(a.M11 * scale, b.M11, 0.000001);
                Assert.AreEqual(a.M12 * scale, b.M12, 0.000001);
                Assert.AreEqual(a.M21 * scale, b.M21, 0.000001);
                Assert.AreEqual(a.M22 * scale, b.M22, 0.000001);
                Assert.AreEqual(a.OffsetX, b.OffsetX, 0.000001);
                Assert.AreEqual(a.OffsetY, b.OffsetY, 0.000001);
                Assert.AreEqual(Field(reference, "position"), Field(effect, "position"));
                Assert.AreEqual(Field(reference, "velocity"), Field(effect, "velocity"));
                for (var i = 0; i < 11; i++) effect.Update(new Point(300, 100), 0.1);
                Assert.IsTrue(effect.StarCount > 0);
                var star = ((System.Collections.IList)Field(effect, "stars"))[0]!;
                var starSize = (double)star.GetType().GetField("Size")!.GetValue(star)!;
                Assert.IsTrue(starSize >= 2.5 && starSize < 4);
            });
        }

        [TestMethod]
        public void OtherEffectsHideCloudPanel()
        {
            RunSta(() =>
            {
                var window = new MouseEffectSettingWindow(MouseEffectType.Butterflies, new MainConfig());
                try { Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName("panel_Cloud")).Visibility); }
                finally { window.Close(); }
            });
        }

        private static object Field(CloudCursorEffect effect, string name) => typeof(CloudCursorEffect).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(effect)!;
        private static DrawingGroup Drawing(CloudCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
        private static IEnumerable<DrawingGroup> Groups(Drawing drawing)
        {
            if (drawing is DrawingGroup group)
            {
                yield return group;
                foreach (var child in group.Children)
                    foreach (var nested in Groups(child)) yield return nested;
            }
        }
        private static long Alpha(CloudCursorEffect effect)
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
