using WindowsX.Config.Model.StartMenu;
using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsX.Config.Model
{
    public class BackgroundSetting
    {
        public int BackgroundIndex { get; set; }
        public int ThemeIndex { get; set; }

        public List<string> Colors { get; set; } = new List<string>();

        public List<string> ResourceImages { get; set; } = new List<string>();

        public List<string> LocalImages { get; set; } = new List<string>();

        public float Opacity { get; set; }

        /// <summary>
        /// reserved
        /// </summary>
        public bool IsBlurBackground { get; set; }
    }

    public class MainConfig
    {
        public BackgroundSetting BackgroundSetting { get; set; }

        public List<FlatStartMenuGroupedItem> FlatStartMenuGroupedItems { get; set; }

        public List<TodoDateItem> FlatStartMenuToDoList { get; set; }
    }
}
