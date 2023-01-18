using Master_Zhao.Shell.Infrastructure.Navigation;
using Master_Zhao.Shell.View.Pages;
using Master_Zhao.Shell.View.Utility.Pages;
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
    /// Interaction logic for ToolsPage.xaml
    /// </summary>
    public partial class ToolsPage : Page, IPageAction
    {
        private ToggleButton toggleButton = null;
        private FastRunSetting fastRunSetting = new FastRunSetting();
        private BossKeySetting bossKeySetting = new BossKeySetting();
        private ExecuteToolSetting executeToolSetting = new ExecuteToolSetting();
        private Page defaultPage = null;

        public ToolsPage()
        {
            InitializeComponent();
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            (System.Windows.Application.Current.MainWindow as MainWindow).EndShowMenuAnimation();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (toggleButton != null)
                toggleButton.IsChecked = false;

            toggleButton = sender as ToggleButton;
        }

        private void btn_FastRunClick(object sender, RoutedEventArgs e)
        {
            frame.Content = fastRunSetting;
            fastRunSetting.InitFastRun();
            defaultPage = fastRunSetting;
        }

        private void btn_BossKeyClick(object sender, RoutedEventArgs e)
        {
            frame.Content = bossKeySetting;
            defaultPage = bossKeySetting;
        }

        public void Terminate()
        {
            fastRunSetting.CloseFastRun();
            bossKeySetting.CloseBossKey();
        }

        public void ShowDefaultPage()
        {
           if(defaultPage == null)
            {
                btn_FastRunClick(null, null);
            }
           else
            {
                frame.Content = defaultPage;
            }    
        }

        private async void btn_ExecuteToolClick(object sender, RoutedEventArgs e)
        {
            frame.Content = executeToolSetting;
            await executeToolSetting.LoadExecuteListAsync();
        }
    }
}
