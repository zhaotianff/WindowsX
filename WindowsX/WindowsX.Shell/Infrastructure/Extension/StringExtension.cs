using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Shell.Infrastructure.Extension
{
    public static class StringExtension
    {
        public static uint ToUint32(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0;

            uint num;
            uint.TryParse(str, out num);
            return num;
        }
    }
}
