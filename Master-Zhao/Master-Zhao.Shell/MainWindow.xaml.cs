using Master_Zhao.Config;
using Master_Zhao.Shell.Infrastructure.Navigation;
using Master_Zhao.Shell.Pages;
using Master_Zhao.Shell.PInvoke;
using Master_Zhao.Shell.StartMenu;
using Master_Zhao.Shell.Util;
using Master_Zhao.Shell.View.Setting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Master_Zhao.Shell
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : TianXiaTech.BlurWindow
    {
        private DesktopBeautify desktopBeautify = new DesktopBeautify();
        private SystemManagement systemManagement = new SystemManagement();
        private ToolsPage toolsPage = new ToolsPage();
        private SettingPage settingPage = new SettingPage();
        private AboutPage aboutPage = new AboutPage();
        Storyboard start;
        Storyboard end;

        private NavigationPages CurrentPage { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeAnimation();
            InitializeBackground();
            DoStartupTasksAsync();
            CreateNotifyIcon();
        }

        private void InitializeAnimation()
        {
            start = TryFindResource("start") as Storyboard;
            end = TryFindResource("end") as Storyboard;
            start.Completed += (sender, e) => { main.Visibility = Visibility.Collapsed; };
            end.Completed += (sender, e) => { main.Visibility = Visibility.Visible; this.frame.Content = null; };
        }

        private void InitializeBackground()
        {
            var wallpaperConfig = GlobalConfig.Instance.MainConfig.BackgroundSetting;
            var index = wallpaperConfig.Index;
            var tempIndex = index;
            var colorsLength = wallpaperConfig.Colors.Count;
            var resourcesLength = wallpaperConfig.ResourceImages.Count;

            try
            {
                if (index < colorsLength)
                {
                    this.Background = new System.Windows.Media.SolidColorBrush((Color)ColorConverter.ConvertFromString(wallpaperConfig.Colors[index]));
                }
                else if (index >= colorsLength && index < colorsLength + resourcesLength)
                {
                    tempIndex = index - colorsLength;
                    this.Background = new ImageBrush() { ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri($"pack://application:,,,/{wallpaperConfig.ResourceImages[tempIndex]}")), Stretch = Stretch.UniformToFill };
                }
                else
                {
                    tempIndex = index - colorsLength - resourcesLength;
                    this.Background = new ImageBrush() { ImageSource = new System.Windows.Media.Imaging.BitmapImage(new Uri(wallpaperConfig.LocalImages[tempIndex], UriKind.Absolute)), Stretch = Stretch.UniformToFill };
                }

                this.Background.Opacity = wallpaperConfig.Opacity;
            }
            catch
            {

            }
        }

        private async void DoStartupTasksAsync()
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(desktopBeautify.dynamicWallpaper.GetStartupTask());
            await Task.WhenAll(tasks);
        }

        private void CreateNotifyIcon()
        {
            NotifyIconCreateData data = new NotifyIconCreateData();
            data.ClickHandler = ShowOrHiderMainWindow;
            data.ContextMenu = this.TryFindResource("NotifyIconContextMenu") as ContextMenu;
            InitContextMenuEvent(data.ContextMenu);
            data.IconRelativePath = "logo.ico";
            data.Tooltip = "Master-Zhao";
            NotifyIconHelper.Instance.CreateNotifyIcon(data);
            NotifyIconHelper.Instance.SetNotifyIconState(true);
        }

        private void InitContextMenuEvent(ContextMenu contextMenu)
        {
            if (contextMenu == null)
                return;

            MenuItem exitMenu = contextMenu.Items[0] as MenuItem;
            exitMenu.Click += (a, b) => 
            {
                NotifyIconHelper.Instance.SetNotifyIconState(false);
                BlurWindow_Closing(null, null);
                Environment.Exit(0);
            };
        }


        public void BeginShowMenuAnimation(NavigationPages targetPage,bool isStateChanged = false)
        {
            DoubleAnimation startWidthAnimation = start.Children[0] as DoubleAnimation;
            startWidthAnimation.From = this.ActualWidth - 100;
            startWidthAnimation.To = this.ActualWidth;
            DoubleAnimation startHeightAnimation = start.Children[1] as DoubleAnimation;
            startHeightAnimation.From = this.ActualHeight - 50;
            startHeightAnimation.To = this.ActualHeight;
            var pageContent = GetNavigationPage(targetPage);
            if(isStateChanged == false)
                (pageContent as IPageAction)?.ShowDefaultPage();//TODO
            this.frame.Content = pageContent;
            start?.Begin();
            CurrentPage = targetPage;
        }

        public void EndShowMenuAnimation()
        {
            DoubleAnimation endWidthAnimation = end.Children[0] as DoubleAnimation;
            endWidthAnimation.From = this.ActualWidth;
            endWidthAnimation.To = 500;
            DoubleAnimation endHeightAnimation = end.Children[1] as DoubleAnimation;
            endHeightAnimation.From = this.ActualHeight;
            endHeightAnimation.To = 250;
            end?.Begin();
            CurrentPage = NavigationPages.Main;
        }

        private System.Windows.Controls.Page GetNavigationPage(NavigationPages page)
        {
            switch(page)
            {
                case NavigationPages.Main:
                    return null;
                case NavigationPages.Beautify:
                    return desktopBeautify;
                case NavigationPages.SysManagement:
                    return systemManagement;
                case NavigationPages.Utility:
                    return toolsPage;
                case NavigationPages.HuaShui:
                    return null;
                case NavigationPages.Setting:
                    return settingPage;
                case NavigationPages.About:
                    return aboutPage;
                default:
                    return null;
            }
        }

        private void ShowOrHiderMainWindow()
        {
            if(this.Visibility == Visibility.Visible)
            {
                HideMainWindow();
            }
            else
            {
                ShowMainWindow();
            }
        }

        private async void ShowMainWindow()
        {
            this.Visibility = Visibility.Visible;
            this.ShowInTaskbar = true;

            if (WindowState == WindowState.Minimized)
            {
                await Task.Delay(100);
                WindowState = WindowState.Normal;
            }
        }

        private void HideMainWindow()
        {
            this.Visibility = Visibility.Hidden;
            this.ShowInTaskbar = false;
        }

        #region Event

        private void BlurWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            main.Width = e.NewSize.Width * 0.7;
            main.Height = e.NewSize.Height * 0.85;
        }

        private void btnDesktop_Click(object sender, RoutedEventArgs e)
        {
            BeginShowMenuAnimation(NavigationPages.Beautify);
        }

        private void btnUtility_Click(object sender, RoutedEventArgs e)
        {
            BeginShowMenuAnimation(NavigationPages.Utility);
        }

        private void btnSystem_Click(object sender, RoutedEventArgs e)
        {
            BeginShowMenuAnimation(NavigationPages.SysManagement);
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            BeginShowMenuAnimation(NavigationPages.About);
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            BeginShowMenuAnimation(NavigationPages.Setting);
        }

        private void BlurWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (GlobalConfig.Instance.DynamicWallpaperConfig.KeepWallpaper == false)
            {
                DesktopTool.CloseEmbedWindow();
            }

            GlobalConfig.Instance.SaveAllConfig();

            //TODO 
            toolsPage.Terminate();
            StartMenuManager.UnHookStart();
            NotifyIconHelper.Instance.RemoveNotifyIcon();
        }

        private async void BlurWindow_StateChanged(object sender, EventArgs e)
        {
            if (CurrentPage != NavigationPages.Main)
            {
                await System.Threading.Tasks.Task.Delay(100);
                BeginShowMenuAnimation(CurrentPage,true);
            }

            if(WindowState == WindowState.Minimized)
            {
                await Task.Delay(100);
                HideMainWindow();
            }
        }
        #endregion
    }
}
