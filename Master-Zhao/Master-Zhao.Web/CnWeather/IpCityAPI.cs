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

        public async static Task<string> GetIPAsync()
        {
            var html = await WebHelper.GetHtmlSource(IP_URL);
            var inputElement = HtmlAgilityPackUtil.XPathQuerySingle(html, "//input");
            return inputElement.GetAttributeValue("value","");
        }

        public async static Task<IpCityInfo> GetCurrentIpCityInfoAsync(string ipAddress)
        {         
            var url = string.Format(IP_CITY_URL, ipAddress);
            var html = await WebHelper.GetHtmlSource(url);

            if (string.IsNullOrEmpty(html))
                return null;

            return ParseIpCityInfo(html,ipAddress);
        }

        private static IpCityInfo ParseIpCityInfo(string html,string ip)
        {
            var tableElement = HtmlAgilityPackUtil.XPathQuerySingle(html, "//table/tbody/tr/td/span");

            if (tableElement == null)
                return IpCityInfo.DefaultIpCityInfo;

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
