using Master_Zhao.Config.Json;
using Master_Zhao.Config.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Config.Helper
{
    public class ToolsHelper
    {
        private static readonly string ListFileName = "tools.json";
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

        public static ToolsConfig LoadToolsConfig()
        {
            try
            {
                if (InvalidateListFile() == false)
                    return new ToolsConfig();

                var config = JsonUtil.DeserializeFile<ToolsConfig>(ListFilePath);
                return config;
            }
            catch (Exception ex)
            {
                //TODO
                Trace.WriteLine(ex.Message);
            }
            return new ToolsConfig();
        }

        public static void SaveToolsConfig(ToolsConfig config)
        {
            config.FastRunConfig = new FastRunConfig();
            config.FastRunConfig.FastRunList.Add(new FastRunItem() { Args = new string[] { "" }, HotKey = new int[] { 1, 2, 3 }, Name = "notepad", Path = "C:\\Windows\\System32\\notepad.exe", RunType = FastRunType.Applicataion });
            JsonUtil.Serialize(config, ListFilePath);
        }
    }
}
