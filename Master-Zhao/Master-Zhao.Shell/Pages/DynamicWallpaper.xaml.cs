using Master_Zhao.Config;
using Master_Zhao.Config.Model;
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

namespace Master_Zhao.Shell.Pages
{
    /// <summary>
    /// DynamicWallpaper.xaml 的交互逻辑
    /// </summary>
    public partial class DynamicWallpaper : Page
    {
        private static readonly string DynamicDesktopProgramFileName = "Master-Zhao.DynamicDesktop.exe";
        private static readonly string ExePath = System.IO.Path.GetFullPath($"../../../../Master-Zhao.DynamicDesktop/bin/Debug/net5.0-windows/{DynamicDesktopProgramFileName}");
        private static readonly string DynamicDesktopProgramClassName = "MainWindow";

        private const int WM_USER = 0x0400;
        private const int WM_PAUSE = WM_USER + 0x100;
        private const int WM_PLAY = WM_USER + 0x101;
        private const int WM_SETVIDEO = WM_USER + 0x103;
        private const int WM_MUTE = WM_USER + 0x104;
        private const int WM_REPEAT = WM_USER + 0x105;
        private const int WM_GETVIDEO = WM_USER + 0x106;

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
            IntPtr hProgman = User32.FindWindow("Progman", "Program Manager");
            IntPtr ptr = User32.FindWindowEx(hProgman, IntPtr.Zero, null, DynamicDesktopProgramClassName);
            if(ptr == IntPtr.Zero)
            {
                System.Diagnostics.Process.Start(ExePath, "\"" + path + "\"");
                var result = DesktopTool.EmbedWindowToDesktop("MainWindow");
            }
            else
            {
                //IntPtr ptrPath = Marshal.AllocHGlobal(1024);
                //var buffer = Encoding.UTF8.GetBytes(path);
                //Marshal.Copy(buffer, 0, ptrPath, buffer.Length);
                //_ = User32.SendMessage(ptr, (uint)WM_SETVIDEO, (IntPtr)buffer.Length, ptrPath);            
            }

            await Task.Delay(1000);
            var isKeep = await MessageBoxEx.WaitMessageBox.Show("提示信息", "是否保存当前壁纸设置？", "保留");

            if(isKeep == false)
            {
                if(ptr == IntPtr.Zero)
                {
                    DesktopTool.CloseEmbedWindow();
                }
                else
                {
                    //IntPtr pathPtr = Marshal.AllocHGlobal(1024);
                    //var result = User32.SendMessage(ptr, WM_GETVIDEO, (IntPtr)1024, pathPtr);
                    //if(result > 0)
                    //{
                    //    User32.SendMessage(ptr, WM_SETVIDEO, IntPtr.Zero, pathPtr);
                    //}                
                }
            }
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
