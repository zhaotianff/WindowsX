using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Config.Model
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
        public bool KeepWallpaper { get; set; } = false;
        public List<DynamicWallpaperItem> WallpaperList { get; set; } = new List<DynamicWallpaperItem>();
        public int Wallpaperindex { get; set; } = 0;
        public bool AutoRunWithStarup { get; set; } = false;
    }
}
