using WindowsX.Shell.Util;
using WindowsX.Video;
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

namespace WindowsX.Shell.UserControls
{
    /// <summary>
    /// Interaction logic for DynamicWallpaperItem.xaml
    /// </summary>
    public partial class DynamicWallpaperControl : UserControl
    {
        private string thumbnailPath = "";
        private string wallpaperName = "";
        private SolidColorBrush accentBaseColor;

        public bool IsChecked { get; set; } = false;

        public string VideoPath { get; set; }

        public string ThumbnailPath
        {
            get => thumbnailPath;
            set
            {
                thumbnailPath = value;
                image.Source = ImageHelper.GetBitmapImageFromLocalFile(thumbnailPath);
            }
        }

        public string WallpaperName
        {
            get => wallpaperName;
            set
            {
                wallpaperName = value;
                title.Content = wallpaperName;
            }
        }

        public delegate void WallpaperEventHandler(object sender, string path);
        public event WallpaperEventHandler OnPreview;
        public event WallpaperEventHandler OnSet;
        public event EventHandler OnSelect;

        public DynamicWallpaperControl()
        {
            InitializeComponent();
            accentBaseColor = this.TryFindResource("AccentBaseColor") as SolidColorBrush;
        }

        private void set_Click(object sender, RoutedEventArgs e)
        {
            OnSet?.Invoke(sender, VideoPath);
        }

        private void preview_Click(object sender, RoutedEventArgs e)
        {
            OnPreview?.Invoke(sender, VideoPath);
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsChecked == false)
            {
                ResetBorderBrush();
            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OnSelect?.Invoke(sender,e);
            SetBorderBrush();
            IsChecked = true;
        }

        private void SetBorderBrush()
        {
            BorderBrush = accentBaseColor;
        }

        public void ResetBorderBrush()
        {
            BorderBrush = Brushes.Transparent;
        }

        public void HidePreviewButton()
        {
            btn_Preview.Visibility = Visibility.Collapsed;
        }
    }
}
