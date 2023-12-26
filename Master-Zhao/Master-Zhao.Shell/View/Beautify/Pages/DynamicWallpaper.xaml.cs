using Master_Zhao.Config;
using Master_Zhao.Config.Model;
using Master_Zhao.IO;
using Master_Zhao.Shell.PInvoke;
using Master_Zhao.Shell.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private static readonly string DefaultWallpaperName = "默认桌面";

        private AnonymousPipeServer server = new AnonymousPipeServer();

        public DynamicWallpaper()
        {
            InitializeComponent();
            InitializeConfig();
        }

        private void InitializeConfig()
        {
            var dynamicWallpaperConfig = GlobalConfig.Instance.DynamicWallpaperConfig;
            this.cbx_AutoRepeat.IsChecked = dynamicWallpaperConfig.Repeat;
            this.cbx_Mute.IsChecked = dynamicWallpaperConfig.Mute;
            this.cbx_KeepWallpaper.IsChecked = dynamicWallpaperConfig.KeepWallpaper;
        }

        private async void LoadDynamicWallpaperListAsync()
        {
            var list = new List<DynamicWallpaperItem>(GlobalConfig.Instance.DynamicWallpaperConfig.WallpaperList);
            DynamicWallpaperItem defaultWallpaper = new DynamicWallpaperItem();
            defaultWallpaper.Name = DefaultWallpaperName;
            var sb = new StringBuilder(DesktopTool.MAX_PATH);
            if (DesktopTool.GetBackground(sb))
            {
                defaultWallpaper.Path = sb.ToString();
                defaultWallpaper.Thumbnail = sb.ToString();
            }

            list.Insert(0, defaultWallpaper);

            wrap.Children.Clear();
            await Task.Factory.StartNew(()=> {
                for(int i = 0;i<list.Count;i++)
                {
                    AppendDynamicWallpaperItem(list[i], i == 0);
                }
            },new System.Threading.CancellationToken(),TaskCreationOptions.None,TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void AppendDynamicWallpaperItem(DynamicWallpaperItem item,bool isDefaultDesktop = false)
        {
            DynamicWallpaperControl dynamicWallpaperControl = new DynamicWallpaperControl();
            dynamicWallpaperControl.WallpaperName = item.Name;
            dynamicWallpaperControl.VideoPath = System.IO.Path.GetFullPath(item.Path);
            if (!string.IsNullOrEmpty(item.Thumbnail))
                dynamicWallpaperControl.ThumbnailPath = System.IO.Path.GetFullPath(item.Thumbnail);
            dynamicWallpaperControl.OnPreview += DynamicWallpaperControl_OnPreview;

            if(isDefaultDesktop == true)
            {
                dynamicWallpaperControl.HidePreviewButton();
                dynamicWallpaperControl.OnSet += DefaultWallpaperControl_OnSet;
            }
            else
            {
                dynamicWallpaperControl.OnSet += DynamicWallpaperControl_OnSet;
            }
       
            dynamicWallpaperControl.OnSelect += DynamicWallpaperControl_OnSelect;
            wrap.Children.Add(dynamicWallpaperControl);
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
            SetDynamicWallpaper(path, false);
        }

        private void DynamicWallpaperControl_OnPreview(object sender, string path)
        {
            SetDynamicWallpaper(path, true);
        }

        private async void SetDynamicWallpaper(string path,bool isAsk)
        {           
            StartDynamicWallpaperProcess(server, DynamicDesktopProgramFileName, path);
            DesktopTool.EmbedWindowToDesktop("MainWindow");
            DesktopTool.SwitchToDesktop();

            if (isAsk)
            {
                var result = await MessageBoxEx.WaitMessageBox.Show("提示信息", "是否保存当前壁纸设置？", "保留");
                if (result == false)
                {
                    SendMessageToDynamicWallpaper(DYWALLPAPER_RECOVERLAST);
                }
            }

            UpdateDynamiicWallpaperIndex(path);
        }

        private void UpdateDynamiicWallpaperIndex(string path)
        {
            //TODO
            var wallpaperConfig = GlobalConfig.Instance.DynamicWallpaperConfig;
            var wallpaperList = wallpaperConfig.WallpaperList;
            var currentWallpaper = wallpaperList.FirstOrDefault(x => x.Path == path);
            GlobalConfig.Instance.DynamicWallpaperConfig.Wallpaperindex = wallpaperConfig.WallpaperList.IndexOf(currentWallpaper);
        }

        private void DefaultWallpaperControl_OnSet(object sender, string path)
        {
            StopDynamicWallpaperProcess();
        }

        private void TerminateDynamicDesktop(AnonymousPipeServer server)
        {
            var processes = System.Diagnostics.Process.GetProcesses();
            var dynamicDesktopProcess = processes.FirstOrDefault(x => x.ProcessName == DynamicDesktopProcessName);
            if (dynamicDesktopProcess != null)
            {
                dynamicDesktopProcess.Kill();
            }
        }

        private void StartDynamicWallpaperProcess(AnonymousPipeServer server, string processPath,string videoPath)
        {
            TerminateDynamicDesktop(server);
            server.StartServer(processPath,"\"" + videoPath + "\"");

            //set config
            var mute = GlobalConfig.Instance.DynamicWallpaperConfig.Mute == true ? S_OK : S_FALSE;
            var repeat = GlobalConfig.Instance.DynamicWallpaperConfig.Repeat == true ? S_OK : S_FALSE;
            SendMessageToDynamicWallpaper(DYWALLPAPER_MUTE + COMMANDSPLITCHAR + mute);
            SendMessageToDynamicWallpaper(DYWALLPAPER_REPEAT + COMMANDSPLITCHAR + repeat);
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
                System.Drawing.Image.FromHbitmap(thumbBitmap).Save(thumbnailPath);
                var targetFilePath = System.IO.Path.Combine(dir, wallpaperFileName);
                System.IO.File.Copy(openFileDialog.FileName, targetFilePath,true);
                var dynamicWallpaperItem = new DynamicWallpaperItem()
                {
                    Name = wallpaperFileName,
                    Path = targetFilePath,
                    Thumbnail = thumbnailPath
                };
                GlobalConfig.Instance.DynamicWallpaperConfig.WallpaperList.Add(dynamicWallpaperItem);
                AppendDynamicWallpaperItem(dynamicWallpaperItem);
            }
        }

        #region event
        private void cbx_AutoRepeat_Checked(object sender, RoutedEventArgs e)
        {
            GlobalConfig.Instance.DynamicWallpaperConfig.Repeat = true;
            SendMessageToDynamicWallpaper(DYWALLPAPER_REPEAT + COMMANDSPLITCHAR + S_OK);
        }

        private void cbx_AutoRepeat_Unchecked(object sender, RoutedEventArgs e)
        {
            GlobalConfig.Instance.DynamicWallpaperConfig.Repeat = false;
            SendMessageToDynamicWallpaper(DYWALLPAPER_REPEAT + COMMANDSPLITCHAR + S_FALSE);
        }

        private void cbx_Mute_Checked(object sender, RoutedEventArgs e)
        {
            GlobalConfig.Instance.DynamicWallpaperConfig.Mute = true;
            SendMessageToDynamicWallpaper(DYWALLPAPER_MUTE + COMMANDSPLITCHAR + S_OK);
        }

        private void cbx_Mute_Unchecked(object sender, RoutedEventArgs e)
        {
            GlobalConfig.Instance.DynamicWallpaperConfig.Mute = false;
            SendMessageToDynamicWallpaper(DYWALLPAPER_MUTE + COMMANDSPLITCHAR + S_FALSE);
        }

        private void cbx_KeepWallpaper_Checked(object sender, RoutedEventArgs e)
        {
            GlobalConfig.Instance.DynamicWallpaperConfig.KeepWallpaper = true;
        }

        private void cbx_KeepWallpaper_Unchecked(object sender, RoutedEventArgs e)
        {
            GlobalConfig.Instance.DynamicWallpaperConfig.KeepWallpaper = false;
        }

        private void btnImportBilibili_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("敬请期待");
            //Master_Zhao.Shell.Windows.BilibiliDownloader bilibiliDownloader = new Windows.BilibiliDownloader();
            //bilibiliDownloader.Owner = Application.Current.MainWindow;
            //bilibiliDownloader.Show();
        }

        private void cbx_Startup_Checked(object sender, RoutedEventArgs e)
        {
            GlobalConfig.Instance.DynamicWallpaperConfig.AutoRunWithStarup = true;
            StartupTool.CreateStartupRun(Assembly.GetExecutingAssembly().Location);
        }

        private void cbx_Startup_Unchecked(object sender, RoutedEventArgs e)
        {
            GlobalConfig.Instance.DynamicWallpaperConfig.AutoRunWithStarup = false;
            StartupTool.RemoveStartupRun(Assembly.GetExecutingAssembly().Location);
        }
        #endregion

        #region static funcs
        public Task GetStartupTask()
        {
            var config = GlobalConfig.Instance.DynamicWallpaperConfig;

            if (config.AutoRunWithStarup == false)
                return Task.Delay(0);

            return Task.Factory.StartNew(() => {
                var path = config.WallpaperList[config.Wallpaperindex].Path;
                SetDynamicWallpaper(path, false);
            }, new System.Threading.CancellationToken(), TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }
        #endregion
    }
}
