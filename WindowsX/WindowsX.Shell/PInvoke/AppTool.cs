using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowsX.Shell.PInvoke
{
    public class AppTool
    {
        [DllImport("WindowsXCore.dll")]
        public static extern bool GetAppPath(IntPtr szBuffer, uint nSize);
    }
}
