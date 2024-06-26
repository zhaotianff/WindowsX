﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace WindowsX.Shell.Infrastructure.Converter
{
    public class StartupItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch(parameter.ToString())
            {
                case "Name":
                    return "" + value;
                case "Path":
                    return "路径: " + value;
                case "Description":
                    return "描述: " + value;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
