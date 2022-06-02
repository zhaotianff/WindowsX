using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Shell.PInvoke
{
    public class Kernel32
    {
        [DllImport("Kernel32.dll")]
        public static extern int GetLastError();
    }
}
