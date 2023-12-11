using Master_Zhao.Shell.Model.Utility;
using Master_Zhao.Shell.View.Utility.UserControls;
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
using System.Windows.Shapes;

namespace Master_Zhao.Shell.View.Utility.Windows
{
    /// <summary>
    /// WorkTimeCount.xaml 的交互逻辑
    /// </summary>
    public partial class WorkTimeCount : System.Windows.Window
    {
        private ObservableCollection<WorkTimeItem> workTimeItems;
        private SolidColorBrush[] BackgroundBrushes = { Brushes.Pink, Brushes.LightBlue, Brushes.LightGreen };

        public WorkTimeCount(ObservableCollection<WorkTimeItem> workTimeItems)
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
            InitializeWorkItems(workTimeItems);
        }

        private void InitializeWorkItems(ObservableCollection<WorkTimeItem> workTimeItems)
        {
            this.workTimeItems = workTimeItems;
            this.list_WorkItems.ItemsSource = this.workTimeItems;
        }

        private void BlurWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    this.DragMove();
                }
                catch
                {

                }
            }
        }
    }
}
