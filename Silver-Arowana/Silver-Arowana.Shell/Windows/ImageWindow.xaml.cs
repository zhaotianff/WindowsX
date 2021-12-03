using Silver_Arowana.Config;
using Silver_Arowana.Shell.MessageBoxEx;
using Silver_Arowana.Shell.PInvoke;
using Silver_Arowana.Shell.Util;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using static Silver_Arowana.Shell.PInvoke.DesktopTool;

namespace Silver_Arowana.Shell.Windows
{
    /// <summary>
    /// ImageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ImageWindow : TianXiaTech.BlurWindow
    {
        private string previousBackground = "";
        private bool isInternetImage = false;

        public ImageWindow()
        {
            InitializeComponent();
        }

        public void SetImageUrl(string url,string previousBackground)
        {       
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(url);
            bi.EndInit();
            image.Source = bi;

            this.previousBackground = previousBackground;
            isInternetImage = true;
        }

        public void SetLocalImage(string path,string previousBackground)
        {
            if (System.IO.File.Exists(path) == false)
                return;

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            var buffer = System.IO.File.ReadAllBytes(path);
            System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer);
            bi.StreamSource = ms;
            bi.EndInit();
            image.Source = bi;

            this.previousBackground = previousBackground;
            isInternetImage = false;
        }

        private async void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            if (image.Source == null)
                return;

            SetWallpaper();
            var keepWallpaper = await WaitMessageBox.Show("提示信息","是否保存当前壁纸设置？","保留");
            SetKeepWallpaper(keepWallpaper);
        }

        private void btnSet_Click(object sender, RoutedEventArgs e)
        {
            SetWallpaper();
        }

        private void SetWallpaper()
        {
            var filePath = SaveImageControlAsFile();
            StringBuilder sb = new StringBuilder(filePath);
            SetBackground(sb);
            SwitchToDesktop();
        }

        private void SetKeepWallpaper(bool isKeep)
        {
            if (isKeep == false)
            {
                SwitchToWindow(new WindowInteropHelper(this).Handle);
                StringBuilder sb = new StringBuilder(previousBackground);
                SetBackground(sb);         
            }
        }

        private string SaveImageControlAsFile()
        {
            var filePath = System.IO.Path.Combine(GlobalConfig.Instance.StaticWallpaperConfig.DownloadPath,
               $"{Guid.NewGuid()}.jpg");
            UIElementHelper.SaveImageSourceAsFile((BitmapSource)image.Source, filePath);
            return filePath;
        }
    }
}
