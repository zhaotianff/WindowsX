using Silver_Arowana.Video;
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

namespace Silver_Arowana.Shell.UserControls
{
    /// <summary>
    /// Interaction logic for DynamicWallpaperItem.xaml
    /// </summary>
    public partial class DynamicWallpaperItem : UserControl
    {
        private string videoPath;

        public string VideoPath 
        {
            get => videoPath;
            set
            {
                videoPath = value;
                image.Source = VideoHelper.GetVideoThumbnail(videoPath);
            }
        }

        public delegate void WallpaperEventHandler(object sender, string path);
        public event WallpaperEventHandler OnPreview;
        public event WallpaperEventHandler OnSet;

        public DynamicWallpaperItem()
        {
            InitializeComponent();
        }

        private void set_Click(object sender, RoutedEventArgs e)
        {
            OnSet?.Invoke(sender, videoPath);
        }

        private void preview_Click(object sender, RoutedEventArgs e)
        {
            OnPreview?.Invoke(sender, videoPath);
        }
    }
}
