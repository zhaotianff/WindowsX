using Master_Zhao.Shell.Infrastructure.Navigation;
using Master_Zhao.Shell.View.SystemMgmt.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Master_Zhao.Shell.Pages
{
    /// <summary>
    /// Interaction logic for SystemManagement.xaml
    /// </summary>
    public partial class SystemManagement : Page,IPageAction
    {
        private ToggleButton toggleButton = null;
        private StartupManagement startupManagement = new StartupManagement();
        private DiskFileManagement diskFileManagement = new DiskFileManagement();
        private Page defaultPage = null;

        public SystemManagement()
        {
            InitializeComponent();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (toggleButton != null)
                toggleButton.IsChecked = false;

            toggleButton = sender as ToggleButton;
        }

        private void btn_StartupMgmtClick(object sender, RoutedEventArgs e)
        {
            this.frame.Content = startupManagement;
            defaultPage = startupManagement;
        }

        private void btn_DiskFileMgmtClick(object sender, RoutedEventArgs e)
        {
            this.frame.Content = diskFileManagement;
            defaultPage = diskFileManagement;
            diskFileManagement.LoadDiskFileTree();
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            (System.Windows.Application.Current.MainWindow as MainWindow).EndShowMenuAnimation();
        }


        public void Terminate()
        {
            
        }

        public void ShowDefaultPage()
        {
            if (defaultPage == null)
            {
                btn_StartupMgmtClick(null, null);
            }
            else
            {
                frame.Content = defaultPage;
            }
        }
    }
}
