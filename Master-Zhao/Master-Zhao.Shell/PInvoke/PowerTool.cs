using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Master_Zhao.Shell.PInvoke
{
    public class PowerTool
    {
        [DllImport("MasterZhaoCore.dll")]
        public static extern void Shutdown();

        [DllImport("MasterZhaoCore.dll")]
        public static extern void ShowShutDownDialog();

        [DllImport("MasterZhaoCore.dll")]
        public static extern void SwitchUser();

        [DllImport("MasterZhaoCore.dll")]
        public static extern void Logoff();

        [DllImport("MasterZhaoCore.dll")]
        public static extern void Lock();

        [DllImport("MasterZhaoCore.dll")]
        public static extern void Restart();

        [DllImport("MasterZhaoCore.dll")]
        public static extern void Sleep();
    }
}
