using Master_Zhao.Shell.Infrastructure.Navigation;
using Master_Zhao.Shell.View.Setting.Pages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Master_Zhao.Shell.View.Setting
{
    /// <summary>
    /// SettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPage : Page, IPageAction
    {
        private ToggleButton toggleButton = null;
        private BackgroundSetting backgroundSetting = new BackgroundSetting();

        public SettingPage()
        {
            InitializeComponent();
        }

        public void ShowDefaultPage()
        {
            btn_BackgroundSettingClick(null, null);
        }

        public void Terminate()
        {
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            (System.Windows.Application.Current.MainWindow as MainWindow).EndShowMenuAnimation();
        }

        private void btn_BackgroundSettingClick(object sender, RoutedEventArgs e)
        {
            this.frame.Content = backgroundSetting;
            backgroundSetting.Load();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (toggleButton != null)
                toggleButton.IsChecked = false;

            toggleButton = sender as ToggleButton;
        }
    }
}
