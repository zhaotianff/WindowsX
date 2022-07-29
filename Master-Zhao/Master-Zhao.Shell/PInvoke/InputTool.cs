using System;
using System.Runtime.InteropServices;

namespace Master_Zhao.Shell.PInvoke
{
    public class InputTool
    {
        public static readonly int VK_MENU = 0x12;

        [DllImport("MasterZhaoCore.dll")]
        public static extern int GetRawInput(IntPtr lParam);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool IsKeyPressed(int vKey);
    }
}
