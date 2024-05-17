using System;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowsX.Shell.PInvoke
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

        [DllImport("WindowsXCore.dll")]
        public static extern bool GetBackground([MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpImagePath);

        [DllImport("WindowsXCore.dll")]
        public static extern bool SetBackground([MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpImagePath);

        [DllImport("WindowsXCore.dll")]
        public static extern bool GetRecentBackground([MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpRecentPath);

        [DllImport("WindowsXCore.dll")]
        public static extern bool SwitchToDesktop();//SwitchToWindow

        [DllImport("WindowsXCore.dll")]
        public static extern bool SwitchToWindow(IntPtr intPtr);

        [DllImport("WindowsXCore.dll")]
        public static extern bool EmbedWindowToDesktop([MarshalAs(UnmanagedType.LPWStr)]string lpWindowName);

        [DllImport("WindowsXCore.dll")]
        public static extern bool CloseEmbedWindow();

        [DllImport("WindowsXCore.dll")]
        public static extern IntPtr GetFileThumbnail([MarshalAs(UnmanagedType.LPWStr)] string path);

        [DllImport("WindowsXCore.dll")]
        public static extern bool BlurTaskbar(ACCENT_POLICY accent_policy);

        [DllImport("WindowsXCore.dll")]
        public static extern bool CenterStartMenu(bool enable);

        [DllImport("WindowsXCore.dll")]
        public static extern bool CenterTaskListIcon(bool enable);

        [DllImport("WindowsXCore.dll")]
        public static extern bool SetBackgroundPosition(DESKTOP_WALLPAPER_POSITION pos);

        [DllImport("WindowsXCore.dll")]
        public static extern bool GetBackgroundPosition(ref DESKTOP_WALLPAPER_POSITION pos);

        [DllImport("WindowsXCore.dll")]
        public static extern void SetDesktopIcon(DESKTOPICONS desktopIcon, bool isEnable);

        [DllImport("WindowsXCore.dll")]
        public static extern bool GetDesktopIconState(DESKTOPICONS desktopIcon);

        [DllImport("WindowsXCore.dll")]
        public static extern void RefreshDesktop();

        [DllImport("WindowsXCore.dll")]
        public static extern bool CreateLink([MarshalAs(UnmanagedType.LPWStr)] string lpszPathObj,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszPathLink,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszArgs,
            [MarshalAs(UnmanagedType.LPWStr)] string lpszDesc);

        [DllImport("WindowsXCore.dll")]
        public static extern bool GetLink([MarshalAs(UnmanagedType.LPWStr)]string lpszLinkFile, [MarshalAs(UnmanagedType.LPWStr)]StringBuilder lpszPath, int iPathBufferSize);

        [DllImport("WindowsXCore.dll")]
        public static extern bool GetGodModeShortCutState();

        [DllImport("WindowsXCore.dll")]
        public static extern bool CreateGodModeShortCut();

        [DllImport("WindowsXCore.dll")]
        public static extern bool RemoveGodModeShortCut();

        [DllImport("WindowsXCore.dll")]
        public static extern bool GetShortcutArrowState();

        [DllImport("WindowsXCore.dll")]
        public static extern bool RemoveShortcutArrow();

        [DllImport("WindowsXCore.dll")]
        public static extern bool RestoreShortcutArrow();

        [DllImport("WindowsXCore.dll")]
        public static extern bool GetWindowsPhotoViewerState();

        [DllImport("WindowsXCore.dll")]
        public static extern void RegisterWindowsPhotoViewerFormat();

        [DllImport("WindowsXCore.dll")]
        public static extern void UnregisterWindowsPhotoViewerFormat();

        [DllImport("WindowsXCore.dll")]
        public static extern bool GetPaintVersionState();

        [DllImport("WindowsXCore.dll")]
        public static extern bool PaintVersionInfo(bool enable);

        [DllImport("WindowsXCore.dll")]
        public static extern bool GetTaskbarThumbnailSize(ref int size);

        [DllImport("WindowsXCore.dll")]
        public static extern bool SetTaskbarThumbnailSize(int dwSize, bool bRestartExplorer);

        [DllImport("WindowsXCore.dll")]
        public static extern void ActivateTaskBar();

        [DllImport("WindowsXCore.dll")]
        public static extern bool CanAddToTaskBar(IntPtr hwnd);

        [DllImport("WindowsXCore.dll")]
        public static extern bool GetFileExtensionFriendlyName([MarshalAs(UnmanagedType.LPWStr)]string szExtension, IntPtr buffer, uint bufferSize);

        [DllImport("WindowsXCore.dll")]
        public static extern bool GetFileExtensionExecutablePath([MarshalAs(UnmanagedType.LPWStr)] string szExtension, IntPtr buffer, uint bufferSize);

        /// <summary>
        /// get process name
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        /// <remarks>why not return string ? https://stackoverflow.com/questions/370079/pinvoke-for-c-function-that-returns-char</remarks>
        [DllImport("WindowsXCore.dll")]
        public static extern IntPtr GetProcessNameFromHwnd(IntPtr hWnd);

        [DllImport("WindowsXCore.dll")]
        public static extern void RestartExplorer();

        [DllImport("WindowsXCore.dll")]
        public static extern void SelectFile([MarshalAs(UnmanagedType.LPWStr)]string lpszFile);

        [DllImport("WindowsXCore.dll")]
        public static extern void OpenFileProperty([MarshalAs(UnmanagedType.LPWStr)]string lpszFile);

        [DllImport("WindowsXCore.dll")]
        public static extern void OpenWindowsHelp();

        [DllImport("WindowsXCore.dll")]
        public static extern void OpenRunDialog([MarshalAs(UnmanagedType.LPWStr)]string command);

        [DllImport("WindowsXCore.dll")]
        public static extern void OpenSearchWindow([MarshalAs(UnmanagedType.LPWStr)] string inputs);
    }
}
