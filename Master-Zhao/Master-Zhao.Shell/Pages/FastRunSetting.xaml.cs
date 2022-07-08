using Master_Zhao.Shell.Windows;
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

namespace Master_Zhao.Shell.Pages
{
    /// <summary>
    /// FastRunSetting.xaml 的交互逻辑
    /// </summary>
    public partial class FastRunSetting : Page
    {
        FastRun fastRun = new FastRun();

        public FastRunSetting()
        {
            InitializeComponent();

            //TODO
            fastRun.Visibility = Visibility.Visible;
            fastRun.Visibility = Visibility.Hidden;
            fastRun.UnregisterHotKey();
        }

        private void cbxFastrun_Checked(object sender, RoutedEventArgs e)
        {
            fastRun.RegisterHotKey();
        }

        private void cbxFastrun_Unchecked(object sender, RoutedEventArgs e)
        {
            fastRun.UnregisterHotKey();
        }
    }
}
