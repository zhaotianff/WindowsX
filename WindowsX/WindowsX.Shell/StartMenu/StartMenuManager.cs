using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using WindowsX.Shell.PInvoke;
using WindowsX.Shell.StartMenu.Win98;
using WindowsX.Shell.StartMenu.WinFlat;
using WindowsX.Shell.Util;
using static WindowsX.Shell.PInvoke.User32;

namespace WindowsX.Shell.StartMenu
{
    public class StartMenuManager
    {
        private const string DEFAULT_MENU_NAME = "Default";
        private const string Win98_MENU_NAME = "Windows98";
        private const string WINFLAT_MENU_NAME = "WindowsFlat";

        private static string previousStartMenuName = DEFAULT_MENU_NAME;
        private static bool hookFlag = false;

        public static void LaunchStartMenu(string menuName)
        {
            if (menuName == previousStartMenuName)
                return;

            ClosePreviousStartMenu();

            switch (menuName)
            {
                case DEFAULT_MENU_NAME:
                    SetDefaultStartMenu();
                    break;
                case Win98_MENU_NAME:
                    SetWindows98StartMenu();
                    break;
                case WINFLAT_MENU_NAME:
                    SetWindowsFlatStartMenu();
                    break;
            }

            previousStartMenuName = menuName;
        }


        private static void SetDefaultStartMenu()
        {
            PInvoke.SystemTool.UnHookStart();
        }

        private static void SetWindows98StartMenu()
        {
            var startMenuHandle = ProcessHelper.FindProcessWindow(Win98_MENU_NAME);
            if (startMenuHandle == IntPtr.Zero)
            {
                Windows98 windows98 = new Windows98();
                windows98.Loaded += Windows98_SourceInitialized;
                windows98.Show();
                hookFlag = PInvoke.SystemTool.HookStart(new WindowInteropHelper(windows98).Handle);
                HideStartMenu();
            }
        }

        private static void Windows98_SourceInitialized(object sender, EventArgs e)
        {
            HwndSource.FromHwnd(new WindowInteropHelper(sender as Windows98).Handle).AddHook(Window98Hook);
        }

        private static IntPtr Window98Hook (IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case PInvoke.User32.WM_INPUT:
                    {
                        int dwSize = 0;
                        User32.GetRawInputData(lParam, User32.RID_INPUT, null, ref dwSize, Marshal.SizeOf(typeof(User32.RAWINPUTHEADER)));
                        byte[] buffer = new byte[dwSize];
                        if (User32.GetRawInputData(lParam, User32.RID_INPUT, buffer, ref dwSize, Marshal.SizeOf(typeof(User32.RAWINPUTHEADER))) == dwSize)
                        {
                            User32.RAWINPUT raw = User32.GetRawInputFromBuffer(buffer);

                            if (raw.header.dwType == User32.RIM_TYPEMOUSE)
                            {
                                if ((raw.mouse.ulButtons & 0x0001) == 0x0001)
                                {
                                    PInvoke.POINT pOINT = new POINT();
                                    User32.GetCursorPos(ref pOINT);

                                    PInvoke.RECT rECT = new RECT();
                                    User32.GetWindowRect(hwnd, ref rECT);

                                    if (pOINT.x < rECT.left || pOINT.x > rECT.left + rECT.right
                                        || pOINT.y < rECT.top || pOINT.y > rECT.top + rECT.bottom)
                                    {
                                        if ((User32.GetWindowLong(hwnd, User32.GWL_STYLE) & User32.WS_VISIBLE) != 0)
                                        {
                                            SystemTool.HideCustomStart();
                                        }
                                    }

                                }
                            }
                        }
                    }
                    break;
                    
            }
            return IntPtr.Zero;
        }

        private static void SetWindowsFlatStartMenu()
        {
            var startMenuHandle = ProcessHelper.FindProcessWindow(Win98_MENU_NAME);
            if (startMenuHandle == IntPtr.Zero)
            {
                WindowsFlat windowsFlat = new WindowsFlat();
                windowsFlat.Show();
                hookFlag = PInvoke.SystemTool.HookStart(new WindowInteropHelper(windowsFlat).Handle);
                HideStartMenu();
            }
        }

        public static void ClosePreviousStartMenu()
        {
            if (!string.IsNullOrEmpty(previousStartMenuName) && previousStartMenuName != DEFAULT_MENU_NAME)
                PInvoke.SystemTool.CloseCustomStart();
        }

        public static void UnHookStart()
        {
            if(hookFlag)
            {
                PInvoke.SystemTool.CloseCustomStart();
                PInvoke.SystemTool.UnHookStart();
            }

            hookFlag = false;
        }

        public static void ShowStartMenu()
        {
            PInvoke.SystemTool.ShowCustomStart();
        }

        public static void HideStartMenu()
        {
            PInvoke.SystemTool.HideCustomStart();
        }
    }
}
