using System;
using System.Collections.Generic;
using System.Text;

namespace Master_Zhao.Web.Model
{
    public class IpCityInfo
    {
        public static IpCityInfo DefaultIpCityInfo = new IpCityInfo() { City = new string[] { "广东", "深圳", "福田" } };

        public string Ip { get; set; }

        public string[] City { get; set; }

        public string GetCityString()
        {
            if (City.Length > 0)
                return City[City.Length - 1];

            return "深圳市";
        }
    }
}
