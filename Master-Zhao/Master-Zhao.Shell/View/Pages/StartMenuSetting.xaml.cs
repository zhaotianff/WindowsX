using Master_Zhao.Shell.StartMenu.Win98;
using Master_Zhao.Shell.Util;
using Master_Zhao.Shell.View.UserControls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Master_Zhao.Shell.View.Pages
{
    /// <summary>
    /// StartMenuSetting.xaml 的交互逻辑
    /// </summary>
    public partial class StartMenuSetting : Page
    {
        private static readonly string SYS_MENU_NAME = "系统默认";

        public StartMenuSetting()
        {
            InitializeComponent();
        }

        private void OnSetStartMenu(object sender, string menuName)
        {
            if (menuName == SYS_MENU_NAME)
            {
                PInvoke.SystemTool.UnHookStart();
                ProcessHelper.KillProcess(menuName);
                return;
            }

            LaunchStartMenu(menuName);
        }

        private void LaunchStartMenu(string menuName)
        {
            var startMenuHandle = ProcessHelper.FindProcessWindow(menuName);
            if (startMenuHandle == IntPtr.Zero)
            {
                Windows98 windows98 = new Windows98();
            }
        }

        private void OnSelectStartMenu(object sender, EventArgs e)
        {
            foreach (var item in wrap.Children)
            {
                var startMenuControl = item as StartMenuControl;
                startMenuControl?.ResetBorderBrush();
            }
        }
    }
}
