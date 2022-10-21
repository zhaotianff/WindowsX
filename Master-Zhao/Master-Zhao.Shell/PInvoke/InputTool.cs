using System;
using System.Runtime.InteropServices;

namespace Master_Zhao.Shell.PInvoke
{
    public class InputTool
    {
        public static readonly int VK_CONTROL = 0x11;
        public static readonly int VK_MENU = 0x12;

        [DllImport("MasterZhaoCore.dll")]
        public static extern int GetRawInput(IntPtr lParam);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool IsKeyPressed(int vKey);

        [DllImport("MasterZhaoCore.dll")]
        public static extern void  AutoCode([MarshalAs(UnmanagedType.LPWStr)]string code);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool SendUnicodeInput([MarshalAs(UnmanagedType.LPWStr)]char c);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool SendAsciiInput([MarshalAs(UnmanagedType.LPWStr)]char c);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool SendSearch();
    }
}
