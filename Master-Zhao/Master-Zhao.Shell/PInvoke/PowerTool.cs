using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Master_Zhao.Shell.PInvoke
{
    public class PowerTool
    {
        [DllImport("MasterZhaoCore.dll")]
        public static extern void ShowShutDownDialog();
        [DllImport("MasterZhaoCore.dll")]
        public static extern void Logoff();

        [DllImport("MasterZhaoCore.dll")]
        public static extern void ShutdownComputer();

        [DllImport("MasterZhaoCore.dll")]
        public static extern void SwitchUser();

        [DllImport("MasterZhaoCore.dll")]
        public static extern void LockComputer();

        [DllImport("MasterZhaoCore.dll")]
        public static extern void RestartComputer();

        [DllImport("MasterZhaoCore.dll")]
        public static extern void SleepComputer();
        [DllImport("MasterZhaoCore.dll")]
        public static extern void ShowRunDialog();
    }
}
