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
    public class FollowingAnimalEffectTests
    {
        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{\"FollowingSize\":50}}")]
        public void DefaultsAndTypesRemainCompatible(string json)
        {
            Assert.AreEqual(17, (int)MouseEffectType.HaloRing);
            Assert.AreEqual(18, (int)MouseEffectType.FollowingAnimal);
            Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.FollowingAnimalOpacity);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { FollowingAnimalOpacity = value }.FollowingAnimalOpacity);
        }

        [TestMethod]
        public void CatFollowsSmoothlyTurnsAndSettlesBesideCursor()
        {
            RunSta(() =>
            {
                var effect = new FollowingAnimalCursorEffect();
                effect.Update(new Point(100, 100), 0.016);
                Assert.AreEqual(64.0, Transform(effect).OffsetX);
                Assert.AreEqual(128.0, Transform(effect).OffsetY);
                effect.Update(new Point(200, 100), 0.1);
                var chasing = Transform(effect);
                Assert.IsTrue(chasing.OffsetX > 64 && chasing.OffsetX < 164);
                Assert.AreEqual(1.0, chasing.M11);
                effect.Update(new Point(200, 100), 2);
                Assert.AreEqual(164.0, Transform(effect).OffsetX, 0.00001);
                Assert.AreEqual(128.0, Transform(effect).OffsetY, 0.00001);
                effect.Update(new Point(50, 100), 0.1);
                Assert.AreEqual(-1.0, Transform(effect).M11);
                Assert.IsTrue(Transform(effect).OffsetX < 164);
                Assert.IsFalse(effect.IsHitTestVisible);
            });
        }

        [TestMethod]
        public void StationaryCatWagsTailAndDoesNotAccumulateVisuals()
        {
            RunSta(() =>
            {
                var effect = new FollowingAnimalCursorEffect();
                var point = new Point(150, 100);
                effect.Update(point, 0.01);
                var before = Transform(effect);
                var tail = Shapes(Drawing(effect)).First(s => s.Pen?.Thickness == 5).Geometry.ToString();
                var count = Shapes(Drawing(effect)).Count();
                effect.Update(point, 0.4);
                Assert.AreEqual(before, Transform(effect));
                Assert.AreNotEqual(tail, Shapes(Drawing(effect)).First(s => s.Pen?.Thickness == 5).Geometry.ToString());
                Assert.IsTrue(Shapes(Drawing(effect)).Count(s => s.Geometry is EllipseGeometry) >= 6);
                Assert.IsTrue(Shapes(Drawing(effect)).Any(s => s.Geometry is LineGeometry));
                Assert.IsTrue(FollowingAnimalCursorEffect.PreviewImage.IsFrozen);
                for (var i = 0; i < 500; i++) effect.Update(new Point(i, 100), 0.016);
                Assert.AreEqual(1, VisualTreeHelper.GetChildrenCount(effect));
                Assert.AreEqual(count, Shapes(Drawing(effect)).Count());
                effect.Clear();
                effect.Clear();
                Assert.IsFalse(Shapes(Drawing(effect)).Any());
                effect.Update(point, 0.01);
                Assert.AreEqual(before, Transform(effect));
            });
        }

        [TestMethod]
        public void FollowingIsIndependentOfFrameInterval()
        {
            RunSta(() =>
            {
                var first = new FollowingAnimalCursorEffect();
                var second = new FollowingAnimalCursorEffect();
                first.Update(new Point(100, 100), 0.01);
                second.Update(new Point(100, 100), 0.01);
                first.Update(new Point(200, 200), 0.2);
                for (var i = 0; i < 20; i++) second.Update(new Point(200, 200), 0.01);
                Assert.AreEqual(Transform(first).OffsetX, Transform(second).OffsetX, 0.000001);
                Assert.AreEqual(Transform(first).OffsetY, Transform(second).OffsetY, 0.000001);
            });
        }

        [TestMethod]
        public void InvalidInputIsIgnoredAndOpacityUpdatesLive()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new FollowingAnimalCursorEffect(settings);
                foreach (var elapsed in new[] { 0.0, -1, double.NaN, double.PositiveInfinity }) effect.Update(new Point(100, 100), elapsed);
                effect.Update(new Point(double.NaN, 100), 0.016);
                effect.Update(new Point(100, double.PositiveInfinity), 0.016);
                Assert.IsFalse(Shapes(Drawing(effect)).Any());
                effect.Measure(new Size(200, 200));
                effect.Arrange(new Rect(0, 0, 200, 200));
                effect.Update(new Point(100, 100), 0.1);
                var initial = Alpha(effect);
                Assert.IsTrue(initial > 0);
                settings.FollowingAnimalOpacity = 0;
                effect.Update(new Point(100, 100), 2);
                Assert.AreEqual(0L, Alpha(effect));
                settings.FollowingAnimalOpacity = 0.25;
                effect.Update(new Point(100, 100), 2);
                Assert.IsTrue(Alpha(effect) > 0 && Alpha(effect) < initial);
                Assert.AreEqual(1.0, settings.HaloRingOpacity);
            });
        }

        [DataTestMethod]
        [DataRow("switch")]
        [DataRow("start")]
        [DataRow("stop")]
        [DataRow("close")]
        public void WindowClearsAnimal(string action)
        {
            RunSta(() =>
            {
                var window = new MouseEffectWindow();
                try
                {
                    window.UpdateMouseEffectType(MouseEffectType.FollowingAnimal);
                    var effect = new FollowingAnimalCursorEffect();
                    effect.Update(new Point(100, 100), 0.016);
                    var field = typeof(MouseEffectWindow).GetField("followingAnimalEffect", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    field.SetValue(window, effect);
                    var canvas = (Canvas)window.FindName("canvas");
                    canvas.Children.Add(effect);
                    if (action == "switch") window.UpdateMouseEffectType(MouseEffectType.FollowingDot);
                    else if (action == "start") window.StartMouseEffect();
                    else if (action == "stop") window.StopMouseEffect();
                    else window.Close();
                    Assert.IsFalse(Shapes(Drawing(effect)).Any());
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
                config.MouseEffectSetting.FollowingAnimalOpacity = 0.6;
                var window = new MouseEffectSettingWindow(MouseEffectType.FollowingAnimal, config);
                try
                {
                    Assert.AreEqual("跟随小动物设置", window.Title);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(60.0, slider.Value);
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_ParticleOpacity")).Visibility);
                    foreach (var name in new[] { "panel_Following", "panel_Trail", "panel_Expansion", "panel_StardustSize", "panel_InkFluid", "panel_NeonRibbon" })
                        Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName(name)).Visibility);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    slider.Value = 20;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.6, config.MouseEffectSetting.FollowingAnimalOpacity);
            });
        }

        [TestMethod]
        public void DialogPersistsOnlyAnimalAndKeepsSelection()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-AnimalTests-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.FollowingSize = 60;
                    settings.HaloRingOpacity = 0.6;
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)MouseEffectType.FollowingAnimal;
                    var window = new MouseEffectSettingWindow(MouseEffectType.FollowingAnimal, config);
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
                    Assert.AreEqual(0.25, settings.FollowingAnimalOpacity);
                    var restored = ConfigHelper.LoadMainConfig();
                    Assert.AreEqual(0.25, restored.MouseEffectSetting.FollowingAnimalOpacity);
                    Assert.AreEqual(0.6, restored.MouseEffectSetting.HaloRingOpacity);
                    Assert.AreEqual(60.0, restored.MouseEffectSetting.FollowingSize);
                    Assert.IsTrue(restored.MouseEffectSetting.IsEnabled);
                    Assert.AreEqual(18, restored.MouseEffectSetting.SelectedEffectType);
                }
                finally
                {
                    Environment.CurrentDirectory = original;
                    System.IO.Directory.Delete(directory, true);
                }
            });
        }

        private static DrawingGroup Drawing(FollowingAnimalCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
        private static Matrix Transform(FollowingAnimalCursorEffect effect) => Groups(Drawing(effect)).Single(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value;
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
        private static long Alpha(FollowingAnimalCursorEffect effect)
        {
            var bitmap = new RenderTargetBitmap(200, 200, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(effect);
            var pixels = new byte[200 * 200 * 4];
            bitmap.CopyPixels(pixels, 200 * 4, 0);
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
