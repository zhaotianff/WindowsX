using Master_Zhao.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Master_Zhao.Shell.View.Setting.Pages
{
    /// <summary>
    /// BackgroundSetting.xaml 的交互逻辑
    /// </summary>
    public partial class BackgroundSetting : Page
    {
        private bool isBackgroundLoaded = false;

        public BackgroundSetting()
        {
            InitializeComponent();
        }

        public void LoadBackground()
        {
            if (isBackgroundLoaded == true)
                return;

            var backgroundConfig = GlobalConfig.Instance.MainConfig.BackgroundSetting;
            for(int i = 0;i<backgroundConfig.Colors.Count;i++)
            {
                AppendBackgroundItem(new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundConfig.Colors[i])));
            }
            for(int i = 0;i<backgroundConfig.ResourceImages.Count;i++)
            {
                //DrawingBrush DrawingImage
                AppendBackgroundItem(new ImageBrush() { ImageSource = new BitmapImage(new Uri($"pack://application:,,,/{backgroundConfig.ResourceImages[i]}")) ,Stretch = Stretch.UniformToFill});
            }
            for (int i = 0; i < backgroundConfig.LocalImages.Count; i++)
            {
                //DrawingBrush DrawingImage
                var imagePath = backgroundConfig.LocalImages[i];

                if (System.IO.File.Exists(imagePath) == false)
                    continue;

                AppendBackgroundItem(new ImageBrush() { ImageSource = new BitmapImage(new Uri(backgroundConfig.LocalImages[i], UriKind.Absolute)),Stretch = Stretch.UniformToFill });
            }

            this.slider_Opacity.Value = backgroundConfig.Opacity;

            isBackgroundLoaded = true;
        }

        private void AppendBackgroundItem(Brush brush)
        {
            Rectangle rectangle = new Rectangle();
            rectangle.Margin = new Thickness(10);
            rectangle.Fill = brush;
            rectangle.MouseDown += Setbackground_MouseDown;
            this.wrap.Children.Add(rectangle);
        }

        private void Setbackground_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow;
            mainWindow.Background = (sender as Rectangle).Fill;
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.Duration = TimeSpan.FromSeconds(2);
            doubleAnimation.From = 0;
            doubleAnimation.To = 1;

            if(mainWindow.Background is ImageBrush)
            {
                mainWindow.Background.BeginAnimation(Brush.OpacityProperty, doubleAnimation);
            }

            var index = this.wrap.Children.IndexOf(sender as Rectangle);
            GlobalConfig.Instance.MainConfig.BackgroundSetting.Index = index;
        }

        private void slider_Opacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Application.Current.MainWindow.Background.Opacity = e.NewValue;
            GlobalConfig.Instance.MainConfig.BackgroundSetting.Opacity = (float)e.NewValue;
        }
    }
}
