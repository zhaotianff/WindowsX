using System;
using System.Runtime.InteropServices;

namespace WindowsX.Shell.PInvoke
{
    public class InputTool
    {
        public static readonly int VK_CONTROL = 0x11;
        public static readonly int VK_MENU = 0x12;
        public static readonly int VK_TAB = 0x09;

        [DllImport("WindowsXCore.dll")]
        public static extern int GetRawInput(IntPtr lParam);

        [DllImport("WindowsXCore.dll")]
        public static extern bool IsKeyPressed(int vKey);

        [DllImport("WindowsXCore.dll")]
        public static extern void  AutoCode([MarshalAs(UnmanagedType.LPWStr)]string code);

        [DllImport("WindowsXCore.dll")]
        public static extern bool SendUnicodeInput([MarshalAs(UnmanagedType.LPWStr)]char c);

        [DllImport("WindowsXCore.dll")]
        public static extern bool SendAsciiInput([MarshalAs(UnmanagedType.LPWStr)]char c);

        [DllImport("WindowsXCore.dll")]
        public static extern bool SendSearch();
    }
}
