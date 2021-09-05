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

namespace Silver_Arowana.Shell.Controls
{
    public class ThumbImageControl : Button
    {
        public static readonly DependencyProperty ImagePathProperty = DependencyProperty.Register("ImagePath", typeof(string),typeof(Button));

        static ThumbImageControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ThumbImageControl), new FrameworkPropertyMetadata(typeof(ThumbImageControl)));            
        }

        public string ImagePath
        {
            get => GetValue(ImagePathProperty).ToString();
            set => SetValue(ImagePathProperty, value);
        }

        protected override void OnClick()
        {
            if (System.IO.File.Exists(ImagePath) == false) 
                return;
            //设置壁纸
            PInvoke.DesktopTool.SetBackground(ImagePath);
        }
    }
}
