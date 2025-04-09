using WindowsX.Shell.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using WindowsX.Shell.Model.SystemMgmt;

namespace WindowsX.Shell.Infrastructure.Converter
{
    public class DiskSizeToPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var diskPath = value as DiskPath;

            if (diskPath == null)
                return 0f;

            if (diskPath.Parent == null)
                return 100f;

            return ((float)diskPath.Size / diskPath.Parent.Size) * 100;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
