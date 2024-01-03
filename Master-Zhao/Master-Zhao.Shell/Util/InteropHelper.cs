using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Shell.Util
{
    public static class InteropHelper
    {
        public static T PtrToStructure<T>(this IntPtr ptr) where T:struct
        {
            if (ptr == IntPtr.Zero)
                return default(T);

            return Marshal.PtrToStructure<T>(ptr);
        }
    }
}
