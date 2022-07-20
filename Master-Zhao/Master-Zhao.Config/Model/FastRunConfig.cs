using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Config.Model
{
    public enum FastRunType
    {
        Applicataion,
        ControlPanel,
        Setting
    }

    public class FastRunConfig
    {
        public List<FastRunItem> FastRunList { get; set; } = new List<FastRunItem>();
    }

    public class FastRunItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public byte[] Icon { get; set; }
        public string[] Args { get; set; }
        public FastRunType RunType { get; set; }
        public int[] HotKey { get; set; }
    }
}
