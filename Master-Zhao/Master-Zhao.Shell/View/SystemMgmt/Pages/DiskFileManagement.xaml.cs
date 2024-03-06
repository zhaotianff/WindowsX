using Master_Zhao.Shell.Controls;
using Master_Zhao.Shell.Model.SystemMgmt;
using Master_Zhao.Shell.PInvoke;
using Master_Zhao.Shell.Util;
using Master_Zhao.Shell.View.Common.MessageBoxEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        private Dictionary<string, FileAssocItem> assocDic;
        private bool isLoaded = false;
        private ImageSource defaultIcon;

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
                await Task.Run(() => { Kernel32.EnumerateSubDirectory(disk.Path, disk.Children,isEnumOnce:true); });
                computer.Children.Add(disk);
            }

            await Task.WhenAll(taskList);
            uc_Loading.SetStatusText("目录信息已经加载完成");
            isLoaded = true;
            uc_Loading.Visibility = Visibility.Collapsed;
            this.tree.IsEnabled = true;
        }

        private void tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var diskPath = this.tree.SelectedItem as DiskPath;

            if(diskPath.DiskPathType == DiskPathType.Computer)
            {
                DisplayAllDiskStatistics(diskPath);
            }
            else if(diskPath.DiskPathType == DiskPathType.Disk)
            {
                DisplayDiskStatistics(diskPath);
            }
            else
            {
                DisplayFolderStatistics(diskPath);
            }
        }

        private void DisplayAllDiskStatistics(DiskPath diskPath)
        {
            this.stack_Statistics.Children.Clear();

            System.IO.DriveInfo[] driveInfos = System.IO.DriveInfo.GetDrives();

            for (int i = 0; i < driveInfos.Length; i++)
            {
                var driver = driveInfos[i];
                var subDiskPath = diskPath.Children[i];
                UserControls.DiskDetailControl diskDetailControl = new UserControls.DiskDetailControl(driver, subDiskPath);
                this.stack_Statistics.Children.Add(diskDetailControl);
            }
        }

        private void DisplayDiskStatistics(DiskPath diskPath)
        {
            var driver = System.IO.DriveInfo.GetDrives().FirstOrDefault(x => x.Name == diskPath.DisplayName);

            if(driver != null)
            {
                this.stack_Statistics.Children.Clear();
                UserControls.DiskDetailControl diskDetailControl = new UserControls.DiskDetailControl(driver, diskPath);
                this.stack_Statistics.Children.Add(diskDetailControl);
            }
        }


        private void DisplayFolderStatistics(DiskPath diskPath)
        {
            this.stack_Statistics.Children.Clear();

            var driverInfos = System.IO.DriveInfo.GetDrives();
            System.IO.DriveInfo driveInfo = null;
            for (int i = 0; i < driverInfos.Length; i++)
            {
                if (diskPath.Path.Contains(driverInfos[i].Name))
                {
                    driveInfo = driverInfos[i];
                    break;
                }
            }

            UserControls.FolderDetailControl folderDetailControl = new UserControls.FolderDetailControl(diskPath, driveInfo);
            this.stack_Statistics.Children.Add(folderDetailControl);
        }

        private void tree_Expanded(object sender, RoutedEventArgs e)
        {
            var diskPath = (e.OriginalSource as TreeViewItem).Header as DiskPath;

            if (diskPath == null)
                return;

            foreach (var subDiskPath in diskPath.Children)
            {
                if (subDiskPath.Children == null)
                {
                    subDiskPath.Children = new System.Collections.ObjectModel.ObservableCollection<DiskPath>();
                    Kernel32.EnumerateSubDirectory(subDiskPath.Path, subDiskPath.Children, isEnumOnce: true);
                }
            }
        }

        private async void btn_Statistics_Click(object sender, RoutedEventArgs e)
        {
            if (this.tree.SelectedItem == null)
                return;

            var diskPath = this.tree.SelectedItem as DiskPath;

            //not support computer temporarily 
            if (diskPath.Path == "Root")
                return;

            WaitingDialog waitingDialog = new WaitingDialog(Application.Current.MainWindow);
            waitingDialog.Show();

            var statisticsDiskPath = new DiskPath();
            statisticsDiskPath.Children = new System.Collections.ObjectModel.ObservableCollection<DiskPath>();
            statisticsDiskPath.DiskPathType = DiskPathType.Folder;
            statisticsDiskPath.Icon = diskPath.Icon;
            statisticsDiskPath.Path = diskPath.Path;
            statisticsDiskPath.DisplayName = diskPath.DisplayName;

            await Task.Factory.StartNew(() => {
                Kernel32.EnumerateSubDirectoryEx(statisticsDiskPath.Path, statisticsDiskPath.Children, true);
            },TaskCreationOptions.LongRunning);
            DiskPath.CurrentRootSize = GetRootSize(statisticsDiskPath);
            statisticsDiskPath.Size = DiskPath.CurrentRootSize;

            this.tree_Statistics.ItemsSource = new List<DiskPath>() { statisticsDiskPath };
            waitingDialog.Close();
        }

        private long GetRootSize(DiskPath diskPath)
        {
            if (diskPath.Children.Count == 0)
                return 0;

            long size = 0;
            foreach (var subDiskPath in diskPath.Children)
            {
                size += subDiskPath.Size;
            }

            return size;
        }

        private async void btn_FileAssoc_Click(object sender, RoutedEventArgs e)
        {
            if (this.tree.SelectedItem == null)
                return;

            var diskPath = this.tree.SelectedItem as DiskPath;

            if (diskPath.Path == "Root")
                return;

            WaitingDialog waitingDialog = new WaitingDialog(Application.Current.MainWindow);
            waitingDialog.Show();

            var statisticsDiskPath = new DiskPath();
            statisticsDiskPath.Children = new System.Collections.ObjectModel.ObservableCollection<DiskPath>();
            statisticsDiskPath.DiskPathType = DiskPathType.Folder;
            statisticsDiskPath.Icon = diskPath.Icon;
            statisticsDiskPath.Path = diskPath.Path;
            statisticsDiskPath.DisplayName = diskPath.DisplayName;

            await Task.Factory.StartNew(() => {
                Kernel32.EnumerateSubDirectoryEx(statisticsDiskPath.Path, statisticsDiskPath.Children, true);
            }, TaskCreationOptions.LongRunning);
            DiskPath.CurrentRootSize = GetRootSize(statisticsDiskPath);
            statisticsDiskPath.Size = DiskPath.CurrentRootSize;

            assocDic = new Dictionary<string, FileAssocItem>();
            GetFileAssoc(statisticsDiskPath, assocDic);
            CalcExtensionPercentage(assocDic);

            this.lst_FileAssoc.ItemsSource = null;
            this.lst_FileAssoc.ItemsSource = assocDic.Select(x => x.Value);

            waitingDialog.Close();
        }

        private void GetFileAssoc(DiskPath diskPath, Dictionary<string,FileAssocItem> assocDic)
        {
            if (diskPath.Children == null)
                return;

            foreach (var file in diskPath.Children)
            {
                if(file.DiskPathType == DiskPathType.File)
                {
                    var fileExtension = System.IO.Path.GetExtension(file.Path);

                    if (string.IsNullOrEmpty(fileExtension))
                        continue;

                    FileAssocItem fileAssocItem;
                    if(assocDic.ContainsKey(fileExtension))
                    {
                        fileAssocItem = assocDic[fileExtension];
                        fileAssocItem.Count += 1;
                        fileAssocItem.AllFiles.Add(file.Path);
                    }
                    else
                    {
                        fileAssocItem = new FileAssocItem();
                        fileAssocItem.Extension = fileExtension;
                        fileAssocItem.Count = 1;
                        var friendlyName = GetExtensionFriendlyName(fileExtension);
                        var executablePath = GetExtensionExecutablePath(fileExtension);
                        fileAssocItem.Executable = executablePath;
                        fileAssocItem.FriendlyName = $"{fileExtension} ({friendlyName})";
                        fileAssocItem.Icon = GetExtensionIcon(fileExtension);
                        fileAssocItem.AllFiles.Add(file.Path);
                        assocDic[fileExtension] = fileAssocItem;
                    }
                }

                if(file.DiskPathType == DiskPathType.Folder)
                {
                    GetFileAssoc(file, assocDic);
                }
            }
        }

        private ImageSource GetExtensionIcon(string fileExtension)
        {
            IntPtr hIcon = IntPtr.Zero;
            IconTool.GetFileExtensionAssocIcon(fileExtension, ref hIcon);
            if (hIcon != IntPtr.Zero)
            {
                ImageSource imageSource = ImageHelper.GetBitmapImageFromHIcon(hIcon);
                IconTool.DestroyIcon(hIcon);
                return imageSource;
            }
            else
            {
                if (defaultIcon == null)
                {
                    IconTool.GetShell32Icon(0, ref hIcon);
                    defaultIcon = ImageHelper.GetBitmapImageFromHIcon(hIcon);
                    IconTool.DestroyIcon(hIcon);
                }

                return defaultIcon;    
            }
        }

        private string GetExtensionFriendlyName(string fileExtension)
        {
            IntPtr sb = Marshal.AllocHGlobal(120);
            DesktopTool.GetFileExtensionFriendlyName(fileExtension, sb, 120);
            if(sb == IntPtr.Zero)
            {
                Marshal.FreeHGlobal(sb);
                return "";
            }
            var extension =  Marshal.PtrToStringAuto(sb);
            Marshal.FreeHGlobal(sb);
            return extension;
        }

        private string GetExtensionExecutablePath(string fileExtension)
        {
            IntPtr sb = Marshal.AllocHGlobal(DesktopTool.MAX_PATH);
            DesktopTool.GetFileExtensionExecutablePath(fileExtension, sb, (uint)DesktopTool.MAX_PATH);
            if (sb == IntPtr.Zero)
            {
                Marshal.FreeHGlobal(sb);
                return "";
            }
            var extension = Marshal.PtrToStringAuto(sb);
            Marshal.FreeHGlobal(sb);
            return extension;
        }

        private void CalcExtensionPercentage(Dictionary<string, FileAssocItem> assocDic)
        {
            float fileCount = 0;
            assocDic.Values.ToList().ForEach(x => fileCount += x.Count);
            assocDic.Values.ToList().ForEach(x => { x.Percentage = (float)(x.Count / fileCount) * 100;x.PercentageText = x.Percentage.ToString("0.00") + $"% ({x.Count}个文件)"; });
        }

        private void menu_OpenDirLocation_Click(object sender, RoutedEventArgs e)
        {
            if (this.tree.SelectedItem == null)
                return;

            var diskPath = this.tree.SelectedItem as DiskPath;

            if(!string.IsNullOrEmpty(diskPath.Path))
            {
                System.Diagnostics.Process.Start("explorer", "/select, " + diskPath.Path.Replace("\\\\","\\"));
            }
        }

        private void btn_ExtensionCountDesc_Click(object sender, RoutedEventArgs e)
        {
            if (assocDic == null)
                return;

            this.lst_FileAssoc.ItemsSource = assocDic.OrderByDescending(x=>x.Value.Count).Select(x=>x.Value);
        }

        private void btn_ExtensionCountAsc_Click(object sender, RoutedEventArgs e)
        {
            if (assocDic == null)
                return;

            this.lst_FileAssoc.ItemsSource = assocDic.OrderBy(x => x.Value.Count).Select(x => x.Value);
        }

        private void btn_ExtensionCountDefault_Click(object sender, RoutedEventArgs e)
        {
            if (assocDic == null)
                return;

            this.lst_FileAssoc.ItemsSource = assocDic.Select(x => x.Value);
        }
    }
}
