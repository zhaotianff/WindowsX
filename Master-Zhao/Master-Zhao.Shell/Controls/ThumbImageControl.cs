using Master_Zhao.Shell.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Master_Zhao.Shell.Controls
{
    public class ThumbImageControl : Button
    {
        public static readonly DependencyProperty ThumbPathProperty = DependencyProperty.Register("ThumbPath", typeof(string),typeof(Button));
        public static readonly DependencyProperty ThumbImageObjProperty = DependencyProperty.Register("ThumbImageObj", typeof(BitmapImage), typeof(Button));
        public static readonly DependencyProperty ThumbImageWidthProperty = DependencyProperty.Register("ThumbImageWidth", typeof(int), typeof(Button),new PropertyMetadata(0));
        public static readonly DependencyProperty ThumbImageHeightProperty = DependencyProperty.Register("ThumbImageHeight", typeof(int), typeof(Button),new PropertyMetadata(0));

        static ThumbImageControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ThumbImageControl), new FrameworkPropertyMetadata(typeof(ThumbImageControl)));            
        }

        public string ThumbPath
        {
            get => GetValue(ThumbPathProperty).ToString();
            set
            {
                SetValue(ThumbPathProperty, value);

                if (!string.IsNullOrEmpty(value))
                {
                    ThumbImageObj = ImageHelper.GetBitmapImageFromLocalFile(value, true);

                    if (ThumbImageHeight != 0)
                    {
                        ThumbImageObj.DecodePixelHeight = ThumbImageHeight;
                    }

                    if (ThumbImageWidth != 0)
                    {
                        ThumbImageObj.DecodePixelWidth = ThumbImageWidth;
                    }
                }
            }
        }

        public BitmapImage ThumbImageObj
        {
            get => (BitmapImage)GetValue(ThumbImageObjProperty);
            private set => SetValue(ThumbImageObjProperty, value);
        }

        public int ThumbImageWidth
        {
            get => (int)GetValue(ThumbImageWidthProperty);
            set => SetValue(ThumbImageWidthProperty, value);
        }

        public int ThumbImageHeight
        {
            get => (int)GetValue(ThumbImageHeightProperty);
            set => SetValue(ThumbImageHeightProperty, value);
        }
    }
}
