using Silver_Arowana.Shell.PInvoke;
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
using Silver_Arowana.Shell.Controls;
using Silver_Arowana.Web.Util;
using Silver_Arowana.Web.Model;
using Silver_Arowana.Web.Interface;
using Silver_Arowana.Web.CnBing;
using Silver_Arowana.Shell.Windows;

namespace Silver_Arowana.Shell.Pages
{
    /// <summary>
    /// StaticWallpaper.xaml 的交互逻辑
    /// </summary>
    public partial class StaticWallpaper : Page
    {
        private IEnumerable<string> recentWallpapers;
        private List<ITagImg> list = new List<ITagImg>();
        private IImageSearcher imageSearcher;
        public string CurrentBackground { get; set; }

        public StaticWallpaper()
        {
            InitializeComponent();
            imageSearcher = new BingImageSearcher();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCurrentBackground();
            LoadRecentBackground();
            LoadDailyWallpaper();
        }

        private void LoadCurrentBackground()
        {
            var sb = new StringBuilder(DesktopTool.MAX_PATH);
            if (DesktopTool.GetBackground(sb))
            {
                CurrentBackground = sb.ToString();
                SetPreviewImage(CurrentBackground);
            }
        }

        private void SetPreviewImage(string path)
        {
            this.img_background.Source = new BitmapImage(new Uri(path, UriKind.Absolute));
        }

        private async void LoadRecentBackground()
        {
            await LoadRecentBackgroundAsync();
        }

        private async Task LoadRecentBackgroundAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                wrap_wallpaper.Children.Clear();
                var sb = new StringBuilder(1024);
                DesktopTool.GetRecentBackground(sb);
                recentWallpapers = sb.ToString().Split(";").Take(5);

                foreach (var wallpaper in recentWallpapers)
                {
                    ThumbImageControl thumbImageControl = new ThumbImageControl();
                    thumbImageControl.ThumbPath = wallpaper;
                    thumbImageControl.Click += OnChangeBackground;
                    wrap_wallpaper.Children.Add(thumbImageControl);
                }
            }, new System.Threading.CancellationToken(), TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());

        }

        private void LoadDailyWallpaper()
        {
            this.text_Keyword.Text = "壁纸推荐";
            btn_SearchClick(null, null);
        }

        private void OnChangeBackground(object sender,EventArgs args)
        {
            var path = (sender as ThumbImageControl)?.ThumbPath;

            if (System.IO.File.Exists(path) == false)
                return;

            if (SetBackground(path) == false)
                return;

            SetPreviewImage(path);

            var index = recentWallpapers.ToList().IndexOf(path);
            if (index == 0)
                return;

            var firstThumbImage = wrap_wallpaper.Children[0] as ThumbImageControl;
            var firstThumbImagePath = firstThumbImage.ThumbPath;
            firstThumbImage.ThumbPath = path;
            (wrap_wallpaper.Children[index] as ThumbImageControl).ThumbPath = firstThumbImagePath;
        }

        private bool SetBackground(string path)
        {
            //设置壁纸
            StringBuilder sb = new StringBuilder(PInvoke.DesktopTool.MAX_PATH);
            sb.Append(path);
            return PInvoke.DesktopTool.SetBackground(sb);               
        }

        private async void btn_SearchClick(object sender, RoutedEventArgs e)
        {
            var keyword = this.text_Keyword.Text;
            if (string.IsNullOrEmpty(keyword))
                return;

            keyword += " " + SystemParameters.PrimaryScreenWidth + "x" + SystemParameters.PrimaryScreenHeight;
            list.Clear();

            //TODO cancel task
            list = await imageSearcher.SearchImageAsync(keyword);
            this.panel_OnlineImgList.Children.Clear();
            foreach (var item in list)
            {
                ImgFuncButton image = new ImgFuncButton();
                image.Width = 150;
                image.Height = 150;
                image.Margin = new Thickness(5);
                image.MouseDown += SetNetworkImage;
                image.DetailUrl = item.Src;
                this.panel_OnlineImgList.Children.Add(image);
            }
        }

        private void SetNetworkImage(object sender, MouseButtonEventArgs e)
        {
            //TODO 一些图片不显示
            var index = panel_OnlineImgList.Children.IndexOf(sender as ImgFuncButton);
            ImageWindow imageWindow = new ImageWindow();
            imageWindow.SetImageUrl(list[index].DetailUrl, CurrentBackground);
            imageWindow.Show();
        }
    }
}
