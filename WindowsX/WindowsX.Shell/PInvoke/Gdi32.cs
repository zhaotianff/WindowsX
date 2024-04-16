using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Shell.PInvoke
{
    public class Gdi32
    {
        [DllImport("Gdi32.dll")]
        public static extern bool DeleteObject(IntPtr ho);
    }
}
