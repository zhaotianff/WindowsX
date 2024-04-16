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

namespace WindowsX.Shell.Controls
{
    public class ThumbImageControl : Button
    {
        public static readonly DependencyProperty ThumbPathProperty = DependencyProperty.Register("ThumbPath", typeof(string), typeof(Button));
        public static readonly DependencyProperty ThumbImageObjProperty = DependencyProperty.Register("ThumbImageObj", typeof(BitmapImage), typeof(Button));

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
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    var buffer = System.IO.File.ReadAllBytes(value);
                    System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer);
                    bi.StreamSource = ms;
                    bi.EndInit();
                    ThumbImageObj = bi;
                }
            }
        }

        public BitmapImage ThumbImageObj
        {
            get => (BitmapImage)GetValue(ThumbImageObjProperty);
            private set => SetValue(ThumbImageObjProperty, value);
        }

        public string FilePath { get; set; }
    }
}
