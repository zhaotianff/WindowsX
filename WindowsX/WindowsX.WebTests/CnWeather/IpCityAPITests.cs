using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsX.Web.CnWeather;
using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsX.Web.CnWeather.Tests
{
    [TestClass()]
    public class IpCityAPITests
    {
        [TestMethod()]
        public void GetIPTest()
        {
            var ip = IpCityAPI.GetIPAsync().Result;
            Assert.IsTrue(!string.IsNullOrEmpty(ip));
        }

        [TestMethod()]
        public void GetCurrentIpCityInfoTest()
        {
            var ip = IpCityAPI.GetIPAsync().Result;
            var ipCityInfo = IpCityAPI.GetCurrentIpCityInfoAsync(ip).Result;

            Assert.IsTrue(ipCityInfo.City.Length > 0);
        }
    }
}