using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Master_Zhao.Shell.PInvoke
{
    public class DesktopTool
    {
        public static readonly int MAX_PATH = 260;

        public static readonly int ACCENT_DISABLED = 0;
        public static readonly int ACCENT_ENABLE_GRADIENT = 1;
        public static readonly int ACCENT_ENABLE_TRANSPARENTGRADIENT = 2;
        public static readonly int ACCENT_ENABLE_BLURBEHIND = 3;
        public static readonly int ACCENT_ENABLE_ACRYLICBLURBEHIND = 4;

        public struct ACCENT_POLICY
        {
            public int AcentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        };

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool GetBackground([MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpImagePath);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool SetBackground([MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpImagePath);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool GetRecentBackground([MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpRecentPath);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool SwitchToDesktop();//SwitchToWindow

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool SwitchToWindow(IntPtr intPtr);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool EmbedWindowToDesktop([MarshalAs(UnmanagedType.LPWStr)]string lpWindowName);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool CloseEmbedWindow();

        [DllImport("MasterZhaoCore.dll")]
        public static extern IntPtr GetFileThumbnail([MarshalAs(UnmanagedType.LPWStr)] string path);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool BlurTaskbar(ACCENT_POLICY accent_policy);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool CenterStartMenu(bool enable);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool CenterTaskListIcon(bool enable);
    }
}
