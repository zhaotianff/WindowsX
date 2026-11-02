using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using System.Windows.Controls;
using WindowsX.Shell.Model.SystemMgmt;
using WindowsX.Shell.View.SystemMgmt.UserControls;

namespace WindowsX.ShellTests.Services
{
    [TestClass]
    public class SystemInfoItemControlTests
    {
        [TestMethod]
        public void RefreshReusesSelectorAndSupportsDeviceSwitching()
        {
            RunSta(() =>
            {
                var control = new SystemInfoItemControl { Title = "设备" };
                var information = new SystemInformation
                {
                    SystemInformationTypeList = new() { "设备 A", "设备 B" },
                    SystemInformationKeyValueList = new()
                    {
                        new() { ["型号"] = "A" }, new() { ["型号"] = "B" }
                    }
                };
                for (var i = 0; i < 3; i++)
                    control.SystemInformation = information;
                var dock = (DockPanel)control.FindName("dock");
                Assert.AreEqual(1, dock.Children.OfType<ComboBox>().Count());
                var selector = (ComboBox)control.FindName("cbx_TypeList");
                Assert.AreEqual(2, selector.Items.Count);
                selector.SelectedIndex = 1;
                var stack = (StackPanel)control.FindName("stack");
                var row = (Grid)stack.Children[1];
                Assert.AreEqual("B", row.Children.OfType<TextBox>().Single().Text);
            });
        }

        [TestMethod]
        public void EmptyResultsClearPreviousValuesAndHideSelector()
        {
            RunSta(() =>
            {
                var control = new SystemInfoItemControl();
                control.SystemInformation = new SystemInformation
                {
                    SystemInformationKeyValueList = new() { new() { ["型号"] = "A" }, new() { ["型号"] = "B" } }
                };
                control.SystemInformation = null!;
                var selector = (ComboBox)control.FindName("cbx_TypeList");
                Assert.AreEqual(Visibility.Collapsed, selector.Visibility);
                Assert.AreEqual(0, selector.Items.Count);
                var stack = (StackPanel)control.FindName("stack");
                Assert.AreEqual(2, stack.Children.Count);
                Assert.AreEqual("暂无信息", ((TextBlock)stack.Children[1]).Text);
            });
        }

        [TestMethod]
        public void MissingDeviceNamesReceiveSafeFallbackLabels()
        {
            RunSta(() =>
            {
                var control = new SystemInfoItemControl { Title = "内存" };
                control.SystemInformation = new SystemInformation
                {
                    SystemInformationTypeList = new() { "DIMM A" },
                    SystemInformationKeyValueList = new() { new() { ["容量"] = "8 GB" }, new() { ["容量"] = "16 GB" } }
                };
                var selector = (ComboBox)control.FindName("cbx_TypeList");
                Assert.AreEqual("内存2", selector.Items[1]);
                selector.SelectedIndex = -1;
            });
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
