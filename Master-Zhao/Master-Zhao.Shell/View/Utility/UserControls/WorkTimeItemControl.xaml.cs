using Master_Zhao.Shell.Controls;
using Master_Zhao.Shell.Model.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
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
        private Stopwatch stopwatch = new Stopwatch();
        private bool isStart = false;

        public static readonly DependencyProperty EllapsedTimeStringProperty = DependencyProperty.Register("EllapsedTimeString", typeof(string),typeof(WorkTimeItemControl));
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(WorkTimeItemControl));
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(WorkTimeItem), typeof(WorkTimeItemControl),new PropertyMetadata(OnDataChanged));

        static WorkTimeItemControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WorkTimeItemControl),
                new FrameworkPropertyMetadata(typeof(WorkTimeItemControl)));
        }

        public string EllapsedTimeString
        {
            get => GetValue(EllapsedTimeStringProperty).ToString();
            set => SetValue(EllapsedTimeStringProperty, value);
        }

        public string Title
        {
            get => GetValue(TitleProperty).ToString();
            set => SetValue(TitleProperty, value);
        }

        public WorkTimeItem Data
        {
            get => GetValue(DataProperty) as WorkTimeItem;
            set => SetValue(DataProperty, value);
        }

        public WorkTimeItemControl()
        {
            InitializeComponent();
        }

        private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var workTimeItemControl = d as WorkTimeItemControl;
            workTimeItemControl.DataContext = e.NewValue as WorkTimeItem;
        }

        private void start_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowStartButton(false);
            StartWatcher();
        }

        private void StartWatcher()
        {
            stopwatch.Start();
            isStart = true;

            Task.Run(async () => { 
            while(isStart)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        Data.EllapsedTime = stopwatch.ElapsedMilliseconds;
                        EllapsedTimeString = Data.EllapsedTimeString;
                    });
                    await Task.Delay(1000);
                }
            });
        }

        private void StopWatcher()
        {
            isStart = false;
            stopwatch.Stop();
        }

        private void pause_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowStartButton(true);
            StopWatcher();
        }

        private void ShowStartButton(bool isShow)
        {
            this.img_start.Visibility = isShow == true ? Visibility.Visible : Visibility.Collapsed;
            this.img_pause.Visibility = isShow == false ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
