using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WindowsX.Config.Helper;
using WindowsX.Config.Model;
using WindowsX.Shell.Infrastructure.MouseEffect;
using WindowsX.Shell.View.Beautify.UserControls;
using WindowsX.Shell.Windows;
using MouseEffectPage = WindowsX.Shell.Pages.MouseEffect;

namespace WindowsX.ShellTests.Infrastructure
{
    [TestClass]
    [DoNotParallelize]
    public class MouseEffectStartupTests
    {
        private static Thread applicationThread = null!;
        private static Application application = null!;

        [ClassInitialize]
        public static void InitializeApplication(TestContext context)
        {
            using var ready = new ManualResetEventSlim();
            Exception? failure = null;
            applicationThread = new Thread(() =>
            {
                try
                {
                    application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                    foreach (var resource in new[] { "Styles/ButtonStyle.xaml", "Styles/GroupBoxStyle.xaml" })
                        application.Resources.MergedDictionaries.Add(new ResourceDictionary
                        {
                            Source = new Uri("pack://application:,,,/WindowsX.Shell;component/" + resource)
                        });
                }
                catch (Exception ex) { failure = ex; }
                finally { ready.Set(); }
                if (failure == null) Dispatcher.Run();
            });
            applicationThread.SetApartmentState(ApartmentState.STA);
            applicationThread.Start();
            ready.Wait();
            if (failure != null) Assert.Fail(failure.ToString());
        }

        [ClassCleanup]
        public static void ShutdownApplication()
        {
            if (applicationThread.IsAlive)
            {
                application.Dispatcher.Invoke(() => application.Shutdown());
                applicationThread.Join();
            }
        }

        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{\"MouseEffectSetting\":null}")]
        [DataRow("{\"MouseEffectSetting\":{\"InkFluidOpacity\":0.5}}")]
        public void OldConfigurationsDefaultToDisabled(string json)
        {
            var settings = JsonConvert.DeserializeObject<MainConfig>(json)!.MouseEffectSetting;
            Assert.IsFalse(settings.IsEnabled);
            Assert.AreEqual(0, settings.SelectedEffectType);
        }

        [DataTestMethod]
        [DataRow(0, MouseEffectType.FollowingDot)]
        [DataRow(5, MouseEffectType.WillowCatkinsGdiPlus)]
        [DataRow(8, MouseEffectType.InkFluid)]
        [DataRow(-1, MouseEffectType.FollowingDot)]
        [DataRow(999, MouseEffectType.FollowingDot)]
        public void StartupRestoresSelectedEffectWithoutRewritingConfig(int selected, MouseEffectType expected)
        {
            RunIsolated(() =>
            {
                var config = new MainConfig();
                config.MouseEffectSetting.IsEnabled = true;
                config.MouseEffectSetting.SelectedEffectType = selected;
                Assert.IsTrue(ConfigHelper.SaveMainConfig(config));
                var path = System.IO.Path.Combine("config", "main.json");
                var before = System.IO.File.ReadAllText(path);
                var restored = ConfigHelper.LoadMainConfig();
                var page = new MouseEffectPage(restored);
                try
                {
                    Assert.IsNull(GetWindow(page));
                    page.RestoreMouseEffect();
                    var window = GetWindow(page)!;
                    Assert.IsNotNull(window);
                    Assert.IsTrue(window.IsVisible);
                    Assert.AreEqual(true, GetCheckBox(page).IsChecked);
                    Assert.AreEqual(expected, GetEffectType(window));
                    page.RestoreMouseEffect();
                    Assert.AreSame(window, GetWindow(page));
                    Assert.AreEqual(before, System.IO.File.ReadAllText(path));
                    page.CloseMouseEffectWindow();
                    page.CloseMouseEffectWindow();
                    Assert.IsNull(GetWindow(page));
                    Assert.IsTrue(restored.MouseEffectSetting.IsEnabled);
                    Assert.IsTrue(ConfigHelper.LoadMainConfig().MouseEffectSetting.IsEnabled);
                }
                finally { page.CloseMouseEffectWindow(); }
            });
        }

        [TestMethod]
        public void DisabledStartupDoesNotOpenWindowOrWriteConfig()
        {
            RunIsolated(() =>
            {
                var page = new MouseEffectPage(new MainConfig());
                try
                {
                    page.RestoreMouseEffect();
                    Assert.IsNull(GetWindow(page));
                    Assert.AreEqual(false, GetCheckBox(page).IsChecked);
                    Assert.IsFalse(System.IO.Directory.Exists("config"));
                }
                finally { page.CloseMouseEffectWindow(); }
            });
        }

        [TestMethod]
        public void EnableSelectRestartAndDisablePersistImmediately()
        {
            RunIsolated(() =>
            {
                var config = new MainConfig();
                config.MouseEffectSetting.InkFluidColor1 = "#FF0000";
                config.MouseEffectSetting.InkFluidOpacity = 0.4;
                var page = new MouseEffectPage(config);
                MouseEffectPage? restarted = null;
                try
                {
                    GetCheckBox(page).IsChecked = true;
                    Assert.IsNotNull(GetWindow(page));
                    Assert.IsTrue(ConfigHelper.LoadMainConfig().MouseEffectSetting.IsEnabled);
                    var card = new MouseEffectItemControl { MouseEffectType = MouseEffectType.InkFluid };
                    typeof(MouseEffectPage).GetMethod("MouseEffectItemControl_OnSet", BindingFlags.Instance | BindingFlags.NonPublic)!
                        .Invoke(page, new object[] { card, new RoutedEventArgs() });
                    Assert.AreEqual(MouseEffectType.InkFluid, GetEffectType(GetWindow(page)!));
                    var saved = ConfigHelper.LoadMainConfig();
                    Assert.AreEqual((int)MouseEffectType.InkFluid, saved.MouseEffectSetting.SelectedEffectType);
                    Assert.AreEqual("#FF0000", saved.MouseEffectSetting.InkFluidColor1);
                    Assert.AreEqual(0.4, saved.MouseEffectSetting.InkFluidOpacity);
                    page.CloseMouseEffectWindow();
                    restarted = new MouseEffectPage(saved);
                    restarted.RestoreMouseEffect();
                    Assert.AreEqual(MouseEffectType.InkFluid, GetEffectType(GetWindow(restarted)!));
                    GetCheckBox(restarted).IsChecked = false;
                    Assert.IsNull(GetWindow(restarted));
                    var disabled = ConfigHelper.LoadMainConfig();
                    Assert.IsFalse(disabled.MouseEffectSetting.IsEnabled);
                    Assert.AreEqual((int)MouseEffectType.InkFluid, disabled.MouseEffectSetting.SelectedEffectType);
                    GetCheckBox(restarted).IsChecked = true;
                    Assert.AreEqual(MouseEffectType.InkFluid, GetEffectType(GetWindow(restarted)!));
                    Assert.IsTrue(ConfigHelper.LoadMainConfig().MouseEffectSetting.IsEnabled);
                }
                finally
                {
                    page.CloseMouseEffectWindow();
                    restarted?.CloseMouseEffectWindow();
                }
            });
        }

        private static CheckBox GetCheckBox(MouseEffectPage page)
        {
            return (CheckBox)page.FindName("cbx_EnableMouseEffect");
        }

        private static MouseEffectWindow? GetWindow(MouseEffectPage page)
        {
            return (MouseEffectWindow?)typeof(MouseEffectPage).GetField("mouseEffectWindow", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(page);
        }

        private static MouseEffectType GetEffectType(MouseEffectWindow window)
        {
            return (MouseEffectType)typeof(MouseEffectWindow).GetField("currentEffectType", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(window)!;
        }

        private static void RunIsolated(Action action)
        {
            application.Dispatcher.Invoke(() =>
            {
                var original = Environment.CurrentDirectory;
                var directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX-MouseStartupTests-" + Guid.NewGuid());
                System.IO.Directory.CreateDirectory(directory);
                try
                {
                    Environment.CurrentDirectory = directory;
                    action();
                }
                finally
                {
                    Environment.CurrentDirectory = original;
                    System.IO.Directory.Delete(directory, true);
                }
            });
        }
    }
}
