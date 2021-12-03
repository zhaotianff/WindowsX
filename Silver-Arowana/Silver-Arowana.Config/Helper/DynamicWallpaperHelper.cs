using Silver_Arowana.Config.Json;
using Silver_Arowana.Config.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Silver_Arowana.Config.Helper
{
    internal class DynamicWallpaperHelper
    {
        private static readonly string ListFileName = "dynamicwallpaper.json";
        private static readonly string ConfigDir = Path.Combine(Environment.CurrentDirectory, "config");
        private static readonly string ListFilePath = Path.Combine(Environment.CurrentDirectory, "config",ListFileName);

        private static bool InvalidateListFile()
        {   
            if (Directory.Exists(ConfigDir) == false)
            {
                Directory.CreateDirectory(ConfigDir);
                return false;
            }

            return File.Exists(ListFilePath);
        }

        public static DynamicWallpaperConfig LoadDynamicWallConfig()
        {
            try
            {
                if (InvalidateListFile() == false)
                    return new DynamicWallpaperConfig();
            
                var config = JsonUtil.DeserializeFile<DynamicWallpaperConfig>(ListFilePath);
                return config;
            }
            catch(Exception ex)
            {
                //TODO
                Trace.WriteLine(ex.Message);
            }
            return new DynamicWallpaperConfig();
        }

        public static void SaveDynamicWallList(DynamicWallpaperConfig config)
        {
            JsonUtil.Serialize(config, ListFilePath);
        }
    }
}
