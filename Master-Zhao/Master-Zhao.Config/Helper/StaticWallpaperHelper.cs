using Master_Zhao.Config.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Config.Helper
{
    public class StaticWallpaperHelper
    {
        public static StaticWallpaperConfig LoadStaticWallpaperConfig()
        {
            StaticWallpaperConfig config = new StaticWallpaperConfig();
            config.DownloadPath = System.IO.Path.Combine(Environment.CurrentDirectory, "res\\static wallpaper\\download");
            return config;
        }
    }
}
