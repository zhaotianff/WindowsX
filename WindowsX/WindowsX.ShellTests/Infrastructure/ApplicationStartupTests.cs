using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WindowsX.Shell.Infrastructure;
using WindowsX.Shell.View.Setting.Pages;

namespace WindowsX.ShellTests.Infrastructure
{
    [TestClass]
    [DoNotParallelize]
    public class ApplicationStartupTests
    {
        [TestMethod]
        public void RegistrationQuotesPathAndOnlyRemovesOwnValue()
        {
            var path = @"Software\WindowsX.Tests\" + Guid.NewGuid();
            try
            {
                var service = new ApplicationStartupService(@"D:\Apps With Spaces\中文\WindowsX.Shell.exe", path);
                Assert.IsFalse(service.IsEnabled());
                service.SetEnabled(false);
                service.SetEnabled(true);
                service.SetEnabled(true);
                Assert.IsTrue(service.IsEnabled());
                using (var key = Registry.CurrentUser.OpenSubKey(path, true)!)
                {
                    Assert.AreEqual("\"D:\\Apps With Spaces\\中文\\WindowsX.Shell.exe\" --autostart", key.GetValue(ApplicationStartupService.ValueName));
                    key.SetValue("OtherApp", "unchanged");
                }
                service.SetEnabled(false);
                service.SetEnabled(false);
                Assert.IsFalse(service.IsEnabled());
                using var remaining = Registry.CurrentUser.OpenSubKey(path)!;
                Assert.AreEqual("unchanged", remaining.GetValue("OtherApp"));
            }
            finally { Registry.CurrentUser.DeleteSubKeyTree(path, false); }
        }

        [DataTestMethod]
        [DataRow("")]
        [DataRow("relative.exe")]
        [DataRow("D:\\invalid\"path.exe")]
        public void InvalidExecutablePathIsRejected(string path)
        {
            Assert.ThrowsException<ArgumentException>(() => new ApplicationStartupService(path, @"Software\WindowsX.Tests"));
        }

        [TestMethod]
        public void PageLoadsWithoutWritingAndTogglesImmediately()
        {
            RunSta(() =>
            {
                var service = new FakeStartupService { Enabled = true };
                var page = new SystemSetting(service);
                page.Load();
                var check = (CheckBox)page.FindName("check_AutoStart");
                Assert.AreEqual(true, check.IsChecked);
                Assert.AreEqual(0, service.WriteCount);
                check.IsChecked = false;
                Assert.IsFalse(service.Enabled);
                Assert.AreEqual(1, service.WriteCount);
                check.IsChecked = true;
                Assert.IsTrue(service.Enabled);
                Assert.AreEqual(2, service.WriteCount);
                service.Enabled = false;
                page.Load();
                Assert.AreEqual(false, check.IsChecked);
                Assert.AreEqual(2, service.WriteCount);
            });
        }

        [TestMethod]
        public void PageRestoresToggleOnWriteFailureAndCanRetryRead()
        {
            RunSta(() =>
            {
                var service = new FakeStartupService();
                var page = new SystemSetting(service);
                page.Load();
                var check = (CheckBox)page.FindName("check_AutoStart");
                var error = (TextBlock)page.FindName("text_Error");
                service.FailWrite = true;
                check.IsChecked = true;
                Assert.AreEqual(false, check.IsChecked);
                Assert.AreEqual(Visibility.Visible, error.Visibility);
                Assert.AreEqual(1, service.WriteCount);
                service.FailRead = true;
                page.Load();
                Assert.IsFalse(check.IsEnabled);
                service.FailRead = false;
                page.Load();
                Assert.IsTrue(check.IsEnabled);
                Assert.AreEqual(Visibility.Collapsed, error.Visibility);
            });
        }

        private sealed class FakeStartupService : IApplicationStartupService
        {
            public bool Enabled;
            public bool FailRead;
            public bool FailWrite;
            public int WriteCount;
            public bool IsEnabled()
            {
                if (FailRead) throw new UnauthorizedAccessException("读取被拒绝");
                return Enabled;
            }
            public void SetEnabled(bool enabled)
            {
                WriteCount++;
                if (FailWrite) throw new UnauthorizedAccessException("写入被拒绝");
                Enabled = enabled;
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
