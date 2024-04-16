using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowsX.Shell.PInvoke
{
    public class PowerTool
    {
        [DllImport("WindowsXCore.dll")]
        public static extern void ShowShutDownDialog();
        [DllImport("WindowsXCore.dll")]
        public static extern void Logoff();

        [DllImport("WindowsXCore.dll")]
        public static extern void ShutdownComputer();

        [DllImport("WindowsXCore.dll")]
        public static extern void SwitchUser();

        [DllImport("WindowsXCore.dll")]
        public static extern void LockComputer();

        [DllImport("WindowsXCore.dll")]
        public static extern void RestartComputer();

        [DllImport("WindowsXCore.dll")]
        public static extern void SleepComputer();
        [DllImport("WindowsXCore.dll")]
        public static extern void ShowRunDialog();
    }
}
