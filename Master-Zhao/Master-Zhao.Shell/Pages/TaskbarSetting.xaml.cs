using Master_Zhao.Shell.PInvoke;
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
    /// Interaction logic for TaskbarSetting.xaml
    /// </summary>
    public partial class TaskbarSetting : Page
    {
        public TaskbarSetting()
        {
            InitializeComponent();
        }

        public void CheckCurrentSystem()
        {

        }

        private void btn_EnableBlurTaskbar_Click(object sender, RoutedEventArgs e)
        {
            DesktopTool.ACCENT_POLICY policy = new DesktopTool.ACCENT_POLICY();
            policy.AcentState = DesktopTool.ACCENT_ENABLE_BLURBEHIND;
            policy.AccentFlags = 0;
            policy.AnimationId = 0;
            policy.GradientColor = 0;
            DesktopTool.BlurTaskbar(policy);
        }
    }
}
