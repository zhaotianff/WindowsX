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

namespace Master_Zhao.Shell.View.Utility.UserControls
{
    /// <summary>
    /// WorkTimeItem.xaml 的交互逻辑
    /// </summary>
    public partial class WorkTimeItem : UserControl
    {
        public WorkTimeItem()
        {
            InitializeComponent();
        }

        private void start_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowStartButton(false);
        }

        private void pause_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowStartButton(true);
        }

        private void ShowStartButton(bool isShow)
        {
            this.img_start.Visibility = isShow == true ? Visibility.Visible : Visibility.Collapsed;
            this.img_pause.Visibility = isShow == false ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
