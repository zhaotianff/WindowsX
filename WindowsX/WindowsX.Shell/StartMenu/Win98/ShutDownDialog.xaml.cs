using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WindowsX.Shell.StartMenu.Data;

namespace WindowsX.Shell.StartMenu.Win98
{
    /// <summary>
    /// ShutdownDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ShutdownDialog : Window
    {
        private Windows98ShutdownType currentShutdownType;

        public Windows98ShutdownType ShutdownType { get; private set; }


        public ShutdownDialog()
        {
            InitializeComponent();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            ShutdownType = currentShutdownType;
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            ShutdownType = Windows98ShutdownType.Help;
            this.DialogResult = true;
        }

        private void shutdownType_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            currentShutdownType = (Windows98ShutdownType)stack_Shutdown.Children.IndexOf(radioButton);
        }
    }
}
