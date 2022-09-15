using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Shell.PInvoke
{
    public class SystemTool
    {
        public static readonly uint MOD_ALT = 0x0001;
        public static readonly uint MOD_CONTROL =0x0002;

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool IsWindows10();

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool IsWindows11();

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool IsWindows10OrHigher();

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool RegisterFastRunHotKey(IntPtr hwnd); 

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool UnRegisterFastRunHotKey();

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool HookStart(IntPtr hwnd);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool UnHookStart();

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool ShowCustomStart();

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool HideCustomStart();
    }
}
