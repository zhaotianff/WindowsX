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
using Master_Zhao.Shell.Util;

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
            CloseWorkTimeCount();
        }

        private List<WorkTimeItem> LoadWorkTimeItem()
        {
            return new List<WorkTimeItem>() 
            {
                new WorkTimeItem() { EllapsedTimeString = "", Title = "工作" } ,
                new WorkTimeItem() { EllapsedTimeString = "", Title = "摸鱼" }
            };
        }

        private void btn_AddWorkTimeItem_Click(object sender, RoutedEventArgs e)
        {
            var workItemCount = new WorkTimeItem() { EllapsedTimeString = "", Title = this.tbox_NewWorkItem.Text };
            this.workTimeItems.Add(workItemCount);
            this.workTimeCount.AddWorkTimeCountItem(workItemCount);
        }

        private void btn_RemoveWorkTimeItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.list_WorkItems.SelectedIndex != -1)
            {
                this.workTimeCount.RemoveWorkTimeCountItem(this.list_WorkItems.SelectedIndex);
                this.workTimeItems.RemoveAt(this.list_WorkItems.SelectedIndex);
            }
        }

        private void cbx_IsDocking_Checked(object sender, RoutedEventArgs e)
        {
            if(workTimeCount != null)
                workTimeCount.IsDocking = true;
        }

        private void cbx_IsDocking_Unchecked(object sender, RoutedEventArgs e)
        {
            if (workTimeCount != null)
                workTimeCount.IsDocking = false;
        }

        private void cbx_IsAdsorption_Checked(object sender, RoutedEventArgs e)
        {
            if (workTimeCount != null)
                workTimeCount.IsAdsorption = true;
        }

        private void cbx_IsAdsorption_Unchecked(object sender, RoutedEventArgs e)
        {
            if (workTimeCount != null)
                workTimeCount.IsAdsorption = false;
        }

        public void CloseWorkTimeCount()
        {
            if (workTimeCount != null)
            {
                workTimeCount.StopAllCount();
                workTimeCount.Close();
                workTimeCount = null;
            }
        }

        private void btn_BrowseBackgroundImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "图片文件|*.jpg;*.png;*.bmp;*.jpeg;*.jiff;*.webp";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if(openFileDialog.ShowDialog() == true)
            {
                var btnBrowseBackgroundImage = sender as DependencyObject;
                var workTimeItem = btnBrowseBackgroundImage.FindParent<ListBoxItem>().Content as WorkTimeItem;
                var index = this.workTimeItems.IndexOf(workTimeItem);
                var workCountItem = this.workTimeItems[index];
                workCountItem.BackgroundImage = openFileDialog.FileName;
            }
            
        }
    }
}
