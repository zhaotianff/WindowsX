using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Config.Model
{
    public enum FastRunType
    {
        Applicataion,
        ControlPanel,
        Setting
    }

    public class FastRunConfig
    {
        public List<FastRunConfigItem> FastRunList { get; set; } = new List<FastRunConfigItem>();
    }

    public class FastRunConfigItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string[] Args { get; set; }
        public FastRunType RunType { get; set; }
        public int[] HotKey { get; set; }
    }
}
