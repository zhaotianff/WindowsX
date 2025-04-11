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

namespace WindowsX.Shell.View.SystemMgmt.UserControls
{
    /// <summary>
    /// SystemInfoItemControl.xaml 的交互逻辑
    /// </summary>
    public partial class SystemInfoItemControl : UserControl
    {
        public SystemInfoItemControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(SystemInfoItemControl),new PropertyMetadata());
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(SystemInfoItemControl), new PropertyMetadata());

        public string Title
        {
            get => GetValue(TitleProperty).ToString();
            set => SetValue(TitleProperty, value);
        }

        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }
    }
}
