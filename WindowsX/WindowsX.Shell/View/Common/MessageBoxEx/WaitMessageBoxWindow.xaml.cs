using WindowsX.Shell.PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WindowsX.Shell.MessageBoxEx
{
    /// <summary>
    /// Interaction logic for WaitMessageBoxWindow.xaml
    /// </summary>
    public partial class WaitMessageBoxWindow : TianXiaTech.BlurWindow
    {
        #region static field
        private static AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        private static bool dialogResult = false;
        private static WaitMessageBoxWindow waitMessageBoxWindow;
        private static bool isActiveExit = false;
        #endregion

        #region instance field
        private DispatcherTimer timer = new DispatcherTimer();
        private int leftSecond = 0;
        #endregion

        #region instance property
        public string ConfirmText { get; set; }
      
        public int LeftSecond
        {
            get => leftSecond;
            set
            {
                leftSecond = value;
                btn_Ok.Content = ConfirmText + $"({leftSecond}秒)";
            }
        }

        public string ConfirmTimeText
        {
            get => btn_Ok.Content.ToString();
            set
            {
                Dispatcher.Invoke(() =>
                {
                    btn_Ok.Content = ConfirmText + value;
                });
            }
        }
        #endregion

        #region instance func

        public WaitMessageBoxWindow()
        {
            InitializeComponent();
        }

        public void StartTimer(int second)
        {
            LeftSecond = second;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        public void StopTimer()
        {
            timer.Stop();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            LeftSecond--;
            if (LeftSecond == 0)
            {
                isActiveExit = true;
                dialogResult = true;
                autoResetEvent.Set();
            }
        }

        private void BlurWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isActiveExit == false)
            {
                dialogResult = false;
                autoResetEvent.Set();
            }
        }

        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            isActiveExit = true;
            dialogResult = true;
            autoResetEvent.Set();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            isActiveExit = true;
            dialogResult = false;
            autoResetEvent.Set();
        }

        public void SetMessageBox(string title,string content)
        {
            this.Title = title;
            tbk_Content.Text = content;
        }

        #endregion

        #region static func

        public static async Task<bool> Show(string title, string content, string confirmText,int second)
        {
            waitMessageBoxWindow = new WaitMessageBoxWindow();
            waitMessageBoxWindow.SetMessageBox(title, content);
            waitMessageBoxWindow.ConfirmText = confirmText;
            waitMessageBoxWindow.ConfirmTimeText = $"({second}秒)";
            waitMessageBoxWindow.Owner = Application.Current.MainWindow;
            waitMessageBoxWindow.StartTimer(second);
            
            Task<bool> task = Task.Run(() => 
            {
                autoResetEvent.WaitOne(second * 10000);             
                waitMessageBoxWindow.Dispatcher.Invoke(() => {
                    waitMessageBoxWindow.StopTimer();
                    waitMessageBoxWindow.Close();
                });
                return dialogResult;
            });
            waitMessageBoxWindow.ShowDialog();
            return await task;
        }
        #endregion
    }
}
