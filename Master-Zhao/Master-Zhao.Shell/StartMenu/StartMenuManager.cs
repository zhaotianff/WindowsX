using System;
using System.Windows.Interop;
using Master_Zhao.Shell.StartMenu.Win98;
using Master_Zhao.Shell.Util;

namespace Master_Zhao.Shell.StartMenu
{
    public class StartMenuManager
    {
        private static string previousStartMenuName = "";

        public static void LaunchStartMenu(string menuName)
        {
            if (menuName == previousStartMenuName)
                return;

            if (!string.IsNullOrEmpty(previousStartMenuName))
                StopStartMenu(menuName);

            var startMenuHandle = ProcessHelper.FindProcessWindow(menuName);
            if (startMenuHandle == IntPtr.Zero)
            {
                Windows98 windows98 = new Windows98();
                PInvoke.SystemTool.HookStart(new WindowInteropHelper(windows98).Handle);
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
                ProcessHelper.KillProcess(previousStartMenuName);
                PInvoke.SystemTool.UnHookStart();
            }
        }
    }
}
