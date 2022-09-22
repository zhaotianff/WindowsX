using Master_Zhao.Config.Json;
using Master_Zhao.Config.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Config.Helper
{
    public class StaticWallpaperHelper
    {
        private static readonly string ListFileName = "staticwallpaper.json";
        private static readonly string ConfigDir = Path.Combine(Environment.CurrentDirectory, "config");
        private static readonly string ListFilePath = Path.Combine(Environment.CurrentDirectory, "config", ListFileName);

        private static bool InvalidateListFile()
        {
            if (Directory.Exists(ConfigDir) == false)
            {
                Directory.CreateDirectory(ConfigDir);
                return false;
            }

            return File.Exists(ListFilePath);
        }

        public static StaticWallpaperConfig LoadStaticWallpaperConfig()
        {
            try
            {
                if (InvalidateListFile() == false)
                    return new StaticWallpaperConfig();

                var config = JsonUtil.DeserializeFile<StaticWallpaperConfig>(ListFilePath);
                config.DownloadPath = Path.Combine(Environment.CurrentDirectory, config.DownloadPath);
                return config;
            }
            catch
            {
                return new StaticWallpaperConfig();
            }
        }
    }
}
