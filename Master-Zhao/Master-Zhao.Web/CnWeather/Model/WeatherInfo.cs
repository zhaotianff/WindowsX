using System;
using System.Collections.Generic;
using System.Text;

namespace Master_Zhao.Web.Model
{
    public class CityWeatherData
    {
        public string city { get; set; }

        public string lastUpdateTime { get; set; }

        public string date { get; set; }

        public string weather { get; set; }

        public float temp { get; set; }

        public string humidity { get; set; }

        public string wind { get; set; }

        public float pm25 { get; set; }

        public float pm10 { get; set; }

        public float low { get; set; }

        public float high { get; set; }

        public string airData { get; set; }

        public string airQuality { get; set; }

        public string dateLong { get; set; }

        public int weatherType { get; set; }

        public int windLevel { get; set; }

        public string province { get; set; }
    }

    public class WeatherData
    {
        public int total { get; set; }

        public string sourceName { get; set; }

        public List<CityWeatherData> list { get; set; }

        public string logoUrl { get; set; }
    }

    public class WeatherInfoRoot
    {
        public int code { get; set; }

        public string msg { get; set; }

        public WeatherData data { get; set; }
    }
}
