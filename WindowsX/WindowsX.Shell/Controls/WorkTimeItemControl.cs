using WindowsX.Shell.Infrastructure.Media;
using WindowsX.Shell.Model.Utility;
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

namespace WindowsX.Shell.Controls
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WindowsX.Shell.Controls"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WindowsX.Shell.Controls;assembly=WindowsX.Shell.Controls"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误:
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:WorkTimeItemControl/>
    ///
    /// </summary>
    public class WorkTimeItemControl : Control
    {
        private SolidColorBrush[] BackgroundBrushes = ColorManager.WorkTimeCountColors;
        private Image imgStart;
        private Image imgPause;
        private Stopwatch stopwatch = new Stopwatch();
        private bool isStart = false;

        public static readonly DependencyProperty IsRunningProperty = DependencyProperty.Register("IsRunning", typeof(bool), typeof(WorkTimeItemControl), new PropertyMetadata(OnIsRunningChanged));

        public bool IsRunning
        {
            get => (bool)GetValue(IsRunningProperty);
            set => SetValue(IsRunningProperty, value);
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(WorkTimeItem), typeof(WorkTimeItemControl), new PropertyMetadata(OnDataChanged));

        public WorkTimeItem Data
        {
            get => GetValue(DataProperty) as WorkTimeItem;
            set => SetValue(DataProperty, value);
        }

        public static readonly RoutedEvent OnStartEvent = EventManager.RegisterRoutedEvent("OnStart", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(WorkTimeItemControl));

        public event RoutedEventHandler OnStart
        {
            add
            {
                AddHandler(OnStartEvent, value);
            }
            remove
            {
                RemoveHandler(OnStartEvent, value);
            }
        }

        public static readonly RoutedEvent OnPauseEvent = EventManager.RegisterRoutedEvent("OnPause", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(WorkTimeItemControl));

        public event RoutedEventHandler OnPause
        {
            add
            {
                AddHandler(OnPauseEvent, value);
            }
            remove
            {
                RemoveHandler(OnPauseEvent, value);
            }
        }

        static WorkTimeItemControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WorkTimeItemControl), new FrameworkPropertyMetadata(typeof(WorkTimeItemControl)));
        }

        public WorkTimeItemControl()
        {
            OnStart += WorkTimeItemControl_OnStart;
            OnPause += WorkTimeItemControl_OnPause;
        }

        private void WorkTimeItemControl_OnPause(object sender, RoutedEventArgs e)
        {
            IsRunning = false;
        }

        private void WorkTimeItemControl_OnStart(object sender, RoutedEventArgs e)
        {
            IsRunning = true;
        }

        private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var workTimeItemControl = d as WorkTimeItemControl;
            workTimeItemControl.DataContext = e.NewValue as WorkTimeItem;
        }

        private static void OnIsRunningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var workTimeItemControl = d as WorkTimeItemControl;
            
            if((bool)e.NewValue == true)
            {
                workTimeItemControl.StartWatcher();
            }
            else
            {
                workTimeItemControl.StopWatcher();
            }
        }

        private void StartWatcher()
        {
            ShowStartButton(false);

            stopwatch.Start();
            isStart = true;

            Task.Run(async () => {
                while (isStart)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        Data.EllapsedTime = stopwatch.ElapsedMilliseconds;
                    });
                    await Task.Delay(100);
                }
            });
        }

        private void StopWatcher()
        {
            ShowStartButton(true);

            isStart = false;
            stopwatch.Stop();
        }

        private void ShowStartButton(bool isShow)
        {
            imgStart.Visibility = isShow == true ? Visibility.Visible : Visibility.Collapsed;
            imgPause.Visibility = isShow == false ? Visibility.Visible : Visibility.Collapsed;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Random r = new Random();
            this.Background = BackgroundBrushes[r.Next(0, BackgroundBrushes.Length)];

            imgStart = this.GetTemplateChild("img_start") as Image;
            imgPause = this.GetTemplateChild("img_pause") as Image;
            imgStart.MouseDown += (a, b) => { this.RaiseEvent(new RoutedEventArgs(OnStartEvent)); };
            imgPause.MouseDown += (a, b) => { this.RaiseEvent(new RoutedEventArgs(OnPauseEvent)); };
        }
    }
}
