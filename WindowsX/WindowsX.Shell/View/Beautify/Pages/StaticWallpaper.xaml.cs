﻿using WindowsX.Shell.PInvoke;
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
using WindowsX.Shell.Controls;
using WindowsX.Web.Util;
using WindowsX.Web.Model;
using WindowsX.Web.Interface;
using WindowsX.Web.CnBing;
using WindowsX.Shell.Windows;
using WindowsX.Shell.Util;

namespace WindowsX.Shell.Pages
{
    /// <summary>
    /// StaticWallpaper.xaml 的交互逻辑
    /// </summary>
    public partial class StaticWallpaper : Page
    {
        private const string CacheFolderName = "cache";
        private const int ThumbnailWidth = 800;

        private IEnumerable<string> recentWallpapers;
        private List<ITagImg> list = new List<ITagImg>();
        private IImageSearcher imageSearcher;
        public string CurrentBackground { get; set; }
        private int ImagePage { get; set; } = 1;
        private string CurrentKeyWord { get; set; }

        public StaticWallpaper()
        {
            InitializeComponent();
            imageSearcher = new BingImageSearcher();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCurrentBackground();
            LoadCurrentBackgroundPosition();
            LoadRecentBackground();
            LoadDailyWallpaper();
        }

        private void RefreshBackgroundList()
        {
            LoadCurrentBackground();
            LoadRecentBackground();
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

        private void LoadCurrentBackgroundPosition()
        {
            DesktopTool.DESKTOP_WALLPAPER_POSITION position = DesktopTool.DESKTOP_WALLPAPER_POSITION.DWPOS_STRETCH;
            DesktopTool.GetBackgroundPosition(ref position);
            this.cbx_position.SelectedIndex = (int)position;
            this.cbx_position.SelectionChanged += Cbx_position_SelectionChanged;
        }

        private void Cbx_position_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DesktopTool.SetBackgroundPosition((DesktopTool.DESKTOP_WALLPAPER_POSITION)cbx_position.SelectedIndex);
        }

        private void SetPreviewImage(string path)
        {
            if (!System.IO.File.Exists(path))
                return;

            this.img_background.Source = ImageHelper.GetBitmapImageFromLocalFile(path, true);
            //this.img_background.ImagePath = path;
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
                recentWallpapers = RemoveUnavailableWallpaper(recentWallpapers);

                GenerateBackgroundImageThumbnail(recentWallpapers);

                foreach (var wallpaper in recentWallpapers)
                {
                    ThumbImageControl thumbImageControl = new ThumbImageControl();
                    thumbImageControl.ThumbPath = GetThumbnailPathFromFilePath(wallpaper);
                    thumbImageControl.FilePath = wallpaper;
                    thumbImageControl.Click += OnChangeBackground;
                    wrap_wallpaper.Children.Add(thumbImageControl);
                }
            }, new System.Threading.CancellationToken(), TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void GenerateBackgroundImageThumbnail(IEnumerable<string> recentWallpapers)
        {
            var thumbFolderPath = GetOrCreateThumbnailImageCacheFolder();
           
            foreach (var wallpaper in recentWallpapers)
            {
                var thumbFilePath = System.IO.Path.Combine(thumbFolderPath, System.IO.Path.GetFileName(wallpaper));
                if (System.IO.File.Exists(thumbFilePath) == false)
                {
                    ImageHelper.GenerateThumbnailFile(wallpaper, ThumbnailWidth, thumbFilePath);
                }
            } 
        }

        private string GetThumbnailPathFromFilePath(string wallpaper)
        {
            var thumbnailFolderPath = GetOrCreateThumbnailImageCacheFolder();
            return System.IO.Path.Combine(thumbnailFolderPath, System.IO.Path.GetFileName(wallpaper));
        }

        private string GetOrCreateThumbnailImageCacheFolder()
        {
            var folderPath = Environment.CurrentDirectory + "\\" + CacheFolderName;
            if (System.IO.Directory.Exists(folderPath) == false)
                System.IO.Directory.CreateDirectory(folderPath);

            return folderPath;
        }

        private List<string> RemoveUnavailableWallpaper(IEnumerable<string> list)
        {
            var tempList = new List<string>(list);
            for (int i = list.Count() -1; i >= 0; i--)
            {
                if (System.IO.File.Exists(list.ElementAt(i)) == false)
                    tempList.RemoveAt(i);
            }
            return tempList;
        }

        private void LoadDailyWallpaper()
        {
            this.text_Keyword.Text = "壁纸推荐";
            btn_SearchClick(null, null);
        }

        private void OnChangeBackground(object sender,EventArgs args)
        {
            var path = (sender as ThumbImageControl)?.FilePath;

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

        private void btn_SearchClick(object sender, RoutedEventArgs e)
        {
            var keyword = this.text_Keyword.Text;
            if (string.IsNullOrEmpty(keyword))
                return;

            ImagePage = 1;
            //keyword += " " + SystemParameters.PrimaryScreenWidth + "x" + SystemParameters.PrimaryScreenHeight;
            keyword += " " + "电脑壁纸";
            CurrentKeyWord = keyword;
            SearchImageAsync(keyword, ImagePage);
        }

        private async void SearchImageAsync(string keyword,int page)
        {
            list.Clear();

            //TODO cancel task
            //TODO 这个接口貌似会封IP
            list = await imageSearcher.SearchImageAsync(keyword,page);
            this.panel_OnlineImgList.Children.Clear();
            this.scroll.ScrollToTop();
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

        private void btnPreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentKeyWord))
                return;
            if (ImagePage > 0)
            {
                ImagePage--;
                SearchImageAsync(CurrentKeyWord, ImagePage);
            }
        }

        private void btnNextPage_Click(object sender, RoutedEventArgs e)
        {        
            if (string.IsNullOrEmpty(CurrentKeyWord))
                return;
            ImagePage++;
            SearchImageAsync(CurrentKeyWord, ImagePage);
        }

        private void SetNetworkImage(object sender, MouseButtonEventArgs e)
        {
            //TODO 一些图片不显示
            CloseOtherImageWindow();
            var index = panel_OnlineImgList.Children.IndexOf(sender as ImgFuncButton);
            ImageWindow imageWindow = new ImageWindow(RefreshBackgroundList);
            imageWindow.Owner = Application.Current.MainWindow;
            imageWindow.SetImageUrl(list[index].DetailUrl, CurrentBackground);
            imageWindow.Show();
        }

        private void CloseOtherImageWindow()
        {
            foreach (var item in Application.Current.MainWindow.OwnedWindows)
            {
                if (item is ImageWindow)
                {
                    (item as ImageWindow).Close();
                }
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            openFileDialog.Filter = "图片文件|*.jpg;*.png;*.bmp";
            
            if(openFileDialog.ShowDialog() == true)
            {
                CloseOtherImageWindow();
                ImageWindow imageWindow = new ImageWindow(RefreshBackgroundList);
                imageWindow.Owner = Application.Current.MainWindow;
                imageWindow.SetLocalImage(openFileDialog.FileName, CurrentBackground);
                imageWindow.Show();
            }
        }
    }
}
