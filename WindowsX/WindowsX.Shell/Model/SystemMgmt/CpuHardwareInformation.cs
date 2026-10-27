using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Shell.Model.SystemMgmt
{
    public class CpuHardwareInformation
    {
        public Dictionary<string, string> Details { get; } = new Dictionary<string, string>();

        public string Warning { get; set; }

        public bool IsAvailable => Details.Count > 0;
    }
}
