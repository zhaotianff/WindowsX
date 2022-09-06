using System;
using System.Windows.Interop;
using Master_Zhao.Shell.StartMenu.Win98;
using Master_Zhao.Shell.Util;

namespace Master_Zhao.Shell.StartMenu
{
    public class StartMenuManager
    {
        private static readonly string SYS_MENU_NAME = "系统默认";
        private static string previousStartMenuName = "";
        private static bool hookFlag = false;

        public static void LaunchStartMenu(string menuName)
        {
            if (menuName == previousStartMenuName)
                return;

            if (menuName == SYS_MENU_NAME)
            {
                SetDefaultStartMenu();
                return;
            }

            if (!string.IsNullOrEmpty(previousStartMenuName))
                StopStartMenu(menuName);

            var startMenuHandle = ProcessHelper.FindProcessWindow(menuName);
            if (startMenuHandle == IntPtr.Zero)
            {
                Windows98 windows98 = new Windows98();
                windows98.Show();
                hookFlag = PInvoke.SystemTool.HookStart(new WindowInteropHelper(windows98).Handle);
                HideStartMenu();
            }

            previousStartMenuName = menuName;
        }

        private static void StopStartMenu(string menuName)
        {
            ProcessHelper.KillProcess(menuName);
        }

        public static void SetDefaultStartMenu()
        {
            if(!string.IsNullOrEmpty(previousStartMenuName))
            {
                UnHookStart();
            }
        }

        public static void UnHookStart()
        {
            if(hookFlag)
            {
                ProcessHelper.KillProcess(previousStartMenuName);
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
