using Master_Zhao.Shell.Controls;
using Master_Zhao.Shell.Model.SystemMgmt;
using Master_Zhao.Shell.PInvoke;
using Master_Zhao.Shell.Util;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Master_Zhao.Shell.View.SystemMgmt.Pages
{
    /// <summary>
    /// StartupManagement.xaml 的交互逻辑
    /// </summary>
    public partial class StartupManagement : Page
    {
        List<StartupItem> startupItemList;
        List<StartupItem> startupItemDisableList;

        public StartupManagement()
        {
            InitializeComponent();
        }

        private void LoadStartUpItem()
        {
            var startupItemList = LoadRegistryStartupItem();
            this.listbox.ItemsSource = startupItemList;

            startupItemDisableList = LoadRegistryStartupDisableItem();
        }

        private List<StartupItem> LoadRegistryStartupItem()
        {
            var startupItemList = new List<StartupItem>();
            //temp 10
            int count = 10;
            var itemSize = Marshal.SizeOf<tagSTARTUPITEM>();
            int size = count * itemSize;
            IntPtr buffer = Marshal.AllocHGlobal(size);
            var result = PInvoke.StartupTool.GetStartupItems(buffer, size, ref count);

            if (result == false)
                return startupItemList;

            for (int i = 0; i < count; i++)
            {
                byte[] startupItemBuffer = new byte[itemSize];
                Marshal.Copy(buffer, startupItemBuffer, 0, itemSize);

                IntPtr newPtr = Marshal.AllocHGlobal(itemSize);
                Marshal.Copy(startupItemBuffer, 0, newPtr, itemSize);

                var tagStartupItem = Marshal.PtrToStructure<tagSTARTUPITEM>(newPtr);

                buffer = new IntPtr(buffer.ToInt64() + itemSize);

                StartupItem startupItem = new StartupItem();
                startupItem.Name = tagStartupItem.szName;

                if (string.IsNullOrEmpty(tagStartupItem.szPath))
                    continue;

                startupItem.HKey = tagStartupItem.hKey;
                startupItem.RegPath = tagStartupItem.szRegPath;
                startupItem.SamDesired = tagStartupItem.samDesired;
                startupItem.Path = FixPath(tagStartupItem.szPath);
                startupItem.Description = tagStartupItem.szDescription;
                startupItem.IsEnabled = tagStartupItem.bEnabled;
                startupItem.StartupItemType = (StartupItemType)tagStartupItem.type;
                startupItem.Icon = ImageHelper.GetBitmapImageFromLocalFile(IconHelper.GetCachedIconPath(startupItem.Path));
                startupItemList.Add(startupItem);
            }

            return startupItemList;
        }

        private string FixPath(string path)
        {
            if (path.Contains("\""))
            {
                var startIndex = path.IndexOf("\"") + 1;
                var lastIndex = path.LastIndexOf("\"");
                return path.Substring(startIndex, lastIndex - startIndex);
            }

            return path;
        }

        private List<StartupItem> LoadRegistryStartupDisableItem()
        {
            return new List<StartupItem>();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStartUpItem();
        }

        private void openProperty_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = this.listbox.SelectedItem as StartupItem;

            if (selectedItem != null)
            {
                DesktopTool.OpenFileProperty(selectedItem.Path);
            }
        }

        private void openFilePath_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = this.listbox.SelectedItem as StartupItem;

            if(selectedItem != null)
            {
                DesktopTool.SelectFile(selectedItem.Path);
            }
        }

        private void enable_Checked(object sender, RoutedEventArgs e)
        {
            var selectedItem = TreeHelper.FindParent<ListBoxItem>(sender as ToggleSwitch);

            if(selectedItem != null)
            {
                var startupItem = selectedItem.Content as StartupItem;
                EnableStartupItem(startupItem);
            }
        }

        private void disable_Checked(object sender, RoutedEventArgs e)
        {
            var selectedItem = TreeHelper.FindParent<ListBoxItem>(sender as ToggleSwitch);

            if (selectedItem != null)
            {
                var startupItem = selectedItem.Content as StartupItem;
                DisableStartupItem(startupItem);
            }
        }

        private async void EnableStartupItem(StartupItem startupItem)
        {
            if (startupItem == null)
                return;

            if(startupItem.StartupItemType == StartupItemType.Registry)
            {
                await ProcessHelper.ExecuteAdminTask(new string[] { "startup", "-enable", ((uint)startupItem.HKey).ToString(), startupItem.RegPath, startupItem.SamDesired.ToString(), startupItem.Name, startupItem.Path });
            }
            else if(startupItem.StartupItemType == StartupItemType.ShellStartup)
            {
                StartupTool.EnableShellStartupItem(startupItem.Name, startupItem.Path);
            }
            
            LoadStartUpItem();
        }

        private async void DisableStartupItem(StartupItem startupItem)
        {
            if (startupItem == null)
                return;

            if(startupItem.StartupItemType == StartupItemType.Registry)
            {
                await ProcessHelper.ExecuteAdminTask(new string[] { "startup", "-disable", ((uint)startupItem.HKey).ToString(), startupItem.RegPath, startupItem.SamDesired.ToString(), startupItem.Name, startupItem.Path });
            }
            else if(startupItem.StartupItemType == StartupItemType.ShellStartup)
            {
                StartupTool.DisableShellStartupItem(startupItem.Name, startupItem.Path);
            }
            LoadStartUpItem();
        }
    }
}
