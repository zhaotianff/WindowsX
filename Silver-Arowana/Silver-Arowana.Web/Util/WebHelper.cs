using Silver_Arowana.Web.CnBing;
using Silver_Arowana.Web.Model;
using Silver_Arowana.Web.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace Silver_Arowana.Web.Util
{
    public class WebHelper
    {
        private const string CNBingImageDetailUrl = "https://cn.bing.com/images/api/custom/search?q=[keyword]&count=25&offset=[start]&skey=x0N3tvnBi4Or09GnYTSGBz-1Y_hczfN8mDl9KUAupCo&safeSearch=Strict&mkt=zh-cn&setLang=zh-cn&IG=095F7668227148A5BE61ABD2FCF8DA04&IID=idpfs&SFX=2";

        public static async Task<string> GetHtmlSource(string url, string accept, string userAgent, Encoding encoding = null, CookieContainer cookieContainer = null)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((a, b, c, d) => { return true; });

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Method = "GET";
                request.Accept = accept;
                request.UserAgent = userAgent;
                if (cookieContainer != null)
                    request.CookieContainer = cookieContainer;

                using (WebResponse response = await request.GetResponseAsync())
                {
                    Encoding tempEncoding = Encoding.Default;

                    if (encoding == null)
                    {
                        tempEncoding = EncodingUtil.GetEncoding(url);
                    }
                    else
                    {
                        tempEncoding = encoding;
                    }

                    Stream stream = response.GetResponseStream();  

                    using (StreamReader sr = new StreamReader(stream, tempEncoding))
                    {
                        return sr.ReadToEnd();
                    }

                }
            }
            catch
            {
                throw;
            }
        }

        public async static Task<List<ITagImg>> GetBingImgFromHtmlAsync(string html)
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

        public async static Task<List<ITagImg>> SearchBingImage(string keyword,int pageImageNum, int page = 1)
        {
            List<ITagImg> searchImgList = new List<ITagImg>();
            var start = 1;
            if (page > 1)
                start = page * pageImageNum + 1;
            var url = CNBingImageDetailUrl.Replace("[keyword]", keyword).Replace("[start]", start.ToString());
            searchImgList = await GetBingImgFromUrlAsync(url);
            return searchImgList;
        }

        public async static Task<List<ITagImg>> GetBingImgFromUrlAsync(string url)
        {
            var accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            var userAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36 TheWorld 7";
            var html = await WebHelper.GetHtmlSource(url, accept, userAgent, Encoding.UTF8);
            return await GetBingImgFromHtmlAsync(html);
        }

        public static void SaveFileAsync(string url,string path)
        {
            try
            {
                System.Net.WebClient client = new WebClient();
                client.DownloadFileAsync(new Uri(url), path);
            }
            catch
            {
                //TODO
            }
        }
    }
}
