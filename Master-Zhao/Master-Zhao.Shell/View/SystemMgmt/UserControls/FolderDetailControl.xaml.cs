using Master_Zhao.Shell.Model.SystemMgmt;
using Master_Zhao.Shell.Util;
using System;
using System.Collections.Generic;
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
    /// FolderDetailControl.xaml 的交互逻辑
    /// </summary>
    public partial class FolderDetailControl : UserControl
    {
        public FolderDetailControl(DiskPath diskPath,System.IO.DriveInfo driveInfo)
        {
            InitializeComponent();

            this.img_Icon.Source = diskPath.Icon;
            this.lbl_Name.Content = diskPath.DisplayName;
            this.lbl_FullPath.Content = diskPath.Path;
            this.lbl_Size.Content = FileHelper.GetSizeString(diskPath.Size);
            this.percentageBar.Value = (float)((double)diskPath.Size / (double)driveInfo.TotalSize) * 100;
            this.percentageBar.Text = this.percentageBar.Value.ToString("0.0") + " %";
            this.lbl_DirCount.Content = diskPath.SubDirCount.ToString();
            this.lbl_FileCount.Content = diskPath.FileCount.ToString();
            this.lbl_CreateTime.Content = diskPath.CreationTime.ToString();
            this.lbl_LastAccessTime.Content = diskPath.LastAccessTime.ToString();
            this.lbl_LastWriteTime.Content = diskPath.LastWriteTime.ToString();
        }
    }
}
