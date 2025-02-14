using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WindowsX.Shell.PInvoke
{
    public struct POINT
    {
        public int x;
        public int y;
    }

    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    public class User32
    {
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int GWL_EXSTYLE = (-20);
        public const int GWL_STYLE = (-16);
        public const int WS_VISIBLE = 0x10000000;

        public const int WM_INPUT = 0x00FF;
        public const int WM_HOTKEY = 0x0312;
        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_NCMOUSEMOVE = 0x00A0;

        [DllImport("User32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("User32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("User32.dll")]
        public static extern int GetCursorPos(ref POINT point);

        [DllImport("User32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("User32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetRawInputData(IntPtr hRawInput, uint uiCommand, byte[] pData, ref int pcbSize, int cbSizeHeader);

        [DllImport("User32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd,int nCmdShow);

        public const int SW_SHOW = 5;
        public const int SW_HIDE = 0;

        public const uint RID_INPUT = 0x10000003;
        public const uint RIM_TYPEMOUSE = 0;

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUTHEADER
        {
            public uint dwType;
            public uint dwSize;
            public IntPtr hDevice;
            public IntPtr wParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWMOUSE
        {
            public uint ulFlags;
            public uint ulButtons;
            public uint ulButtonData;
            public uint ulRawButtons;
            public int lLastX;
            public int lLastY;
            public uint ulExtraInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RAWINPUT
        {
            public RAWINPUTHEADER header;
            public RAWMOUSE mouse;
        }

        public static RAWINPUT GetRawInputFromBuffer(byte[] buffer)
        {
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                return (RAWINPUT)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(RAWINPUT));
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
