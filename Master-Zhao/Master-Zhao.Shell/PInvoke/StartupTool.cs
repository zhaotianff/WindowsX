using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Shell.PInvoke
{

    public struct tagSTARTUPITEM
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szPath;
    }
    

    public class StartupTool
    {
        [DllImport("MasterZhaoCore.dll")]
        public static extern bool CreateStartupRun(string lpszPath);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool RemoveStartupRun(string lpszPath);

        [DllImport("MasterZhaoCore.dll")]
        public static extern IntPtr GetStartupItems(ref int count);
    }
}
