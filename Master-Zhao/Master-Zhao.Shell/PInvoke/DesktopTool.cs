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

        public static readonly int DEFAULT_TASKBAR_THUMBNAIL_SIZE = -1;

        public struct ACCENT_POLICY
        {
            public int AcentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        };

        public enum DESKTOP_WALLPAPER_POSITION : uint
        {
            DWPOS_CENTER = 0,
            DWPOS_TILE = 1,
            DWPOS_STRETCH = 2,
            DWPOS_FIT = 3,
            DWPOS_FILL = 4,
            DWPOS_SPAN = 5
        }

        public enum DESKTOPICONS : int
        {
            ICON_COMPUTER,
            ICON_USER,
            ICON_RECYCLE,
            ICON_CONTROL_PANEL,
            ICON_NETWORK
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

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool SetBackgroundPosition(DESKTOP_WALLPAPER_POSITION pos);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool GetBackgroundPosition(ref DESKTOP_WALLPAPER_POSITION pos);

        [DllImport("MasterZhaoCore.dll")]
        public static extern void SetDesktopIcon(DESKTOPICONS desktopIcon, bool isEnable);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool GetDesktopIconState(DESKTOPICONS desktopIcon);

        [DllImport("MasterZhaoCore.dll")]
        public static extern void RefreshDesktop();

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool CreateLink([MarshalAs(UnmanagedType.LPWStr)] string lpszPathObj,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszPathLink,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszArgs,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszDesc);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool GetGodModeShortCutState();

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool CreateGodModeShortCut();

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool RemoveGodModeShortCut();

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool GetShortcutArrowState();

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool RemoveShortcutArrow();

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool RestoreShortcutArrow();

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool GetWindowsPhotoViewerState();

        [DllImport("MasterZhaoCore.dll")]
        public static extern void RegisterWindowsPhotoViewerFormat();

        [DllImport("MasterZhaoCore.dll")]
        public static extern void UnregisterWindowsPhotoViewerFormat();

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool GetPaintVersionState();

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool PaintVersionInfo(bool enable);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool GetTaskbarThumbnailSize(ref int size);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool SetTaskbarThumbnailSize(int dwSize, bool bRestartExplorer);

        [DllImport("MasterZhaoCore.dll")]
        public static extern void ActivateTaskBar();

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool CanAddToTaskBar(IntPtr hwnd);

        /// <summary>
        /// get process name
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        /// <remarks>why not return string ? https://stackoverflow.com/questions/370079/pinvoke-for-c-function-that-returns-char</remarks>
        [DllImport("MasterZhaoCore.dll")]
        public static extern IntPtr GetProcessNameFomrHwnd(IntPtr hWnd);

        [DllImport("MasterZhaoCore.dll")]
        public static extern void RestartExplorer();

        [DllImport("MasterZhaoCore.dll")]
        public static extern void SelectFile([MarshalAs(UnmanagedType.LPWStr)]string lpszFile);

        [DllImport("MasterZhaoCore.dll")]
        public static extern void OpenFileProperty([MarshalAs(UnmanagedType.LPWStr)]string lpszFile);

        [DllImport("MasterZhaoCore.dll")]
        public static extern void OpenWindowsHelp();
    }
}
