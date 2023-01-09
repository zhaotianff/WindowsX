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
        public IntPtr hKey;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szRegPath;
        public uint samDesired;
        public bool bEnabled;
        public int type;
    }
    

    public class StartupTool
    {
        [DllImport("MasterZhaoCore.dll")]
        public static extern bool CreateStartupRun(string lpszPath);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool RemoveStartupRun(string lpszPath);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool GetStartupItems(IntPtr buffer, int nSizeTarget, ref int count);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool DisableStartupItem(IntPtr hKey, string szRegPath, uint samDesired, string szName, string szPath);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool EnableStartupItem(IntPtr hKey, string szRegPath, uint samDesired, string szName, string szPath);
    }
}
