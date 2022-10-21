using Master_Zhao.Config.Model.StartMenu;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master_Zhao.Config.Model
{
    public class BackgroundSetting
    {
        public int Index { get; set; }

        public List<string> Colors { get; set; } = new List<string>();

        public List<string> ResourceImages { get; set; } = new List<string>();

        public List<string> LocalImages { get; set; } = new List<string>();

        public float Opacity { get; set; }
    }

    public class MainConfig
    {
        public BackgroundSetting BackgroundSetting { get; set; }

        public List<FlatStartMenuGroupedItem> FlatStartMenuGroupedItems { get; set; }
    }
}
