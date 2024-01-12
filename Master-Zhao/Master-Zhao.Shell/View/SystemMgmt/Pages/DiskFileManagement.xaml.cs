using Master_Zhao.Shell.Model.SystemMgmt;
using Master_Zhao.Shell.PInvoke;
using Master_Zhao.Shell.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
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
    /// DiskFileManagement.xaml 的交互逻辑
    /// </summary>
    public partial class DiskFileManagement : Page
    {
        private List<DiskPath> root = new List<DiskPath>();
        private bool isLoaded = false;

        public DiskFileManagement()
        {
            InitializeComponent();
        }

        public async void LoadDiskFileTree()
        {
            if (isLoaded)
            {
                lbl_LoadingStatus.Content = "目录信息已经加载完成";
                return;
            }

            lbl_LoadingStatus.Content = "正在加载目录信息";
            var computer = new DiskPath() { DisplayName = "我的电脑", Path = "Root", DiskPathType = DiskPathType.Computer };
            root.Add(computer);
            this.tree.ItemsSource = root;

            computer.Children = new System.Collections.ObjectModel.ObservableCollection<DiskPath>();
            var driveInfos = System.IO.DriveInfo.GetDrives();

            List<Task> taskList = new List<Task>();

            foreach (var drive in driveInfos)
            {
                var disk = new DiskPath() { DisplayName = drive.Name, Path = drive.RootDirectory.FullName, DiskPathType = DiskPathType.Disk };
                disk.Children = new System.Collections.ObjectModel.ObservableCollection<DiskPath>();
                await Task.Run(() => { Kernel32.EnumerateSubDirectory(disk.Path, disk.Children); });
                computer.Children.Add(disk);
            }

            await Task.WhenAll(taskList);
            lbl_LoadingStatus.Content = "目录信息已经加载完成";
            isLoaded = true;
        }

        private void tree_Expanded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
