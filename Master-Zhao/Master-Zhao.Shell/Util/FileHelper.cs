using Master_Zhao.Shell.Model.SystemMgmt;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Master_Zhao.Shell.PInvoke.Kernel32;

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

        [Obsolete]
        public static long EnumerateSubDirectory(string dir, ObservableCollection<DiskPath> diskPathList)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(dir);

            DiskPath diskPath = new DiskPath();
            diskPath.DisplayName = directoryInfo.Name;
            diskPath.Path = directoryInfo.FullName;
            diskPath.Children = new ObservableCollection<DiskPath>();
            diskPathList.Add(diskPath);

            foreach (var directory in directoryInfo.GetDirectories())
            {
                EnumerateSubDirectory(directory.FullName, diskPath.Children);
            }

            return 0;
        }

        public static long GetRootSizeFromPath(string path)
        {
            if (string.IsNullOrEmpty(path) || path.Length < 3 || path[1] != ':')
                return 0;

            var diskName = path.Substring(0, 3);

            foreach (var driver in DriveInfo.GetDrives())
            {
                if (driver.Name == diskName)
                    return driver.TotalSize;
            }

            return 0;
        }
    }
}
