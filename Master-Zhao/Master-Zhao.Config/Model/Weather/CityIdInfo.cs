using System;
using System.Collections.Generic;
using System.Text;

namespace Master_Zhao.Config.Model.Weather
{
    public class CityInfo
    {
        public List<CityNameId> CityList { get; set; }
    }

    public class CityNameId
    {
        public string Name { get; set; }

        public string ID { get; set; }
    }
}
