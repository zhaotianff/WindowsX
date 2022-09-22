using Master_Zhao.Config.Json;
using Master_Zhao.Config.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master_Zhao.Config.Helper
{
    public class ConfigHelper
    {
        private static readonly string DynamicConfigFile = "dynamicwallpaper.json";
        private static readonly string StaticConfigFile = "staticwallpaper.json";
        private static readonly string ToolsConfigFile = "tools.json";
        private static readonly string MainConfigFile = "main.json";

        private static T LoadConfig<T>(string fileName)
        {
            var path = System.IO.Path.Combine(Environment.CurrentDirectory, "config", fileName);
            if (System.IO.File.Exists(path) == false)
                return default(T);

            return JsonUtil.DeserializeFile<T>(path);
        }

        private static bool SaveConfig<T>(T obj,string fileName)
        {
            var path = System.IO.Path.Combine(Environment.CurrentDirectory, "config");
            if (System.IO.Directory.Exists(path) == false)
                System.IO.Directory.CreateDirectory(path);

            path = System.IO.Path.Combine(path, fileName);

            try
            {
                JsonUtil.Serialize(obj, path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static DynamicWallpaperConfig LoadDynamicWallConfig()
        {
            return LoadConfig<DynamicWallpaperConfig>(DynamicConfigFile);
        }

        public static bool SaveDynamicWallConfig(DynamicWallpaperConfig dynamicWallpaperConfig)
        {
            return SaveConfig(dynamicWallpaperConfig, DynamicConfigFile);
        }

        public static StaticWallpaperConfig LoadStaticWallpaperConfig()
        {
            return LoadConfig<StaticWallpaperConfig>(StaticConfigFile);
        }

        public static bool SaveStaticWallpaperConfig(StaticWallpaperConfig staticWallpaperConfig)
        {
            return SaveConfig(staticWallpaperConfig,StaticConfigFile);
        }

        public static ToolsConfig LoadToolsConfig()
        {
            return LoadConfig<ToolsConfig>(ToolsConfigFile);
        }

        public static bool SaveToolsConfig(ToolsConfig toolsConfig)
        {
            return SaveConfig(toolsConfig,ToolsConfigFile);
        }

        public static MainConfig LoadMainConfig()
        {
            return LoadConfig<MainConfig>(MainConfigFile);
        }
        public static bool SaveMainConfig(MainConfig mainConfig)
        {
            return SaveConfig(mainConfig,MainConfigFile);
        }
    }
}
