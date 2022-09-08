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
        private System.Windows.Threading.DispatcherTimer blurTaskbarTimer = new System.Windows.Threading.DispatcherTimer();
        DesktopTool.ACCENT_POLICY policy = new DesktopTool.ACCENT_POLICY();

        private const int DEFAULT_BLUR_INTERVAL = 5;

        public TaskbarSetting()
        {
            InitializeComponent();
            InitBlurTaskbarTimer();
            InitDefaultPolicy();
        }

        private void InitBlurTaskbarTimer()
        {
            blurTaskbarTimer.Interval = TimeSpan.FromMilliseconds(DEFAULT_BLUR_INTERVAL);
            blurTaskbarTimer.Tick += BlurTaskbarTimer_Tick;
        }

        private void InitDefaultPolicy()
        {
            policy.AcentState = DesktopTool.ACCENT_ENABLE_BLURBEHIND;
            policy.AccentFlags = 0;
            policy.AnimationId = 0;
            policy.GradientColor = 0;
        }

        private void BlurTaskbarTimer_Tick(object sender, EventArgs e)
        {
            DesktopTool.BlurTaskbar(policy);
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
            blurTaskbarTimer.IsEnabled = isEnable;
            RefreshTaskbar();
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

        private void blur_Checked(object sender, RoutedEventArgs e)
        {
            policy.AcentState = DesktopTool.ACCENT_ENABLE_BLURBEHIND;
        }

        private void transparent_Checked(object sender, RoutedEventArgs e)
        {
            policy.AcentState = DesktopTool.ACCENT_ENABLE_ACRYLICBLURBEHIND;
        }

        private async void RefreshTaskbar()
        {
            await Task.Delay(DEFAULT_BLUR_INTERVAL);
            PInvoke.DesktopTool.ActivateTaskBar();
        }
    }
}
