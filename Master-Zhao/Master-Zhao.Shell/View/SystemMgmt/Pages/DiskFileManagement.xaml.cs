using Master_Zhao.Shell.Model.SystemMgmt;
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
                return;

            var computer = new DiskPath() { DisplayName = "我的电脑", Path = "Root", DiskPathType = DiskPathType.Computer };
            root.Add(computer);
            this.tree.ItemsSource = root;

            computer.Children = new System.Collections.ObjectModel.ObservableCollection<DiskPath>();

            await Task.Run(() => {
                var driveInfos = System.IO.DriveInfo.GetDrives();

                foreach (var drive in driveInfos)
                {
                    //Use task.whenall
                    var disk = new DiskPath() { DisplayName = drive.Name, Path = drive.RootDirectory.FullName, DiskPathType = DiskPathType.Disk };
                    this.Dispatcher.Invoke(() => {
                        computer.Children.Add(disk);
                    }); 
                    AppendFolder(disk);
                }

                isLoaded = true;
            });
        }

        private void AppendFolder(DiskPath diskPath)
        {
            System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(diskPath.Path);

            if (directoryInfo.CanAccess() == false || (directoryInfo.Attributes.HasFlag(FileAttributes.Hidden) && diskPath.DiskPathType != DiskPathType.Disk))
                return;

            DirectoryInfo[] subDirs = new DirectoryInfo[] { };

            try
            {
                subDirs = directoryInfo.GetDirectories();
            }
            catch
            {
                return;
            }

            if (subDirs.Length > 0)
                diskPath.Children = new System.Collections.ObjectModel.ObservableCollection<DiskPath>();


            foreach (var dir in subDirs)
            {
                var folder = new DiskPath() { DisplayName = dir.Name, Path = dir.FullName, DiskPathType = DiskPathType.Folder };
                this.Dispatcher.Invoke(() => {
                    diskPath.Children.Add(folder);
                });
                AppendFolder(folder);
            }

        }

        private void tree_Expanded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
