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
    public class MusicNotesEffectTests
    {
        [TestMethod]
        public void DefaultsAndTypesRemainCompatible()
        {
            Assert.AreEqual(23, (int)MouseEffectType.Cloud);
            Assert.AreEqual(24, (int)MouseEffectType.MusicNotes);
            foreach (var json in new[] { "{}", "{\"MouseEffectSetting\":null}" })
                Assert.AreEqual(1.0, JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting.MusicNotesOpacity);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0)]
        [DataRow(2.0, 1.0)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void OpacityIsNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { MusicNotesOpacity = value }.MusicNotesOpacity);
        }

        [TestMethod]
        public void NotesUseTwoFrozenShapesAndMultipleColors()
        {
            RunSta(() =>
            {
                var effect = new MusicNotesCursorEffect();
                effect.Update(new Point(100, 100), 0.01);
                effect.Update(new Point(188, 100), 0.05);
                Assert.AreEqual(5, effect.ParticleCount);
                var shapes = Groups(Drawing(effect)).SelectMany(g => g.Children.OfType<GeometryDrawing>()).ToArray();
                Assert.AreEqual(5, shapes.Length);
                Assert.AreEqual(2, shapes.Select(s => s.Geometry).Distinct().Count());
                Assert.AreEqual(4, shapes.Select(s => s.Brush).Distinct().Count());
                Assert.IsTrue(shapes.All(s => s.Geometry.IsFrozen && s.Brush.IsFrozen && s.Geometry is GeometryGroup));
                var kinds = shapes.Select(s => (GeometryGroup)s.Geometry).Distinct().ToArray();
                Assert.AreEqual(1, kinds[0].Children.OfType<EllipseGeometry>().Count());
                Assert.AreEqual(2, kinds[1].Children.OfType<EllipseGeometry>().Count());
                Assert.IsFalse(effect.IsHitTestVisible);
                Assert.IsTrue(MusicNotesCursorEffect.PreviewImage.IsFrozen);
                Assert.AreEqual(new Rect(0, 0, 260, 170), MusicNotesCursorEffect.PreviewImage.Drawing.Bounds);
            });
        }

        [TestMethod]
        public void NotesFloatFadeAndExpireWithoutIdleEmission()
        {
            RunSta(() =>
            {
                var effect = new MusicNotesCursorEffect();
                var cursor = new Point(200, 200);
                effect.Update(cursor, 0.01);
                var initial = Groups(Drawing(effect)).First(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value;
                effect.Update(cursor, 0.5);
                var later = Groups(Drawing(effect)).First(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value;
                Assert.IsTrue(later.OffsetY < initial.OffsetY);
                Assert.IsTrue(Groups(Drawing(effect)).Any(g => g.Opacity > 0 && g.Opacity < 1));
                Assert.AreEqual(1, effect.ParticleCount);
                effect.Update(cursor, 2);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(new Point(1000, 200), 0.01);
                Assert.AreEqual(1, effect.ParticleCount);
                var resumed = Groups(Drawing(effect)).First(g => g.Transform != null && !g.Transform.Value.IsIdentity).Transform.Value;
                Assert.AreEqual(1000.0, resumed.OffsetX);
            });
        }

        [TestMethod]
        public void FastMovementInvalidInputAndClearStayBounded()
        {
            RunSta(() =>
            {
                var effect = new MusicNotesCursorEffect();
                foreach (var dt in new[] { 0.0, -1, double.NaN, double.PositiveInfinity }) effect.Update(new Point(100, 100), dt);
                effect.Update(new Point(double.NaN, 100), 0.01);
                effect.Update(new Point(100, double.PositiveInfinity), 0.01);
                Assert.AreEqual(0, effect.ParticleCount);
                effect.Update(new Point(100, 100), 0.01);
                effect.Update(new Point(10000, 100), 0.05);
                Assert.AreEqual(1 + MusicNotesCursorEffect.MaxEmissionPerUpdate, effect.ParticleCount);
                for (var i = 0; i < 1000; i++) effect.Update(new Point(i % 2 * 10000, 100), 0.05);
                Assert.IsTrue(effect.ParticleCount <= MusicNotesCursorEffect.MaxParticles);
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
        public void OpacityUpdatesLive()
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = new MusicNotesCursorEffect(settings);
                effect.Measure(new Size(400, 400));
                effect.Arrange(new Rect(0, 0, 400, 400));
                effect.Update(new Point(200, 200), 0.01);
                Assert.IsTrue(Alpha(effect) > 0);
                settings.MusicNotesOpacity = 0;
                effect.Update(new Point(200, 200), 0.01);
                Assert.AreEqual(0L, Alpha(effect));
                settings.MusicNotesOpacity = 0.5;
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
                    window.UpdateMouseEffectType(MouseEffectType.MusicNotes);
                    var create = typeof(MouseEffectWindow).GetMethod("CreateEffects", BindingFlags.NonPublic | BindingFlags.Instance)!;
                    create.Invoke(window, null);
                    create.Invoke(window, null);
                    var canvas = (Canvas)window.FindName("canvas");
                    Assert.AreEqual(1, canvas.Children.Count);
                    var effect = (MusicNotesCursorEffect)canvas.Children[0];
                    effect.Update(new Point(100, 100), 0.01);
                    if (switchEffect) window.UpdateMouseEffectType(MouseEffectType.Cloud);
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
                config.MouseEffectSetting.MusicNotesOpacity = 0.6;
                var window = new MouseEffectSettingWindow(MouseEffectType.MusicNotes, config);
                try
                {
                    Assert.AreEqual("音符轨迹设置", window.Title);
                    var slider = (Slider)window.FindName("slider_Opacity");
                    Assert.AreEqual(60.0, slider.Value);
                    Assert.AreEqual(Visibility.Collapsed, ((FrameworkElement)window.FindName("panel_Cloud")).Visibility);
                    Assert.AreEqual(Visibility.Visible, ((FrameworkElement)window.FindName("panel_ParticleOpacity")).Visibility);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual(100.0, slider.Value);
                    slider.Value = 20;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.6, config.MouseEffectSetting.MusicNotesOpacity);
            });
        }

        [TestMethod]
        public void DialogPersistsOnlyMusicNotesAndKeepsSelection()
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-MusicNotes-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var settings = config.MouseEffectSetting;
                    settings.CloudOpacity = 0.4;
                    settings.CloudSizeScale = 2;
                    settings.IsEnabled = true;
                    settings.SelectedEffectType = (int)MouseEffectType.MusicNotes;
                    var window = new MouseEffectSettingWindow(MouseEffectType.MusicNotes, config);
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
                    Assert.AreEqual(0.25, restored.MusicNotesOpacity);
                    Assert.AreEqual(0.4, restored.CloudOpacity);
                    Assert.AreEqual(2.0, restored.CloudSizeScale);
                    Assert.AreEqual(24, restored.SelectedEffectType);
                    Assert.IsTrue(restored.IsEnabled);
                }
                finally
                {
                    Environment.CurrentDirectory = original;
                    System.IO.Directory.Delete(directory, true);
                }
            });
        }

        private static DrawingGroup Drawing(MusicNotesCursorEffect effect) => ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing;
        private static IEnumerable<DrawingGroup> Groups(Drawing drawing)
        {
            if (drawing is DrawingGroup group)
            {
                yield return group;
                foreach (var child in group.Children)
                    foreach (var nested in Groups(child)) yield return nested;
            }
        }
        private static long Alpha(MusicNotesCursorEffect effect)
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
