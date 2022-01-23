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
            var isWindows10 = SystemTool.IsWindows10();
            var isWindows10Orhigher = SystemTool.IsWindows10OrHigher();

            if(isWindows10)
            {
                cbx_Windows11Taskbar.IsEnabled = true;
            }

            if(isWindows10Orhigher)
            {
                group_TaskbarBlur.IsEnabled = true;
            }
        }

        private void btn_EnableBlurTaskbar_Click(object sender, RoutedEventArgs e)
        {
            BlurTaskbar(true);   
        }

        private void btn_DisableBlurTaskbar_Click(object sender, RoutedEventArgs e)
        {
            BlurTaskbar(false);
        }

        private void BlurTaskbar(bool isEnable)
        {
            DesktopTool.ACCENT_POLICY policy = new DesktopTool.ACCENT_POLICY();
            if (isEnable)
            {
                policy.AcentState = DesktopTool.ACCENT_ENABLE_BLURBEHIND;
            }
            else
            {
                policy.AcentState = DesktopTool.ACCENT_DISABLED;
            }
            policy.AccentFlags = 0;
            policy.AnimationId = 0;
            policy.GradientColor = 0;
            DesktopTool.BlurTaskbar(policy);
        }

        private void cbx_Windows11Taskbar_Checked(object sender, RoutedEventArgs e)
        {
            CenterTaskbar(true);
        }

        private void cbx_Windows11Taskbar_Unchecked(object sender, RoutedEventArgs e)
        {
            CenterTaskbar(false);
        }

        private void CenterTaskbar(bool isEnable)
        {
            DesktopTool.CenterStartMenu(isEnable);
            DesktopTool.CenterTaskListIcon(isEnable);
        }
    }
}
