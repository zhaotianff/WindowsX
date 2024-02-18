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
                uc_Loading.SetStatusText("目录信息已经加载完成");
                return;
            }

            uc_Loading.Visibility = Visibility.Visible;
            this.tree.IsEnabled = false;
            uc_Loading.SetStatusText("正在加载目录信息");
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
            uc_Loading.SetStatusText("目录信息已经加载完成");
            isLoaded = true;

            await Task.Delay(1000);
            uc_Loading.Visibility = Visibility.Collapsed;
            this.tree.IsEnabled = true;
        }

        private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var diskPath = this.tree.SelectedItem as DiskPath;

            if(diskPath.DiskPathType == DiskPathType.Computer)
            {
                DisplayAllDiskStatistics();
            }
            else
            {
                DisplayFolderStatistics(diskPath);
            }
        }

        private void DisplayAllDiskStatistics()
        {
            this.stack_Statistics.Children.Clear();

            System.IO.DriveInfo[] driveInfos = System.IO.DriveInfo.GetDrives();

            foreach (var driver in driveInfos)
            {
                Label label = new Label();
                label.FontSize = 15;
                label.FontWeight = FontWeights.Bold;
                label.Foreground = GlobalData.Instance.AccentColorBrush;
                var volumnLable = string.IsNullOrEmpty(driver.VolumeLabel) ? "本地磁盘" : driver.VolumeLabel;
                label.Content = volumnLable  + "  " + driver.Name;
                label.Margin = new Thickness(5,5,10,0);
                this.stack_Statistics.Children.Add(label);
                Controls.PercentageBar percentageBar = new Controls.PercentageBar();
                percentageBar.Width = 300;
                percentageBar.Height = 20;
                percentageBar.HorizontalAlignment = HorizontalAlignment.Left;
                float freeSizeGB = driver.TotalFreeSpace / 1024 / 1024 / 1024;
                float totalSizeGB = driver.TotalSize / 1024 / 1024 / 1024;
                percentageBar.Text = $"{freeSizeGB}GB / {totalSizeGB}GB";
                var percentage = (( totalSizeGB - freeSizeGB) / totalSizeGB) * 100;
                percentageBar.Value = percentage;
                if (percentage >= 75)
                    percentageBar.Fill = Brushes.Red;
                percentageBar.Margin = new Thickness(10);
                this.stack_Statistics.Children.Add(percentageBar);
            }
        }

        private void DisplayFolderStatistics(DiskPath diskPath)
        {

        }
    }
}
