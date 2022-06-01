using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Shell.PInvoke
{
    public struct POINT
    {
        public int x;
        public int y;
    }

    public class User32
    {
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int GWL_EXSTYLE = (-20);

        public const int WM_INPUT = 0x00FF;

        [DllImport("User32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("User32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("User32.dll")]
        public static extern int GetCursorPos(ref POINT point);
    }
}
