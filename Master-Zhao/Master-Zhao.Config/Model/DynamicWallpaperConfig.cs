using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Config.Model
{
    public class DynamicWallpaperItem
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public string Thumbnail { get; set; }
    }

    public class DynamicWallpaperConfig
    {
        public bool Mute { get; set; } = true;
        public bool Repeat { get; set; } = true;
        public List<DynamicWallpaperItem> WallpaperList { get; set; } = new List<DynamicWallpaperItem>();
    }
}
