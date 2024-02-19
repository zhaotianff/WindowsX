using Master_Zhao.Shell.Controls;
using Master_Zhao.Shell.Model.SystemMgmt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace Master_Zhao.Shell.View.SystemMgmt.UserControls
{
    /// <summary>
    /// DiskDetailControl.xaml 的交互逻辑
    /// </summary>
    public partial class DiskDetailControl : UserControl
    {
        public DiskDetailControl(System.IO.DriveInfo driver,DiskPath diskPath)
        {
            InitializeComponent();

            var volumnLable = string.IsNullOrEmpty(driver.VolumeLabel) ? "本地磁盘" : driver.VolumeLabel;
            this.lbl_Name.Content = volumnLable + "  " + driver.Name;
            float freeSizeGB = driver.TotalFreeSpace / 1024 / 1024 / 1024;
            float totalSizeGB = driver.TotalSize / 1024 / 1024 / 1024;
            percentageBar.Text = $"{freeSizeGB}GB / {totalSizeGB}GB";
            var percentage = ((totalSizeGB - freeSizeGB) / totalSizeGB) * 100;
            percentageBar.Value = percentage;
            if (percentage >= 75)
                percentageBar.Fill = Brushes.Red;
            lbl_DirCount.Content = diskPath.SubDirCount.ToString();
            lbl_FileCount.Content = diskPath.FileCount.ToString();
            img_Icon.Source = diskPath.Icon;
        }

    }
}
