using WindowsX.Web.Model;
using WindowsX.Web.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Web.CnWeather
{
    public class WeatherAPI
    {
        private const string WEATHER_URL = "http://autodev.openspeech.cn/csp/api/v2.1/weather?openId=aiuicus&clientType=android&sign=android&city={0}&needMoreData=false&pageNo=1&pageSize=4";

        public static async Task<List<CityWeatherData>> GetWeatherInfoListAsync(string cityName)
        {
            var url = string.Format(WEATHER_URL, cityName);
            var html = await WebHelper.GetHtmlSource(url);
            var root =  JsonHelper.Deserialize<WeatherInfoRoot>(html);
            if (root != null && root.data != null && root.data.list != null && root.data.list.Count > 0)
                return root.data.list;
            return null;
        }

        public static async Task<CityWeatherData> GetWeatherInfoAsync(string cityName)
        {
            var list = await GetWeatherInfoListAsync(cityName);

            if (list != null && list.Count > 0)
                return list[0];

            return null;
        }
    }
}
