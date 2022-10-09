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

namespace Master_Zhao.Shell.StartMenu.Flat
{
    /// <summary>
    /// FlatStartMenu.xaml 的交互逻辑
    /// </summary>
    public partial class FlatStartMenu : TianXiaTech.BlurWindow
    {
        public FlatStartMenu()
        {
            InitializeComponent();
            this.Visibility = Visibility.Hidden;
            Test();
        }

        private async void Test()
        {
            await Task.Delay(1000);

            this.Left = 0;
            this.Top = SystemParameters.WorkArea.Height - this.Height;


            await Task.Delay(2000);
            ShowMenu();
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
    }
}
