using Master_Zhao.Config;
using Master_Zhao.Shell.Pages;
using Master_Zhao.Shell.PInvoke;
using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Master_Zhao.Shell
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : TianXiaTech.BlurWindow
    {
        private DesktopSetting desktopSetting = new Pages.DesktopSetting();
        Storyboard start;
        Storyboard end;

        private string CurrentPage { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeAnimation();
        }

        private void InitializeAnimation()
        {
            start = TryFindResource("start") as Storyboard;
            end = TryFindResource("end") as Storyboard;
            start.Completed += (sender, e) => { main.Visibility = Visibility.Collapsed; };
            end.Completed += (sender, e) => { main.Visibility = Visibility.Visible; this.frame.Content = null; };
        }

        private void BlurWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            main.Width = e.NewSize.Width * 0.7;
            main.Height = e.NewSize.Height * 0.85;
        }

        private void btnDesktop_Click(object sender, RoutedEventArgs e)
        {
            BeginShowMenuAnimation("Wallpaper");
        }

        public void BeginShowMenuAnimation(string targetPage)
        {
            DoubleAnimation startWidthAnimation = start.Children[0] as DoubleAnimation;
            startWidthAnimation.From = this.ActualWidth - 100;
            startWidthAnimation.To = this.ActualWidth;
            DoubleAnimation startHeightAnimation = start.Children[1] as DoubleAnimation;
            startHeightAnimation.From = this.ActualHeight - 50;
            startHeightAnimation.To = this.ActualHeight;
            this.frame.Content = desktopSetting;
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
            CurrentPage = "";
        }

        private void BlurWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (GlobalConfig.Instance.DynamicWallpaperConfig.KeepWallpaper == false)
            {
                DesktopTool.CloseEmbedWindow();
            }

            desktopSetting.CloseAnonymousPipe();

            GlobalConfig.Instance.SaveAllConfig();
        }

        private async void BlurWindow_StateChanged(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(CurrentPage))
            {
                //TODO Temp
                await System.Threading.Tasks.Task.Delay(100);
                BeginShowMenuAnimation("Wallpaper");
            }
        }
    }
}
