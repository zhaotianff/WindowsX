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

using Silver_Arowana.Shell.PInvoke;

namespace Silver_Arowana.Shell.Pages
{
    /// <summary>
    /// DesktopSetting.xaml 的交互逻辑
    /// </summary>
    public partial class DesktopSetting : Page
    {
        public DesktopSetting()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder(DesktopTool.MAX_PATH);
            if (DesktopTool.GetBackground(sb))
            {
                this.img_background.Source = new BitmapImage(new Uri(sb.ToString(), UriKind.Absolute));
            }
        }
    }

}

