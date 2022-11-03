using System;
using System.Collections.Generic;
using System.Text;

namespace Master_Zhao.Web.Model
{
    public class IpCityInfo
    {
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
