using Microsoft.VisualStudio.TestTools.UnitTesting;
using Master_Zhao.Web.CnWeather;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master_Zhao.Web.CnWeather.Tests
{
    [TestClass()]
    public class WeatherAPITests
    {
        [TestMethod()]
        public void GetWeatherInfoTest()
        {
            WeatherAPI weatherAPI = new WeatherAPI();
            var weatherInfo = weatherAPI.GetWeatherInfo("深圳").Result;
            Assert.IsNotNull(weatherInfo.weather);
        }
    }
}