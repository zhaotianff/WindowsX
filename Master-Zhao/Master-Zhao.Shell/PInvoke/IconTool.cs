using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Master_Zhao.Shell.PInvoke
{
    public class IconTool
    {
        [DllImport("MasterZhaoCore.dll")]
        public static extern bool ExtractIconFromFile([MarshalAs(UnmanagedType.LPWStr)]string lpszExtPath, int nIconIndex, IntPtr buffer, int length);
      
        [DllImport("MasterZhaoCore.dll")]
        public static extern bool  ExtractFirstIconFromFile([MarshalAs(UnmanagedType.LPWStr)] string lpszExtPath, bool isLargeIcon, ref IntPtr icon);
    }
}
