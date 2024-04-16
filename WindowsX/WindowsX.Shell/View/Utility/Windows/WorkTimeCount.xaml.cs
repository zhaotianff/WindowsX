using WindowsX.Shell.Controls;
using WindowsX.Shell.Model.Utility;
using WindowsX.Shell.PInvoke;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WindowsX.Shell.View.Utility.Windows
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
        public bool IsDocking 
        { 
            get => isDocking;
            set
            {
                isDocking = value;

                if (this.WindowState == WindowState.Normal)
                {
                    if(isDocking)
                    {
                        HideWindow();
                    }
                    else
                    {
                        ShowWindow();
                    }
                }
            }
        }

        private bool isAdsorption = false;
        public bool IsAdsorption { get => isAdsorption; set => isAdsorption = value; }

        private bool isAnimation = false;
        private bool isDraged = false;

        private System.Windows.Media.Animation.Storyboard hiddenAnimation;
        private System.Windows.Media.Animation.Storyboard showAnimation;

        public WorkTimeCount(ObservableCollection<WorkTimeItem> workTimeItems)
        {
            InitializeComponent();
            InitializeWorkItems(workTimeItems);
            InitializeAnimation();
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
            //this.list_WorkItems.ItemsSource = this.workTimeItems;
            for(int i = 0;i<workTimeItems.Count;i++)
            {
                AddWorkTimeCountItem(workTimeItems[i]);
            }
        }

        public void AddWorkTimeCountItem(WorkTimeItem workTimeItem)
        {
            //TODO use binding
            WorkTimeItemControl workTimeItemControl = new WorkTimeItemControl();
            workTimeItemControl.Data = workTimeItem;
            workTimeItemControl.OnStart += WorkTimeItemControl_OnStart;
            workTimeItemControl.Height = 30;
            this.list_WorkItems.Items.Add(workTimeItemControl);
        }

        public void UpdateBackgroundImage(string filePath,int index)
        {
            var workTimeItemControl =  this.list_WorkItems.Items[index] as WorkTimeItemControl;
            ImageBrush ib = new ImageBrush();
            ib.Stretch = Stretch.UniformToFill;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(filePath, UriKind.Absolute);
            bi.EndInit();
            ib.ImageSource = bi;
            workTimeItemControl.Background = ib;
        }

        private void BlurWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    this.DragMove();
                    isDraged = true;
                }
                catch
                {

                }
            }

            if (e.LeftButton == MouseButtonState.Released && isDocking == true && isDraged == true)
            {
                POINT point = new POINT();
                if (User32.GetCursorPos(ref point) == 1)
                {
                    var pos = e.GetPosition(this);

                    if (pos.X < 0 && pos.Y < 0)
                        HideWindow();
                }

                isDraged = false;
            }

            //TODO isAdsorption
        }

        private void HideWindow(double left = -1)
        {
            if (left == -1)
            {
                PInvoke.RECT rect = new RECT();
                User32.GetWindowRect(new WindowInteropHelper(this).Handle, ref rect);
                left = rect.left;
            }

            if (SystemParameters.PrimaryScreenWidth - left - this.Width > 15)
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

        private void main_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isDocking && isDraged == false)
            {   
                 HideWindow();            
            }
        }

        private void main_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this);
            if (pos.X >= 0 && pos.Y >= 0)
                isDraged = true;
        }

        private void WorkTimeItemControl_OnStart(object sender, RoutedEventArgs e)
        {
            var index = this.list_WorkItems.Items.IndexOf(sender);

            for(int i =0;i<this.list_WorkItems.Items.Count;i++)
            {
                if(i != index)
                {
                    (this.list_WorkItems.Items[i] as WorkTimeItemControl).IsRunning = false;
                }
            }
        }

        public void StopAllCount()
        {
            for (int i = 0; i < this.list_WorkItems.Items.Count; i++)
            {
                (this.list_WorkItems.Items[i] as WorkTimeItemControl).IsRunning = false;
            }
        }

        public void RemoveWorkTimeCountItem(int index)
        {
            //TODO use binding
            if (index < this.list_WorkItems.Items.Count)
            {
                (this.list_WorkItems.Items[index] as WorkTimeItemControl).IsRunning = false;
                this.list_WorkItems.Items.RemoveAt(index);
            }
        }
    }
}
