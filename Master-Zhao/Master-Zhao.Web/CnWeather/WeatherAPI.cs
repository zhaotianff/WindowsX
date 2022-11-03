using Master_Zhao.Web.Model;
using Master_Zhao.Web.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Web.CnWeather
{
    public class WeatherAPI
    {
        private const string WEATHER_URL = "http://autodev.openspeech.cn/csp/api/v2.1/weather?openId=aiuicus&clientType=android&sign=android&city={0}&needMoreData=false&pageNo=1&pageSize=1";

        public static async Task<CityWeatherData> GetWeatherInfo(string cityName)
        {
            var url = string.Format(WEATHER_URL, cityName);
            var html = await WebHelper.GetHtmlSource(url);
            var root =  JsonHelper.Deserialize<WeatherInfoRoot>(html);
            if (root != null && root.data != null && root.data.list != null && root.data.list.Count > 0)
                return root.data.list[0];
            return null;
        }
    }
}
