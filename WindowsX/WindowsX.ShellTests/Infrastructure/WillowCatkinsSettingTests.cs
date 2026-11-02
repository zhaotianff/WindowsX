using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections;
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
    public class WillowCatkinsSettingTests
    {
        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{\"WillowCatkinsOpacity\":0.4}}")]
        public void OldConfigurationsKeepDefaults(string json)
        {
            var settings = JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting;
            Assert.AreEqual("#FFFAF0", settings.WillowCatkinsColor);
            Assert.AreEqual(1.0, settings.WillowCatkinsSizeScale);
        }

        [DataTestMethod]
        [DataRow(" #aa12ef ", "#AA12EF")]
        [DataRow("red", "#FFFAF0")]
        [DataRow("#123", "#FFFAF0")]
        [DataRow("#GG0011", "#FFFAF0")]
        [DataRow(null, "#FFFAF0")]
        public void ColorsAreNormalized(string? value, string expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { WillowCatkinsColor = value }.WillowCatkinsColor);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.5)]
        [DataRow(10.0, 3.0)]
        [DataRow(1.75, 1.75)]
        [DataRow(double.NaN, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0)]
        public void SizesAreNormalized(double value, double expected)
        {
            Assert.AreEqual(expected, new MouseEffectSetting { WillowCatkinsSizeScale = value }.WillowCatkinsSizeScale);
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RunningParticlesApplyColorSizeAndOpacity(bool gdi)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting { WillowCatkinsColor = "#FF0000", WillowCatkinsSizeScale = 0.5 };
                UIElement effect = gdi ? new GdiParticleCursorEffect(MouseEffectType.WillowCatkinsGdiPlus, settings)
                    : new ParticleCursorEffect(MouseEffectType.WillowCatkins, settings);
                try
                {
                    effect.Measure(new Size(400, 400));
                    effect.Arrange(new Rect(0, 0, 400, 400));
                    Update(effect, 0.016);
                    Update(effect, 0.2);
                    var small = Pixels(effect);
                    Assert.IsTrue(small.Where((_, i) => i % 4 == 3).Sum(b => (long)b) > 0);
                    Assert.IsTrue(small.Where((_, i) => i % 4 == 0 || i % 4 == 1).All(b => b == 0));
                    settings.WillowCatkinsColor = "#0000FF";
                    settings.WillowCatkinsSizeScale = 3;
                    Update(effect, 0.000001);
                    var large = Pixels(effect);
                    Assert.IsTrue(large.Where((_, i) => i % 4 == 3).Sum(b => (long)b)
                        > small.Where((_, i) => i % 4 == 3).Sum(b => (long)b) * 3);
                    Assert.IsTrue(large.Where((_, i) => i % 4 == 1 || i % 4 == 2).All(b => b == 0));
                    Assert.AreEqual(3, gdi ? ((GdiParticleCursorEffect)effect).ParticleCount : ((ParticleCursorEffect)effect).ParticleCount);
                    settings.WillowCatkinsOpacity = 0;
                    Update(effect, 0.000001);
                    Assert.IsTrue(Pixels(effect).All(b => b == 0));
                }
                finally { (effect as IDisposable)?.Dispose(); }
            });
        }

        [DataTestMethod]
        [DataRow(1.0)]
        [DataRow(1.5)]
        [DataRow(2.0)]
        public void GdiBufferContainsEnlargedHalos(double dpiScale)
        {
            RunSta(() =>
            {
                using var effect = new GdiParticleCursorEffect(MouseEffectType.WillowCatkinsGdiPlus,
                    new MouseEffectSetting { WillowCatkinsSizeScale = 3 });
                effect.Update(new Point(200, 200), 0.016, new Size(400, 400), new DpiScale(dpiScale, dpiScale));
                effect.Update(new Point(200, 200), 0.2, new Size(400, 400), new DpiScale(dpiScale, dpiScale));
                var drawing = ((DrawingVisual)VisualTreeHelper.GetChild(effect, 0)).Drawing.Children.OfType<ImageDrawing>().Single();
                var particles = (IList)typeof(GdiParticleCursorEffect).GetField("particles", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(effect)!;
                foreach (var particle in particles)
                {
                    var position = (Point)particle!.GetType().GetField("Position")!.GetValue(particle)!;
                    var size = (double)particle.GetType().GetField("Size")!.GetValue(particle)! * 3;
                    Assert.IsTrue(drawing.Rect.Contains(new Rect(position.X - size * 2 - 1, position.Y - size * 1.6 - 1, size * 4 + 2, size * 3.2 + 2)));
                }
                var image = (BitmapSource)drawing.ImageSource;
                Assert.AreEqual(96 * dpiScale, image.DpiX, 0.001);
                var pixels = new byte[image.PixelWidth * image.PixelHeight * 4];
                image.CopyPixels(pixels, image.PixelWidth * 4, 0);
                for (var y = 0; y < image.PixelHeight; y++)
                    for (var x = 0; x < image.PixelWidth; x++)
                        if (x == 0 || y == 0 || x == image.PixelWidth - 1 || y == image.PixelHeight - 1)
                            Assert.AreEqual((byte)0, pixels[(y * image.PixelWidth + x) * 4 + 3]);
            });
        }

        [DataTestMethod]
        [DataRow(MouseEffectType.WillowCatkins)]
        [DataRow(MouseEffectType.WillowCatkinsGdiPlus)]
        public void DialogValidatesResetsAndDiscardsDraft(MouseEffectType type)
        {
            RunSta(() =>
            {
                var config = new MainConfig();
                config.MouseEffectSetting.WillowCatkinsColor = "#ABCDEF";
                config.MouseEffectSetting.WillowCatkinsSizeScale = 2;
                var window = new MouseEffectSettingWindow(type, config);
                try
                {
                    Assert.AreEqual(Visibility.Visible, ((StackPanel)window.FindName("panel_Catkins")).Visibility);
                    var color = (TextBox)window.FindName("text_CatkinsColor");
                    var size = (Slider)window.FindName("slider_CatkinsSize");
                    var save = (Button)window.FindName("btn_Save");
                    Assert.AreEqual("#ABCDEF", color.Text);
                    Assert.AreEqual(200.0, size.Value);
                    color.Text = "invalid";
                    Assert.IsFalse(save.IsEnabled);
                    Assert.AreEqual(Visibility.Visible, ((TextBlock)window.FindName("text_CatkinsColorError")).Visibility);
                    ((Button)window.FindName("btn_Reset")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    Assert.AreEqual("#FFFAF0", color.Text);
                    Assert.AreEqual(100.0, size.Value);
                    Assert.IsTrue(save.IsEnabled);
                    Assert.AreEqual(Colors.FloralWhite, ((SolidColorBrush)((Border)window.FindName("preview_CatkinsColor")).Background).Color);
                }
                finally { window.Close(); }
                Assert.AreEqual("#ABCDEF", config.MouseEffectSetting.WillowCatkinsColor);
                Assert.AreEqual(2.0, config.MouseEffectSetting.WillowCatkinsSizeScale);
            });
        }

        [DataTestMethod]
        [DataRow(MouseEffectType.Fireworks)]
        [DataRow(MouseEffectType.Stardust)]
        public void OtherDialogsHideCatkinSettings(MouseEffectType type)
        {
            RunSta(() =>
            {
                var window = new MouseEffectSettingWindow(type, new MainConfig());
                try { Assert.AreEqual(Visibility.Collapsed, ((StackPanel)window.FindName("panel_Catkins")).Visibility); }
                finally { window.Close(); }
            });
        }

        [DataTestMethod]
        [DataRow(MouseEffectType.WillowCatkins)]
        [DataRow(MouseEffectType.WillowCatkinsGdiPlus)]
        public void SavePersistsSharedSettingsAndPreservesOtherEffects(MouseEffectType type)
        {
            RunSta(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    var config = new MainConfig();
                    var shared = config.MouseEffectSetting;
                    shared.FireworksOpacity = 0.4;
                    shared.IsEnabled = true;
                    shared.SelectedEffectType = (int)type;
                    var window = new MouseEffectSettingWindow(type, config);
                    Exception? failure = null;
                    window.Dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            ((TextBox)window.FindName("text_CatkinsColor")).Text = "#ff0077";
                            ((Slider)window.FindName("slider_CatkinsSize")).Value = 250;
                            ((Slider)window.FindName("slider_Opacity")).Value = 60;
                            ((Button)window.FindName("btn_Save")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                        catch (Exception ex) { failure = ex; }
                        finally { window.Close(); }
                    }, DispatcherPriority.Loaded);
                    Assert.AreEqual(true, window.ShowDialog());
                    if (failure != null) Assert.Fail(failure.ToString());
                    Assert.AreSame(shared, config.MouseEffectSetting);
                    var restored = ConfigHelper.LoadMainConfig().MouseEffectSetting;
                    foreach (var settings in new[] { shared, restored })
                    {
                        Assert.AreEqual("#FF0077", settings.WillowCatkinsColor);
                        Assert.AreEqual(2.5, settings.WillowCatkinsSizeScale);
                        Assert.AreEqual(0.6, settings.WillowCatkinsOpacity);
                        Assert.AreEqual(0.4, settings.FireworksOpacity);
                        Assert.IsTrue(settings.IsEnabled);
                        Assert.AreEqual((int)type, settings.SelectedEffectType);
                    }
                }
                finally
                {
                    Environment.CurrentDirectory = original;
                    System.IO.Directory.Delete(directory, true);
                }
            });
        }

        private static void Update(UIElement effect, double elapsed)
        {
            if (effect is ParticleCursorEffect wpf) wpf.Update(new Point(200, 200), elapsed);
            else ((GdiParticleCursorEffect)effect).Update(new Point(200, 200), elapsed, new Size(400, 400), new DpiScale(1, 1));
        }

        private static byte[] Pixels(UIElement effect)
        {
            var bitmap = new RenderTargetBitmap(400, 400, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(effect);
            var pixels = new byte[400 * 400 * 4];
            bitmap.CopyPixels(pixels, 400 * 4, 0);
            return pixels;
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
