using Master_Zhao.Web.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Shell.Util
{
    public class WeatherHelper
    {
        private const string CLOUDY_IMAGE = "Cloudy.png";
        private const string LIGHT_RAIN_IMAGE = "LightRain.png";
        private const string MOSTLY_SUNNY_IMAGE = "MostlySunny.png";
        private const string SUNNY_IMAGE = "Sunny.png";
        private const string THUNDER_STORM_IMAGE = "Thunderstorm.png";

        public async static Task<CityWeatherData> GetWeatherDataAsync()
        {
            var ip = await Master_Zhao.Web.CnWeather.IpCityAPI.GetIP();
            var ipCityInfo = await Master_Zhao.Web.CnWeather.IpCityAPI.GetCurrentIpCityInfo(ip);
            var weatherInfo = await Master_Zhao.Web.CnWeather.WeatherAPI.GetWeatherInfo(ipCityInfo.GetCityString());
            return weatherInfo;
        }

        public static string GetWeatherImage(string weatherName)
        {
            var relativePathPrefix = "../../Icon/Weather/";
            switch(weatherName)
            {
                case "阴":
                    return relativePathPrefix + CLOUDY_IMAGE;
                default:
                    return relativePathPrefix + SUNNY_IMAGE;
            }
        }
    }
}
