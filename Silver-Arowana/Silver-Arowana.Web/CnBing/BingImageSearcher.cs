using Silver_Arowana.Web.Interface;
using Silver_Arowana.Web.Model;
using Silver_Arowana.Web.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silver_Arowana.Web.CnBing
{
    public class BingImageSearcher : IImageSearcher
    {
        private const string ApiUrl = "https://cn.bing.com/images/api/custom/search?q=[keyword]%201920x1080&count=25&offset=[start]&skey=x0N3tvnBi4Or09GnYTSGBz-1Y_hczfN8mDl9KUAupCo&safeSearch=Strict&mkt=zh-cn&setLang=zh-cn&IG=095F7668227148A5BE61ABD2FCF8DA04&IID=idpfs&SFX=2";
        private const int CountPerPage = 25;

        public async Task<List<ITagImg>> SearchImageAsync(string keyword, int page = 1)
        {
            var start = (page - 1) * CountPerPage;
            var url = ApiUrl.Replace("[keyword]", keyword).Replace("[start]", start.ToString());
            var html = await GetRawSourceAsync(url);
            var list =  await ExtractImageListAsync(html);
            return list;
        }

        private async Task<string> GetRawSourceAsync(string url)
        {
            var accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            var userAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36 TheWorld 7";
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
