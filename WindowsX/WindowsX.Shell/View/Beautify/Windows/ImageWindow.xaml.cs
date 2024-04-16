using WindowsX.Config;
using WindowsX.Shell.MessageBoxEx;
using WindowsX.Shell.PInvoke;
using WindowsX.Shell.Util;
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

using static WindowsX.Shell.PInvoke.DesktopTool;

namespace WindowsX.Shell.Windows
{
    /// <summary>
    /// ImageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ImageWindow : TianXiaTech.BlurWindow
    {
        private string previousBackground = "";
        private bool isInternetImage = false;
        private string currentBackground = "";

        private Action refreshBackgroundListCallBack;

        public ImageWindow(Action action)
        {
            InitializeComponent();
            refreshBackgroundListCallBack = action;
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
            this.Title = $"预览 {url}";
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
            this.currentBackground = path;
            isInternetImage = false;
            this.Title = $"预览 {path}";
        }

        private async void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            if (image.Source == null)
                return;

            SetWallpaper();
            SwitchToDesktop();
            SetAllWindowState(WindowState.Minimized);
            var keepWallpaper = await WaitMessageBox.Show("提示信息","是否保存当前壁纸设置？","保留");
            SetKeepWallpaper(keepWallpaper);
            SetAllWindowState(WindowState.Normal);
        }

        private void btnSet_Click(object sender, RoutedEventArgs e)
        {
            SetWallpaper();
            refreshBackgroundListCallBack?.Invoke();
        }

        private void MenuCopyPath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Clipboard.SetText(Title.Replace("预览 ", ""));
        }

        private void SetWallpaper()
        {
            var filePath = "";
            if (isInternetImage)
            {
                filePath = SaveImageControlAsFile();
            }
            else
            {
                filePath = currentBackground;
            }
            StringBuilder sb = new StringBuilder(filePath);
            SetBackground(sb);         
        }

        private void SetAllWindowState(WindowState windowState)
        {
            Application.Current.MainWindow.WindowState = windowState;
            foreach (var item in Application.Current.MainWindow.OwnedWindows)
            {
                (item as Window).WindowState = windowState;
            }
        }

        private void SetKeepWallpaper(bool isKeep)
        {
            if (isKeep == false)
            {
                SwitchToWindow(new WindowInteropHelper(this).Handle);
                StringBuilder sb = new StringBuilder(previousBackground);
                SetBackground(sb);         
            }
            else
            {
                refreshBackgroundListCallBack?.Invoke();
            }
        }

        private string SaveImageControlAsFile()
        {
            var downloadPath = GlobalConfig.Instance.StaticWallpaperConfig.DownloadPath;
            if (System.IO.Directory.Exists(downloadPath) == false)
                System.IO.Directory.CreateDirectory(downloadPath);

            var filePath = System.IO.Path.Combine(GlobalConfig.Instance.StaticWallpaperConfig.DownloadPath,
               $"{Guid.NewGuid()}.jpg");
            ImageHelper.SaveImageSourceAsFile((BitmapSource)image.Source, filePath);
            return filePath;
        }
    }
}
