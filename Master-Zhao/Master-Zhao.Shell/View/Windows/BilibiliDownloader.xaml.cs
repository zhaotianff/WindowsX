using Master_Zhao.Web.Bilibili.API;
using Master_Zhao.Web.Bilibili.Model;
using Master_Zhao.Web.Util;
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
using System.Windows.Shapes;

namespace Master_Zhao.Shell.Windows
{
    /// <summary>
    /// Interaction logic for BilibiliDownloader.xaml
    /// </summary>
    public partial class BilibiliDownloader : TianXiaTech.BlurWindow
    {
        public BilibiliDownloader()
        {
            InitializeComponent();
        }

        private async void btnParse_Click(object sender, RoutedEventArgs e)
        {
            var url = this.tbox_Url.Text;

            if(string.IsNullOrEmpty(url))
            {
                MessageBox.Show("请输入链接");
                return;
            }

            var videoInfo = await GetVideoInfo(url);
            SetVideoInfo(videoInfo);
        }

        private void SetVideoInfo(VideoInfo videoInfo)
        {
            this.img_album.Source = new BitmapImage(new Uri(videoInfo.data.pic));
        }

        private async  Task<VideoInfo> GetVideoInfo(string url)
        {
            //https://www.bilibili.com/video/BV1Mt4y1e7JY
            var bvid = GetBvid(url);
            url = string.Format(ApiCollection.GetAlbumApi,bvid);
            var accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            var user_agent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36";
            var html = await WebHelper.GetHtmlSource(url,accept,user_agent);
            var videoInfo = JsonHelper.Deserialize<VideoInfo>(html);
            return videoInfo;
        }

        private string GetBvid(string url)
        {
            if (url.Contains("/") == false)
                return url;

            var index = url.LastIndexOf("/");
            return url.Substring(index + 1);
        }
    }
}
