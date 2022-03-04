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
    /// MouseEffect.xaml 的交互逻辑
    /// </summary>
    public partial class MouseEffect : Page
    {
        MouseEffectWindow mouseEffectWindow;

        public MouseEffect()
        {
            InitializeComponent();
        }

        private void mouseEffect_Checked(object sender, RoutedEventArgs e)
        {
            mouseEffectWindow = new MouseEffectWindow();
            mouseEffectWindow.Show();
        }

        private void mouseEffect_Unchecked(object sender, RoutedEventArgs e)
        {
            mouseEffectWindow?.Close();
            mouseEffectWindow = null;
        }
    }
}
