using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace Master_Zhao.Shell.Infrastructure.Media
{
    public class ColorManager
    {
        public static readonly SolidColorBrush[] WorkTimeCountColors = 
        { 
            GetBrushFromHexString("#969EE8") ,
            GetBrushFromHexString("#C2A1E1") ,
            GetBrushFromHexString("#C2A1E1") ,
            GetBrushFromHexString("#F8AB83") ,
            GetBrushFromHexString("#17D183") ,
            GetBrushFromHexString("#034A77") ,
            GetBrushFromHexString("#769CC2") ,
            GetBrushFromHexString("#DB89D0") 
        };

        public static SolidColorBrush GetBrushFromHexString(string hexColor)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor));
        }
    }
}
