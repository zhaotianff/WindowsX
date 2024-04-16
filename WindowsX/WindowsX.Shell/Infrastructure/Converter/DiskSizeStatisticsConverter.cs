using WindowsX.Shell.Model.SystemMgmt;
using WindowsX.Shell.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WindowsX.Shell.Infrastructure.Converter
{
    public class DiskSizeStatisticsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is DiskPath diskPath)
            {
                if (diskPath.DiskPathType != DiskPathType.Folder)
                    return "";

                (var dirCount, var fileCount) = FileHelper.GetCurrentDirSizeStatistics(diskPath);
                return $"当前目录:   {dirCount} 文件夹 {fileCount} 文件";
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
