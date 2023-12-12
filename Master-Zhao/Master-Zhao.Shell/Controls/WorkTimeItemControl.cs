using Master_Zhao.Shell.Infrastructure.Media;
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

namespace Master_Zhao.Shell.Controls
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Master_Zhao.Shell.Controls"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Master_Zhao.Shell.Controls;assembly=Master_Zhao.Shell.Controls"
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
            StopWatcher();
            ShowStartButton(true);
        }

        private void WorkTimeItemControl_OnStart(object sender, RoutedEventArgs e)
        {
            StartWatcher();
            ShowStartButton(false);
        }

        private Stopwatch stopwatch = new Stopwatch();
        private bool isStart = false;

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(WorkTimeItem), typeof(WorkTimeItemControl), new PropertyMetadata(OnDataChanged));

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

        public WorkTimeItem Data
        {
            get => GetValue(DataProperty) as WorkTimeItem;
            set => SetValue(DataProperty, value);
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
            var imgStart = this.GetTemplateChild("img_start") as Image;
            var imgPause = this.GetTemplateChild("img_pause") as Image;
            imgStart.Visibility = isShow == true ? Visibility.Visible : Visibility.Collapsed;
            imgPause.Visibility = isShow == false ? Visibility.Visible : Visibility.Collapsed;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Random r = new Random();
            this.Background = BackgroundBrushes[r.Next(0, BackgroundBrushes.Length)];

            var imgStart = this.GetTemplateChild("img_start") as Image;
            var imgPause = this.GetTemplateChild("img_pause") as Image;
            imgStart.MouseDown += (a, b) => { this.RaiseEvent(new RoutedEventArgs(OnStartEvent)); };
            imgPause.MouseDown += (a, b) => { this.RaiseEvent(new RoutedEventArgs(OnPauseEvent)); };
        }
    }
}
