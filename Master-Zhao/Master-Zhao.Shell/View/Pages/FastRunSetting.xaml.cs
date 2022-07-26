using Master_Zhao.Config;
using Master_Zhao.Config.Model;
using Master_Zhao.Shell.Windows;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Master_Zhao.Shell.Pages
{
    /// <summary>
    /// FastRunSetting.xaml 的交互逻辑
    /// </summary>
    public partial class FastRunSetting : Page
    {
        FastRun fastRun;

        public FastRunSetting()
        {
            InitializeComponent();
        }

        public void InitFastRun()
        {
            InitUi();
            InitConfig();
        }

        private void InitUi()
        {
            if (fastRun == null)
            {
                fastRun = new FastRun();
                fastRun.Visibility = Visibility.Visible;
                fastRun.Visibility = Visibility.Hidden;
            }
        }

        private void InitConfig()
        {
            //TODO use binding
            var fastRunList = GlobalConfig.Instance.ToolsConfig.FastRunConfig.FastRunList;
            fastrun_item1.FastRunPath = fastRunList[0].Path;
            fastrun_item2.FastRunPath = fastRunList[1].Path;
            fastrun_item3.FastRunPath = fastRunList[2].Path;
            fastrun_item4.FastRunPath = fastRunList[3].Path;
            fastrun_item1.OnFastRunItemConfigChanged += OnFastRunItemConfigChanged;
            fastrun_item2.OnFastRunItemConfigChanged += OnFastRunItemConfigChanged;
            fastrun_item3.OnFastRunItemConfigChanged += OnFastRunItemConfigChanged;
            fastrun_item4.OnFastRunItemConfigChanged += OnFastRunItemConfigChanged;
        }

        public void CloseFastRun()
        {
            fastRun?.Close();
        }

        private void cbxFastrun_Checked(object sender, RoutedEventArgs e)
        {
            fastRun.RegisterHotKey();
        }

        private void cbxFastrun_Unchecked(object sender, RoutedEventArgs e)
        {
            fastRun.UnregisterHotKey();
        }

        private void OnFastRunItemConfigChanged(FastRunItem fastRunItem)
        {
            //TODO refresh part
            var fastRunList = GlobalConfig.Instance.ToolsConfig.FastRunConfig.FastRunList;
            fastRunList[0].Path = fastrun_item1.FastRunPath;
            fastRunList[1].Path = fastrun_item2.FastRunPath;
            fastRunList[2].Path = fastrun_item3.FastRunPath;
            fastRunList[3].Path = fastrun_item4.FastRunPath;

            //TODO update name
            fastRun.LoadFastRunList();
        }
    }
}
