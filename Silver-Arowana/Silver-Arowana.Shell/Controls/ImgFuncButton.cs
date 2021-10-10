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
    public class ImgFuncButton : Control
    {
        public static readonly DependencyProperty DetailUrlProperty = DependencyProperty.Register("DetailUrlProperty", typeof(string), typeof(Button));

        public string DetailUrl
        {
            get => GetValue(DetailUrlProperty).ToString();
            set => SetValue(DetailUrlProperty, value);
        }

        public event ImgFuncButtonHandler PreviewEvent;
        public event ImgFuncButtonHandler DownloadEvent;
        public event ImgFuncButtonHandler SetBackgroundEvent;

        static ImgFuncButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImgFuncButton), new FrameworkPropertyMetadata(typeof(ImgFuncButton)));
        }

        public delegate void ImgFuncButtonHandler(object sender, string url);

       
    }
}
