using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Master_Zhao.Shell.PInvoke
{
    public class DesktopTool
    {
        public static readonly int MAX_PATH = 260;

        [DllImport("SilverArowanaCore.dll")]
        public static extern bool GetBackground([MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpImagePath);

        [DllImport("SilverArowanaCore.dll")]
        public static extern bool SetBackground([MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpImagePath);

        [DllImport("SilverArowanaCore.dll")]
        public static extern bool GetRecentBackground([MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpRecentPath);

        [DllImport("SilverArowanaCore.dll")]
        public static extern bool SwitchToDesktop();//SwitchToWindow

        [DllImport("SilverArowanaCore.dll")]
        public static extern bool SwitchToWindow(IntPtr intPtr);


        [DllImport("SilverArowanaCore.dll")]
        public static extern bool EmbedWindowToDesktop([MarshalAs(UnmanagedType.LPWStr)]string lpWindowName);


        [DllImport("SilverArowanaCore.dll")]
        public static extern bool CloseEmbedWindow();

        [DllImport("SilverArowanaCore.dll")]
        public static extern IntPtr GetFileThumbnail([MarshalAs(UnmanagedType.LPWStr)] string path);
    }
}
