using Silver_Arowana.Shell.Pages;
using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Silver_Arowana.Shell
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : TianXiaTech.BlurWindow
    {
        private DesktopSetting desktopSetting = new Pages.DesktopSetting();
        Storyboard start;
        Storyboard end;

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
            BeginShowMenuAnimation("");
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
        }
    }
}
