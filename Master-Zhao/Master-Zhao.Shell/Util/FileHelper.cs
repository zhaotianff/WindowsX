using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Shell.Util
{
    public class FileHelper
    {
        public static string GetSizeString(long size)
        {
            if (size < 1024)
                return size.ToString() + " 字节";
            else if (size >= 1024 && size < 1024 * 1024)
                return (size / 1024f).ToString("0.0") + " KB" + $"({size}字节)";
            else if (size >= 1024 * 1024 && size < 1024 * 1024 * 1024)
                return (size / 1024f / 1024f).ToString("0.0") + " MB" + $"({size}字节)";
            else
                return (size / 1024f / 1024f / 1024f).ToString("0.0") + " GB" + $"({size}字节)";
        }
    }
}
