using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Shell.PInvoke
{
    public class StartupTool
    {
        [DllImport("MasterZhaoCore.dll")]
        public static extern bool CreateStartupRun(string lpszPath);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool RemoveStartupRun(string lpszPath);
    }
}
