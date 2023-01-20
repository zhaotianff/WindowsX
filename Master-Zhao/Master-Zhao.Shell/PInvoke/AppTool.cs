using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Master_Zhao.Shell.PInvoke
{
    public class AppTool
    {
        [DllImport("MasterZhaoCore.dll")]
        public static extern bool GetAppPath(IntPtr szBuffer, uint nSize);
    }
}
