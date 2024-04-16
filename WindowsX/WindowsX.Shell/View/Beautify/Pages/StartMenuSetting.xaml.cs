using WindowsX.Shell.StartMenu;
using WindowsX.Shell.StartMenu.Win98;
using WindowsX.Shell.Util;
using WindowsX.Shell.View.UserControls;
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

namespace WindowsX.Shell.View.Pages
{
    /// <summary>
    /// StartMenuSetting.xaml 的交互逻辑
    /// </summary>
    public partial class StartMenuSetting : Page
    {
        public StartMenuSetting()
        {
            InitializeComponent();
        }

        private void OnSetStartMenu(object sender, string menuName)
        {
            LaunchStartMenu(menuName);
        }

        private void LaunchStartMenu(string menuName)
        {
            StartMenuManager.LaunchStartMenu(menuName);
        }

        private void OnSelectStartMenu(object sender, EventArgs e)
        {
            foreach (var item in wrap.Children)
            {
                var startMenuControl = item as StartMenuControl;
                startMenuControl?.ResetBorderBrush();
            }
        }

        private void btn_RestartExplorer_Click(object sender, RoutedEventArgs e)
        {
            StartMenuManager.UnHookStart();
            PInvoke.DesktopTool.RestartExplorer();
        }
    }
}
