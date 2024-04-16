using System;
using System.Runtime.InteropServices;

namespace WindowsX.Shell.PInvoke
{
    public class RegTool
    {
        [DllImport("WindowsXCore.dll")]
        public static extern bool SetValue(IntPtr hKey, string lpSubKey, string lpValueName, byte[] value);
    }
}
