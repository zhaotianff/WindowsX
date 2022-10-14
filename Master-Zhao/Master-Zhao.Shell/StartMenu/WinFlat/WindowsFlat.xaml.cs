using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Master_Zhao.Shell.StartMenu.WinFlat
{
    /// <summary>
    /// WindowsFlat.xaml 的交互逻辑
    /// </summary>
    public partial class WindowsFlat : TianXiaTech.BlurWindow
    {
        public WindowsFlat()
        {
            InitializeComponent();
            this.Left = 10;
            this.Top = SystemParameters.WorkArea.Height - this.Height - 10;
        }


        private void ShowMenu()
        {
            this.Visibility = Visibility.Visible;
            DoubleAnimation showAnimation = new DoubleAnimation();
            showAnimation.From = 0;
            showAnimation.To = 1;
            showAnimation.Duration = TimeSpan.FromMilliseconds(300);
            this.BeginAnimation(OpacityProperty, showAnimation);
        }

        private void HideMenu()
        {
            
        }

        private void img_poweroff_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.popup_poweroff.IsOpen = !this.popup_poweroff.IsOpen;
            popup_poweroff.Focus();
        }

        private void img_setting_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void popup_poweroff_LostFocus(object sender, RoutedEventArgs e)
        {
            popup_poweroff.IsOpen = false;
        }
    }
}
