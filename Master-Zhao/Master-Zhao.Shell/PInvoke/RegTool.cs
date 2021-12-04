using System;
using System.Runtime.InteropServices;

namespace Master_Zhao.Shell.PInvoke
{
    public class RegTool
    {
        [DllImport("SilverArowanaCore.dll")]
        public static extern bool SetValue(IntPtr hKey, string lpSubKey, string lpValueName, byte[] value);
    }
}
