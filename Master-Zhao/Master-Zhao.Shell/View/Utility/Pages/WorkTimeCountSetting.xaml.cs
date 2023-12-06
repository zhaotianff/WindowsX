using Master_Zhao.Shell.View.Utility.Windows;
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

namespace Master_Zhao.Shell.View.Utility.Pages
{
    /// <summary>
    /// WorkTimeCountSetting.xaml 的交互逻辑
    /// </summary>
    public partial class WorkTimeCountSetting : Page
    {
        WorkTimeCount workTimeCount;

        public WorkTimeCountSetting()
        {
            InitializeComponent();
        }

        private void cbxWorkTimeCount_Checked(object sender, RoutedEventArgs e)
        {
            workTimeCount = new WorkTimeCount();
            workTimeCount.Show();
        }

        private void cbxWorkTimeCount_Unchecked(object sender, RoutedEventArgs e)
        {
            workTimeCount.Close();
        }
    }
}
