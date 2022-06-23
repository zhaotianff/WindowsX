using System;
using System.Runtime.InteropServices;

namespace Master_Zhao.Shell.PInvoke
{
    public class InputTool
    {
        [DllImport("MasterZhaoCore.dll")]
        public static extern int GetRawInput(IntPtr lParam);
    }
}
