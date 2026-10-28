using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Shell.Model.SystemMgmt
{
    public class InstalledSoftwareResult
    {
        public List<InstalledSoftware> Items { get; set; } = new List<InstalledSoftware>();

        public List<string> Warnings { get; } = new List<string>();
    }
}
