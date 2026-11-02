using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect;
using WindowsX.Shell.Windows;

namespace WindowsX.ShellTests.Infrastructure
{
    [TestClass]
    public class MouseEffectSettingTests
    {
        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{}}")]
        public void OldConfigurationsKeepOriginalAppearance(string json)
        {
            var config = JsonConvert.DeserializeObject<MainConfig>(json)!;
            Assert.AreEqual(1.0, config.MouseEffectSetting.FireworksOpacity);
            Assert.AreEqual(1.0, config.MouseEffectSetting.WillowCatkinsOpacity);
            Assert.AreEqual(1.0, config.MouseEffectSetting.FireworksExpansion);
        }

        [TestMethod]
        public void SettingsSurviveJsonRoundTrip()
        {
            var config = new MainConfig();
            config.MouseEffectSetting.FireworksOpacity = 0.42;
            config.MouseEffectSetting.WillowCatkinsOpacity = 0.67;
            config.MouseEffectSetting.FireworksExpansion = 1.75;
            var restored = JsonConvert.DeserializeObject<MainConfig>(JsonConvert.SerializeObject(config))!;
            Assert.AreEqual(0.42, restored.MouseEffectSetting.FireworksOpacity);
            Assert.AreEqual(0.67, restored.MouseEffectSetting.WillowCatkinsOpacity);
            Assert.AreEqual(1.75, restored.MouseEffectSetting.FireworksExpansion);
        }

        [DataTestMethod]
        [DataRow(-1.0, 0.0, 0.25)]
        [DataRow(10.0, 1.0, 3.0)]
        [DataRow(double.NaN, 1.0, 1.0)]
        [DataRow(double.PositiveInfinity, 1.0, 1.0)]
        [DataRow(double.NegativeInfinity, 1.0, 1.0)]
        public void InvalidValuesAreNormalized(double value, double opacity, double expansion)
        {
            var settings = new MouseEffectSetting
            {
                FireworksOpacity = value,
                WillowCatkinsOpacity = value,
                FireworksExpansion = value
            };
            Assert.AreEqual(opacity, settings.FireworksOpacity);
            Assert.AreEqual(opacity, settings.WillowCatkinsOpacity);
            Assert.AreEqual(expansion, settings.FireworksExpansion);
        }

        [DataTestMethod]
        [DataRow(MouseEffectType.Fireworks)]
        [DataRow(MouseEffectType.WillowCatkins)]
        [DataRow(MouseEffectType.FireworksGdiPlus)]
        [DataRow(MouseEffectType.WillowCatkinsGdiPlus)]
        public void OpacityChangesAffectRunningParticles(MouseEffectType type)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = CreateEffect(type, settings);
                try
                {
                    effect.Measure(new Size(640, 480));
                    effect.Arrange(new Rect(0, 0, 640, 480));
                    Update(effect);
                    Update(effect);
                    var originalAlpha = GetTotalAlpha(effect);
                    Assert.IsTrue(originalAlpha > 0);
                    SetOpacity(settings, type, 0.25);
                    Update(effect);
                    var reducedAlpha = GetTotalAlpha(effect);
                    Assert.IsTrue(reducedAlpha > 0 && reducedAlpha < originalAlpha);
                    SetOpacity(settings, type, 0);
                    Update(effect);
                    Assert.AreEqual(0L, GetTotalAlpha(effect));
                    SetOpacity(settings, type, 1);
                    Update(effect);
                    Assert.IsTrue(GetTotalAlpha(effect) > 0);
                }
                finally { (effect as IDisposable)?.Dispose(); }
            });
        }

        [DataTestMethod]
        [DataRow(MouseEffectType.Fireworks)]
        [DataRow(MouseEffectType.FireworksGdiPlus)]
        public void ExpansionControlsNewFireworksVelocity(MouseEffectType type)
        {
            RunSta(() =>
            {
                var settings = new MouseEffectSetting();
                var effect = CreateEffect(type, settings);
                try
                {
                    foreach (var expansion in new[] { 0.25, 1.0, 3.0 })
                    {
                        if (effect is ParticleCursorEffect wpf) wpf.Clear();
                        else ((GdiParticleCursorEffect)effect).Clear();
                        settings.FireworksExpansion = expansion;
                        Update(effect);
                        var particles = (IList)effect.GetType().GetField("particles", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(effect)!;
                        Assert.AreEqual(28, particles.Count);
                        foreach (var particle in particles)
                        {
                            var velocity = (Vector)particle!.GetType().GetField("Velocity")!.GetValue(particle)!;
                            Assert.IsTrue(velocity.Length >= 65 * expansion - 0.001);
                            Assert.IsTrue(velocity.Length <= 175 * expansion + 0.001);
                        }
                    }
                }
                finally { (effect as IDisposable)?.Dispose(); }
            });
        }

        [DataTestMethod]
        [DataRow(MouseEffectType.Fireworks, true)]
        [DataRow(MouseEffectType.FireworksGdiPlus, true)]
        [DataRow(MouseEffectType.WillowCatkins, false)]
        [DataRow(MouseEffectType.WillowCatkinsGdiPlus, false)]
        public void DialogLoadsRelevantSettingsAndClosingDiscardsEdits(MouseEffectType type, bool fireworks)
        {
            RunSta(() =>
            {
                var config = new MainConfig();
                config.MouseEffectSetting.FireworksOpacity = 0.42;
                config.MouseEffectSetting.WillowCatkinsOpacity = 0.67;
                config.MouseEffectSetting.FireworksExpansion = 1.75;
                var window = new MouseEffectSettingWindow(type, config);
                try
                {
                    var opacity = (Slider)window.FindName("slider_Opacity");
                    var expansion = (Slider)window.FindName("slider_Expansion");
                    var panel = (StackPanel)window.FindName("panel_Expansion");
                    Assert.AreEqual(fireworks ? "烟花设置" : "春日柳絮设置", window.Title);
                    Assert.AreEqual(fireworks ? 42.0 : 67.0, opacity.Value, 0.001);
                    Assert.AreEqual(175.0, expansion.Value, 0.001);
                    Assert.AreEqual(fireworks ? Visibility.Visible : Visibility.Collapsed, panel.Visibility);
                    opacity.Value = 20;
                    expansion.Value = 250;
                }
                finally { window.Close(); }
                Assert.AreEqual(0.42, config.MouseEffectSetting.FireworksOpacity);
                Assert.AreEqual(0.67, config.MouseEffectSetting.WillowCatkinsOpacity);
                Assert.AreEqual(1.75, config.MouseEffectSetting.FireworksExpansion);
            });
        }

        private static UIElement CreateEffect(MouseEffectType type, MouseEffectSetting settings)
        {
            return type == MouseEffectType.Fireworks || type == MouseEffectType.WillowCatkins
                ? new ParticleCursorEffect(type, settings)
                : new GdiParticleCursorEffect(type, settings);
        }

        private static void Update(UIElement effect)
        {
            var cursor = new Point(320, 240);
            if (effect is ParticleCursorEffect wpf) wpf.Update(cursor, 0.05);
            else ((GdiParticleCursorEffect)effect).Update(cursor, 0.05, new Size(640, 480), new DpiScale(1, 1));
        }

        private static void SetOpacity(MouseEffectSetting settings, MouseEffectType type, double opacity)
        {
            if (type == MouseEffectType.Fireworks || type == MouseEffectType.FireworksGdiPlus)
                settings.FireworksOpacity = opacity;
            else
                settings.WillowCatkinsOpacity = opacity;
        }

        private static long GetTotalAlpha(UIElement effect)
        {
            var bitmap = new RenderTargetBitmap(640, 480, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(effect);
            var pixels = new byte[640 * 480 * 4];
            bitmap.CopyPixels(pixels, 640 * 4, 0);
            long alpha = 0;
            for (var i = 3; i < pixels.Length; i += 4)
                alpha += pixels[i];
            return alpha;
        }

        private static void RunSta(Action action)
        {
            Exception? failure = null;
            var thread = new Thread(() =>
            {
                try { action(); }
                catch (Exception ex) { failure = ex; }
                finally { System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown(); }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (failure != null)
                Assert.Fail(failure.ToString());
        }
    }
}
