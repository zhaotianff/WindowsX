using System;
using System.Windows.Interop;
using WindowsX.Shell.StartMenu.Win98;
using WindowsX.Shell.StartMenu.WinFlat;
using WindowsX.Shell.Util;

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
                windows98.Show();
                hookFlag = PInvoke.SystemTool.HookStart(new WindowInteropHelper(windows98).Handle);
                HideStartMenu();
            }
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
