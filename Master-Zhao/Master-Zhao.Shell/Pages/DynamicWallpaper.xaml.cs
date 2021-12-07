using Master_Zhao.Config;
using Master_Zhao.Config.Model;
using Master_Zhao.Shell.PInvoke;
using Master_Zhao.Shell.UserControls;
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

namespace Master_Zhao.Shell.Pages
{
    /// <summary>
    /// DynamicWallpaper.xaml 的交互逻辑
    /// </summary>
    public partial class DynamicWallpaper : Page
    {
        private static readonly string DynamicDesktopProgramFileName = "Master-Zhao.DynamicDesktop.exe";
        private static readonly string ExePath = System.IO.Path.GetFullPath($"../../../../Master-Zhao.DynamicDesktop/bin/Debug/net5.0-windows/{DynamicDesktopProgramFileName}");
       
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
                    wrap.Children.Add(dynamicWallpaperControl);
                }
            },new System.Threading.CancellationToken(),TaskCreationOptions.None,TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void DynamicWallpaperControl_OnSet(object sender, string path)
        {
            
        }

        private void DynamicWallpaperControl_OnPreview(object sender, string path)
        {
            System.Diagnostics.Process.Start(ExePath, "\"" + path + "\"");
            var result = DesktopTool.EmbedWindowToDesktop("MainWindow");
            //MessageBox.Show(result.ToString());
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
            openFileDialog.Filter = "全部文件|*.*";

            if(openFileDialog.ShowDialog() == true)
            {
                GlobalConfig.Instance.DynamicWallpaperConfig.WallpaperList.Add(new DynamicWallpaperItem() { Name = System.IO.Path.GetFileName(openFileDialog.FileName), Path = openFileDialog.FileName});
            }

            LoadDynamicWallpaperListAsync();
        }
    }
}
