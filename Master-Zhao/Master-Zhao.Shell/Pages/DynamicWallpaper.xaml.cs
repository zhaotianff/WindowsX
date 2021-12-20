using Master_Zhao.Config;
using Master_Zhao.Config.Model;
using Master_Zhao.IO;
using Master_Zhao.Shell.PInvoke;
using Master_Zhao.Shell.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
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

using static Master_Zhao.IO.Commands.DynamicWallpaperCommands;

namespace Master_Zhao.Shell.Pages
{
    /// <summary>
    /// DynamicWallpaper.xaml 的交互逻辑
    /// </summary>
    public partial class DynamicWallpaper : Page
    {
        private static readonly string DynamicDesktopProgramFileName = "Master-Zhao.DynamicDesktop.exe";
        private static readonly string DynamicDesktopProcessName = "Master-Zhao.DynamicDesktop";

        private AnonymousPipeServer server = new AnonymousPipeServer();

        public DynamicWallpaper()
        {
            InitializeComponent();
        }

        private async void LoadDynamicWallpaperListAsync()
        {
            var list = GlobalConfig.Instance.DynamicWallpaperConfig.WallpaperList;

            wrap.Children.Clear();

            await Task.Factory.StartNew(()=> {
                foreach (var item in list)
                {
                    DynamicWallpaperControl dynamicWallpaperControl = new DynamicWallpaperControl();
                    dynamicWallpaperControl.WallpaperName = item.Name;
                    dynamicWallpaperControl.VideoPath = System.IO.Path.GetFullPath(item.Path);
                    if(!string.IsNullOrEmpty(item.Thumbnail))
                        dynamicWallpaperControl.ThumbnailPath = System.IO.Path.GetFullPath(item.Thumbnail);
                    dynamicWallpaperControl.OnPreview += DynamicWallpaperControl_OnPreview;
                    dynamicWallpaperControl.OnSet += DynamicWallpaperControl_OnSet;
                    dynamicWallpaperControl.OnSelect += DynamicWallpaperControl_OnSelect;
                    wrap.Children.Add(dynamicWallpaperControl);
                }
            },new System.Threading.CancellationToken(),TaskCreationOptions.None,TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void DynamicWallpaperControl_OnSelect(object sender, EventArgs e)
        {
            foreach (var item in wrap.Children)
            {
                var dynamicWallpaperControl = item as DynamicWallpaperControl;
                dynamicWallpaperControl?.ResetBorderBrush();
            }
        }

        private void DynamicWallpaperControl_OnSet(object sender, string path)
        {
            
        }


        private async void DynamicWallpaperControl_OnPreview(object sender, string path)
        {
            //TODO 可以优化
            TerminateDynamicDesktop(server);
            StartDynamicWallpaperProcess(DynamicDesktopProgramFileName, path);
            DesktopTool.EmbedWindowToDesktop("MainWindow");

            var result = await MessageBoxEx.WaitMessageBox.Show("提示信息", "是否保存当前壁纸设置？", "保留");
            if (result == false)
            {
                SendMessageToDynamicWallpaper(DYWALLPAPER_RECOVERLAST);
            }
        }

        private void TerminateDynamicDesktop(AnonymousPipeServer server)
        {
            if (server != null)
                return;

            var processes = System.Diagnostics.Process.GetProcesses();
            var dynamicDesktopProcess = processes.FirstOrDefault(x => x.ProcessName == DynamicDesktopProcessName);
            dynamicDesktopProcess?.Kill();
        }

        private void StartDynamicWallpaperProcess(string processPath,string videoPath)
        {
            server.StartServer(processPath,"\"" + videoPath + "\"");
        }

        private void SendMessageToDynamicWallpaper(string message)
        {
            if (server == null)
                return;

            server.SendMessage(message);
        }

        public void StopDynamicWallpaperProcess()
        {
            DesktopTool.CloseEmbedWindow();
        }

        private void SetDynamicBackground_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDynamicWallpaperListAsync();
        }

        private void btnAddDynamicWallpaper_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "视频文件|*.mp4;*.avi;*.mpg;*.mpeg";

            if(openFileDialog.ShowDialog() == true)
            {
                var wallpaperName = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                var wallpaperFileName = System.IO.Path.GetFileName(openFileDialog.FileName);
                var dir = System.IO.Path.Combine(Environment.CurrentDirectory, "res\\dynamic wallpaper\\", wallpaperName);
                if (System.IO.Directory.Exists(dir) == false)
                    System.IO.Directory.CreateDirectory(dir);

                var thumbBitmap = DesktopTool.GetFileThumbnail(openFileDialog.FileName);
                var thumbnailPath = System.IO.Path.Combine(dir, wallpaperName + ".png");
                System.Drawing.Bitmap.FromHbitmap(thumbBitmap).Save(thumbnailPath);
                var targetFilePath = System.IO.Path.Combine(dir, wallpaperFileName);
                System.IO.File.Copy(openFileDialog.FileName, targetFilePath,true);
                GlobalConfig.Instance.DynamicWallpaperConfig.WallpaperList.Add(new DynamicWallpaperItem()
                {
                    Name = wallpaperFileName,
                    Path = targetFilePath,
                    Thumbnail = thumbnailPath
                });
            }

            LoadDynamicWallpaperListAsync();
        }
    }
}
