using Master_Zhao.Shell.Model.Utility;
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
        private const double DockAreaWidth = 5;
        private const double ContentAreaWidth = 300;

        private ObservableCollection<WorkTimeItem> workTimeItems;
        private bool isDocking = false;
        private bool isAdsorption = false;
        private bool isAnimation = false;
        private System.Windows.Media.Animation.Storyboard hiddenAnimation;
        private System.Windows.Media.Animation.Storyboard showAnimation;
        private bool isPressed = false;

        public WorkTimeCount(ObservableCollection<WorkTimeItem> workTimeItems,bool isDocking, bool isAdsorption)
        {
            InitializeComponent();
            InitializeWorkItems(workTimeItems);
            InitializeAnimation();

            this.isDocking = isDocking;
            this.isAdsorption = isAdsorption;
        }

        private void InitializeAnimation()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            hiddenAnimation = this.FindResource("hiddenAnimation") as System.Windows.Media.Animation.Storyboard;
            var hiddenDoubleAnimation = hiddenAnimation.Children[0] as System.Windows.Media.Animation.DoubleAnimation;            
            hiddenDoubleAnimation.From = screenWidth - DockAreaWidth - ContentAreaWidth;
            hiddenDoubleAnimation.To = screenWidth - DockAreaWidth;
            hiddenAnimation.Completed += HiddenAnimation_Completed;

            showAnimation = this.FindResource("showAnimation") as System.Windows.Media.Animation.Storyboard;
            var showDoubleAnimation = showAnimation.Children[0] as System.Windows.Media.Animation.DoubleAnimation;
            showDoubleAnimation.From = screenWidth - DockAreaWidth;
            showDoubleAnimation.To = screenWidth - DockAreaWidth - ContentAreaWidth;
            showAnimation.Completed += ShowAnimation_Completed;
        }

        private void ShowAnimation_Completed(object sender, EventArgs e)
        {
            isAnimation = false;
        }

        private void HiddenAnimation_Completed(object sender, EventArgs e)
        {
            isAnimation = false;
            grid_DockArea.Visibility = Visibility.Visible;
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
                    isPressed = true;
                }
                catch
                {

                }
            }
            else if(e.LeftButton == MouseButtonState.Released)
            {
                if (isPressed)
                {
                    HideWindow();
                    isPressed = false;
                }
                
            }

            //TODO isAdsorption

            var pos = e.GetPosition(this);
        }

        private async void main_Loaded(object sender, RoutedEventArgs e)
        {
            await System.Threading.Tasks.Task.Delay(3000);
            HideWindow();
        }

        private void HideWindow()
        {
            if (SystemParameters.PrimaryScreenWidth - this.Left - this.Width > 15)
                return;

            if (isAnimation)
                return;

            isAnimation = true;
            hiddenAnimation.Begin();
        }

        private void grid_DockArea_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowWindow();
        }

        private void ShowWindow()
        {
            if (isAnimation)
                return;

            grid_DockArea.Visibility = Visibility.Collapsed;
            isAnimation = true;
            showAnimation.Begin();  
        }
    }
}
