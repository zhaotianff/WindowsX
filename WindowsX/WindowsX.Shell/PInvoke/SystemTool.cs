using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Shell.PInvoke
{
    public class SystemTool
    {
        public static readonly uint MOD_ALT = 0x0001;
        public static readonly uint MOD_CONTROL =0x0002;

        [DllImport("WindowsXCore.dll")]
        public static extern bool IsWindows10();

        [DllImport("WindowsXCore.dll")]
        public static extern bool IsWindows11();

        [DllImport("WindowsXCore.dll")]
        public static extern bool IsWindows10OrHigher();

        [DllImport("WindowsXCore.dll")]
        public static extern bool RegisterFastRunHotKey(IntPtr hwnd); 

        [DllImport("WindowsXCore.dll")]
        public static extern bool UnRegisterFastRunHotKey();

        [DllImport("WindowsXCore.dll")]
        public static extern bool HookStart(IntPtr hwnd);

        [DllImport("WindowsXCore.dll")]
        public static extern bool UnHookStart();

        [DllImport("WindowsXCore.dll")]
        public static extern bool ShowCustomStart();

        [DllImport("WindowsXCore.dll")]
        public static extern bool HideCustomStart();

        [DllImport("WindowsXCore.dll")]
        public static extern void CloseCustomStart();

        [DllImport("WindowsXCore.dll")]
        public static extern bool RegisterBossKeyHotKey(IntPtr hwnd, uint modifier, uint vkCode,uint hotKeyId);

        [DllImport("WindowsXCore.dll")]
        public static extern bool UnRegisterBossKeyHotKey(IntPtr hwnd, uint hotKeyId);

        [DllImport("WindowsXCore.dll")]
        public static extern bool GetUserProfilePicturePath(IntPtr buf, uint nSize);
    }
}
