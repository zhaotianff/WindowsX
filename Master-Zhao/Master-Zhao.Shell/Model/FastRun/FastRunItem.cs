using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Master_Zhao.Shell.Model.FastRun
{
    public class FastRunItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public byte[] Icon { get; set; }
        public string[] Args { get; set; }
        public FastRunType RunType { get; set; }
        public System.Windows.Input.Key[] HotKey { get; set; }
    }
}
