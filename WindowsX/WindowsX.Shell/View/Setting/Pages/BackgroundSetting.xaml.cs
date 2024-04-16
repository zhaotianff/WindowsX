using WindowsX.Config;
using WindowsX.Config.Helper;
using WindowsX.Shell.Controls.UserControls;
using WindowsX.Shell.Util;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace WindowsX.Shell.View.Setting.Pages
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

        public void Load()
        {
            LoadBackground();
            LoadTheme();
        }

        public void LoadBackground()
        {
            if (isBackgroundLoaded == true)
                return;

            var backgroundConfig = GlobalConfig.Instance.MainConfig.BackgroundSetting;
            for(int i = 0;i<backgroundConfig.Colors.Count;i++)
            {
                AppendBackgroundItem(colorStr: backgroundConfig.Colors[i], title: backgroundConfig.Colors[i]);
            }
            for(int i = 0;i<backgroundConfig.ResourceImages.Count;i++)
            {
                AppendBackgroundItem($"pack://application:,,,/{backgroundConfig.ResourceImages[i]}", title: $"预置{i + 1}");
            }
            for (int i = 0; i < backgroundConfig.LocalImages.Count; i++)
            {
                var imagePath = backgroundConfig.LocalImages[i];

                if (System.IO.File.Exists(imagePath) == false)
                    continue;

                AppendBackgroundItem(imagePath, title: Path.GetFileNameWithoutExtension(imagePath));
            }

            this.slider_Opacity.Value = backgroundConfig.Opacity;

            isBackgroundLoaded = true;
        }

        public void LoadTheme()
        {
            //temp
            ThemeControl themeControl = new ThemeControl();
            themeControl.ColorBrush = System.Windows.Media.Brushes.Silver;
            themeControl.Margin = new Thickness(10);
            themeControl.Title = "默认";
            themeControl.ThemeFile = "pack://application:,,,/Themes/Default.xaml";
            themeControl.MouseDown += ThemeControl_MouseDown;

            ThemeControl themeControl2 = new ThemeControl();
            themeControl2.ColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#39375B")); ;
            themeControl2.Margin = new Thickness(10);
            themeControl2.Title = "深色";
            themeControl2.ThemeFile = "pack://application:,,,/Themes/Night1.xaml";
            themeControl2.MouseDown += ThemeControl_MouseDown;

            this.wrap_theme.Children.Clear();
            this.wrap_theme.Children.Add(themeControl);
            this.wrap_theme.Children.Add(themeControl2);
        }

        private void ThemeControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var themeControl = sender as ThemeControl;
            var rd = Application.Current.Resources.MergedDictionaries[1];
            rd.Source = new Uri(themeControl.ThemeFile, UriKind.Absolute);
            GlobalConfig.Instance.MainConfig.BackgroundSetting.ThemeIndex = wrap_theme.Children.IndexOf(themeControl);
        }

        private ThemeControl AppendBackgroundItem(string imagePath = "",string colorStr = "",string title = "")
        {
            ThemeControl themeControl = new ThemeControl();
            if (!string.IsNullOrEmpty(imagePath))
            {
                themeControl.Stretch = Stretch.UniformToFill;
                themeControl.ImagePath = imagePath;
            }

            if (!string.IsNullOrEmpty(colorStr))
                themeControl.ColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorStr));

            themeControl.Margin = new Thickness(10);
            themeControl.Title = title;
            themeControl.MouseDown += Setbackground_MouseDown;

            this.wrap.Children.Add(themeControl);
            return themeControl;
        }

        private void Setbackground_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetBackground(sender);
        }

        private void SetBackground(object sender)
        {
            var themeControl = sender as ThemeControl;

            var mainWindow = Application.Current.MainWindow;

            if(!string.IsNullOrEmpty(themeControl.ImagePath))
            {
                mainWindow.Background = new ImageBrush() { ImageSource = new BitmapImage(new Uri(themeControl.ImagePath, UriKind.Absolute)), Opacity = this.slider_Opacity.Value, Stretch = themeControl.Stretch };
            }
            else
            {
                mainWindow.Background = themeControl.ColorBrush;
            }
            
            if (mainWindow.Background is ImageBrush)
            {
                DoubleAnimation doubleAnimation = new DoubleAnimation();
                doubleAnimation.Duration = TimeSpan.FromSeconds(2);
                doubleAnimation.From = 0;
                doubleAnimation.To = this.slider_Opacity.Value;
                doubleAnimation.Completed += DoubleAnimation_Completed;
                mainWindow.Background.BeginAnimation(Brush.OpacityProperty, doubleAnimation);
            }

            var index = this.wrap.Children.IndexOf(themeControl);
            GlobalConfig.Instance.MainConfig.BackgroundSetting.BackgroundIndex = index;
        }

        private void DoubleAnimation_Completed(object sender, EventArgs e)
        {
            Application.Current.MainWindow.Background.BeginAnimation(Brush.OpacityProperty, null);
        }

        private void slider_Opacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Application.Current.MainWindow.Background.Opacity = e.NewValue;
            GlobalConfig.Instance.MainConfig.BackgroundSetting.Opacity = (float)e.NewValue;
        }

        private void btn_BrowseImage(object sender, RoutedEventArgs e)
        {
            var file = DialogHelper.BrowserSingleFile("图片文件|*.jpg;*.png;*.bmp;*.tiff");

            if (string.IsNullOrEmpty(file))
                return;

            var backgroundImageList = GlobalConfig.Instance.MainConfig.BackgroundSetting.LocalImages;
            backgroundImageList.Add(file);
            var rect = AppendBackgroundItem(file,title:System.IO.Path.GetFileNameWithoutExtension(file));
            SetBackground(rect);
        }
    }
}
