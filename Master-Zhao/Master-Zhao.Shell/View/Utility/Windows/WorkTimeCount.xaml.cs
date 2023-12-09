using Master_Zhao.Shell.Model.Utility;
using Master_Zhao.Shell.View.Utility.UserControls;
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
using System.Windows.Shapes;

namespace Master_Zhao.Shell.View.Utility.Windows
{
    /// <summary>
    /// WorkTimeCount.xaml 的交互逻辑
    /// </summary>
    public partial class WorkTimeCount : System.Windows.Window
    {
        private List<WorkTimeItem> workTimeItems;
        private SolidColorBrush[] BackgroundBrushes = { Brushes.Pink, Brushes.LightBlue, Brushes.LightGreen };

        public WorkTimeCount(List<WorkTimeItem> workTimeItems)
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
            InitializeWorkItems(workTimeItems);
        }

        private void InitializeWorkItems(List<WorkTimeItem> workTimeItems)
        {
            this.stack_WorkItems.Children.Clear();
            this.workTimeItems = workTimeItems;

            for (int i = 0; i < workTimeItems.Count; i++)
            {
                WorkTimeItemControl workTimeItemControl = new WorkTimeItemControl(workTimeItems[i]);
                if (i == 0)
                    workTimeItemControl.Margin = new Thickness(5);
                else
                    workTimeItemControl.Margin = new Thickness(5, 0, 5, 5);

                workTimeItemControl.Background = BackgroundBrushes[i % 3];

                this.stack_WorkItems.Children.Add(workTimeItemControl);
            }
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
