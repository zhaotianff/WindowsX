using Master_Zhao.Web.Model;
using Master_Zhao.Web.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Web.CnWeather
{
    public class IpCityAPI
    {
        private const string IP_URL = "https://www.ipshudi.com";
        private const string IP_CITY_URL = "https://www.ipshudi.com/{0}.htm";
        private const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36";
        private const string ACCEPT = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";

        public async static Task<string> GetIP()
        {
            var html = await WebHelper.GetHtmlSource(IP_URL, ACCEPT, USER_AGENT);
            var inputElement = HtmlAgilityPackUtil.XPathQuerySingle(html, "//input");
            return inputElement.GetAttributeValue("value","");
        }

        public async static Task<IpCityInfo> GetCurrentIpCityInfo(string ipAddress)
        {         
            var url = string.Format(IP_CITY_URL, ipAddress);
            var html = await WebHelper.GetHtmlSource(url, ACCEPT, USER_AGENT);

            if (string.IsNullOrEmpty(html))
                return null;

            return ParseIpCityInfo(html,ipAddress);
        }

        private static IpCityInfo ParseIpCityInfo(string html,string ip)
        {
            var tableElement = HtmlAgilityPackUtil.XPathQuerySingle(html, "//table/tbody/tr/td/span");
            var ipCityInfo = new IpCityInfo();
            ipCityInfo.Ip = ip;
            var spanText = tableElement.InnerText;
            if (string.IsNullOrEmpty(spanText) || spanText.Length <= 2)
                return null;

            ipCityInfo.City = spanText.Split(" ");
            return ipCityInfo;
        }
    }
}
