using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Master_Zhao.Shell.PInvoke
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct tagSTARTUPITEM
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szPath;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDescription;
        IntPtr hKey;
    }
    

    public class StartupTool
    {
        [DllImport("MasterZhaoCore.dll")]
        public static extern bool CreateStartupRun(string lpszPath);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool RemoveStartupRun(string lpszPath);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool GetStartupItems(IntPtr buffer, int nSizeTarget, ref int count);
    }
}
