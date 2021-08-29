using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Silver_Arowana.Shell.PInvoke
{
    public class DesktopTool
    {
        public static readonly int MAX_PATH = 260;

        [DllImport("SilverArowanaCore.dll")]
        public static extern bool GetBackground([MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpImagePath);

        [DllImport("SilverArowanaCore.dll")]
        public static extern bool SetBackground(string lpImagePath);
    }
}
