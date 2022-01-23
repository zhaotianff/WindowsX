using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Shell.PInvoke
{
    public class SystemTool
    {
        [DllImport("MasterZhaoCore.dll")]
        public static extern bool IsWindows10();

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool IsWindows11();

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool IsWindows10OrHigher();
    }
}
