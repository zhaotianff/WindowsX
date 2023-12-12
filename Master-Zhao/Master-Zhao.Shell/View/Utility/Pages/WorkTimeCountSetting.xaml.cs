using Master_Zhao.Shell.Model.Utility;
using Master_Zhao.Shell.View.Utility.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        ObservableCollection<WorkTimeItem> workTimeItems;

        public WorkTimeCountSetting()
        {
            InitializeComponent();       
        }

        private void cbxWorkTimeCount_Checked(object sender, RoutedEventArgs e)
        {
            workTimeItems = new ObservableCollection<WorkTimeItem>(LoadWorkTimeItem());
            this.list_WorkItems.ItemsSource = workTimeItems;

            workTimeCount = new WorkTimeCount(workTimeItems);
            workTimeCount.Top = 0;
            workTimeCount.Left = SystemParameters.PrimaryScreenWidth - workTimeCount.Width;
            workTimeCount.Show();
        }

        private void cbxWorkTimeCount_Unchecked(object sender, RoutedEventArgs e)
        {
            workTimeCount.Close();
        }

        private List<WorkTimeItem> LoadWorkTimeItem()
        {
            return new List<WorkTimeItem>() 
            {
                new WorkTimeItem() { EllapsedTimeString = "", Title = "测试工作项" } ,
                new WorkTimeItem() { EllapsedTimeString = "", Title = "摸鱼" },
                new WorkTimeItem() { EllapsedTimeString = "", Title = "划水" }
            };
        }

        private void btn_AddWorkTimeItem_Click(object sender, RoutedEventArgs e)
        {
            workTimeItems.Add(new WorkTimeItem() { EllapsedTimeString = "0时0分", Title = this.tbox_NewWorkItem.Text });
        }

        private void btn_RemoveWorkTimeItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.list_WorkItems.SelectedIndex != -1)
                this.workTimeItems.RemoveAt(this.list_WorkItems.SelectedIndex);
        }

        private void cbx_IsDocking_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void cbx_IsDocking_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void cbx_IsAdsorption_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void cbx_IsAdsorption_Unchecked(object sender, RoutedEventArgs e)
        {

        }
    }
}
