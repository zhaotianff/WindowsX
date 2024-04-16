using WindowsX.Web.Interface;
using WindowsX.Web.Model;
using WindowsX.Web.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Web.CnBing
{
    public class BingImageSearcher : IImageSearcher
    {
        private const string ApiUrl = "https://cn.bing.com/images/async?q=[keyword]&first=[start]&count=35&relp=35&qft=+filterui%3aphoto-photo&scenario=ImageBasicHover&datsrc=N_I&layout=RowBased&mmasync=1&dgState=x*939_y*907_h*168_c*4_i*211_r*31&IG=765E054519674C8C861E4630A4BF2FC8&SFX=7&iid=images.5601";
        private const int CountPerPage = 25;

        public async Task<List<ITagImg>> SearchImageAsync(string keyword, int page = 1)
        {
            var start = 1;
            if (page > 1)
                start = page * CountPerPage + 1;
            var url = ApiUrl.Replace("[keyword]", keyword).Replace("[start]", start.ToString());
            var html = await GetRawSourceAsync(url);
            return await ExtractImageListAsync(html);
        }

        private async Task<string> GetRawSourceAsync(string url)
        {
            var accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            var userAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36 TheWorld 7";
            //TODO 解决404的问题
            var html = await WebHelper.GetHtmlSource(url, accept, userAgent, Encoding.UTF8);
            return html;
        }

        private async Task<List<ITagImg>> ExtractImageListAsync(string html)
        {
            Task<List<ITagImg>> task = Task.Run(() => {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);
                var imgList = doc.DocumentNode.SelectNodes("//img");
                var w = 0;
                var h = 0;
                HtmlAgilityPack.HtmlAttribute tempAttribute = null;

                List<ITagImg> list = new List<ITagImg>();
                foreach (var item in imgList)
                {
                    BingTagImage tagImg = new BingTagImage();
                    tempAttribute = item.Attributes["alt"];
                    tagImg.Alt = tempAttribute == null ? "" : tempAttribute.Value;
                    tempAttribute = item.Attributes["src"];
                    tagImg.Src = tempAttribute == null ? "" : tempAttribute.Value;

                    tempAttribute = item.Attributes["h"];
                    if (tempAttribute != null)
                    {
                        int.TryParse(tempAttribute.Value, out h);
                    }
                    tempAttribute = item.Attributes["w"];
                    if (tempAttribute != null)
                    {
                        int.TryParse(tempAttribute.Value, out w);
                    }
                    Tuple<bool, string> extractResult = RegexUtil.ExtractBingImage(item.ParentNode.ParentNode.OuterHtml);

                    if (extractResult.Item1 == true)
                    {
                        tagImg.DetailUrl = extractResult.Item2;
                        tagImg.Width = w;
                        tagImg.Height = h;
                        list.Add(tagImg);
                    }
                }
                return list;
            });
            return await task;
        }
    }
}
