using WindowsX.Web.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Shell.Util
{
    public class WeatherHelper
    {
        private const string CLOUDY_IMAGE = "cloud.png";
        private const string DUST_IMAGE = "dust.png";
        private const string HEAVY_RAIN_IMAGE = "heavy_rain.png";
        private const string LIGHT_RAIN_IMAGE = "light_rain.png";
        private const string LIGHT_SNOW_IMAGE = "light_snow.png";
        private const string MOSTLY_SUNNY_IMAGE = "mostly_sun.png";
        private const string RAIN_IMAGE = "rain.png";
        private const string RAIN_CLOUD_IMAGE = "rain_cloud.png";
        private const string SLEET_IMAGE = "sleet.png";
        private const string SNOW_IMAGE = "snow.png";
        private const string SNOW_STORM_IMAGE = "snow_storm.png";
        private const string STORM_IMAGE = "storm.png";
        private const string SUN_IMAGE = "sun.png";

        public async static Task<CityWeatherData> GetWeatherDataAsync()
        {
            var weatherInfoList = await GetWeatherDataListAsync();
            if (weatherInfoList != null && weatherInfoList.Count > 0)
                return weatherInfoList[0];
            return null;
        }

        public async static Task<List<CityWeatherData>> GetWeatherDataListAsync()
        {
            var ip = await WindowsX.Web.CnWeather.IpCityAPI.GetIPAsync();
            var ipCityInfo = await WindowsX.Web.CnWeather.IpCityAPI.GetCurrentIpCityInfoAsync(ip);
            var weatherInfo = await WindowsX.Web.CnWeather.WeatherAPI.GetWeatherInfoListAsync(ipCityInfo.GetCityString());
            return weatherInfo;
        }

        public static string GetWeatherImage(string weatherName)
        {
            var relativePathPrefix = "../../Icon/Weather/";
            switch(weatherName)
            {
                case "阴":
                    return relativePathPrefix + CLOUDY_IMAGE;
                case "霾":
                    return relativePathPrefix + DUST_IMAGE;
                case "晴":
                    return relativePathPrefix + SUN_IMAGE;
                case "雨夹雪":
                    return SLEET_IMAGE;
                case "小雨":
                case "阵雨":
                    return LIGHT_RAIN_IMAGE;
                case "多云":
                    return MOSTLY_SUNNY_IMAGE;
                default:
                    return relativePathPrefix + SUN_IMAGE;
            }
        }
    }
}
