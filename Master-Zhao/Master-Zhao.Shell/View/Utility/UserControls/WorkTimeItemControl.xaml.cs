using Master_Zhao.Shell.Model.Utility;
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
    /// WorkTimeItemControl.xaml 的交互逻辑
    /// </summary>
    public partial class WorkTimeItemControl : UserControl
    {
        private WorkTimeItem workTimeItem;

        public WorkTimeItemControl(WorkTimeItem workTimeItem)
        {
            InitializeComponent();
            this.workTimeItem = workTimeItem;
            this.lbl_Title.Content = workTimeItem.Title;
            this.lbl_EllapsedTimeString.Content = workTimeItem.EllapsedTimeString;
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
