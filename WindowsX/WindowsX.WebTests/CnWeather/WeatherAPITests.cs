using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsX.Web.CnWeather;
using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsX.Web.CnWeather.Tests
{
    [TestClass()]
    public class WeatherAPITests
    {
        [TestMethod()]
        public void GetWeatherInfoTest()
        {
            var weatherInfo = WeatherAPI.GetWeatherInfoAsync("深圳").Result;
            Assert.IsNotNull(weatherInfo.weather);
        }
    }
}