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

namespace WindowsX.Shell.View.Common.MessageBoxEx
{
    /// <summary>
    /// WaitingDialog.xaml 的交互逻辑
    /// </summary>
    public partial class WaitingDialog : Window
    {
        CancellationTokenSource tokenSource = new CancellationTokenSource();

        public WaitingDialog(Window ownerWindow, int waitMillionSeconds = 20000)
        {
            InitializeComponent();

            StartWait(waitMillionSeconds);

            this.Width = ownerWindow.Width;
            this.Height = ownerWindow.Height;
            this.Left = ownerWindow.Left;
            this.Top = ownerWindow.Top;
        }

        private async void StartWait(int waitMillionSeconds)
        {
            try
            {
                await Task.Delay(waitMillionSeconds, tokenSource.Token);

                this.Close();
            }
            catch
            {
                //cancel
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            tokenSource.Cancel();
        }
    }
}
