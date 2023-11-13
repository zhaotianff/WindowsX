//#define FASTRUN_PREVIOUS

using Master_Zhao.Config;
using Master_Zhao.Config.Model;
using Master_Zhao.Shell.View.Utility.Windows;
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
#if FASTRUN_PREVIOUS
        FastRun fastRun;
#else
        FastRunNew fastRunNew;
#endif

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
#if FASTRUN_PREVIOUS
            if (fastRun == null)
            {
                fastRun = new FastRun();
                fastRun.Visibility = Visibility.Visible;
                fastRun.Visibility = Visibility.Hidden;
            }
#else
            if (fastRunNew == null)
            {
                fastRunNew = new FastRunNew();
                fastRunNew.Visibility = Visibility.Visible;
                fastRunNew.Visibility = Visibility.Hidden;
            }
#endif
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
#if FASTRUN_PREVIOUS
            fastRun?.Close();
#else
            fastRunNew?.Close();
#endif
        }

        private void cbxFastrun_Checked(object sender, RoutedEventArgs e)
        {
#if FASTRUN_PREVIOUS
            fastRun.RegisterHotKey();
#else
            fastRunNew.RegisterHotKey();
#endif
        }

        private void cbxFastrun_Unchecked(object sender, RoutedEventArgs e)
        {
#if FASTRUN_PREVIOUS
            fastRun.UnregisterHotKey();
#else
            fastRunNew.UnregisterHotKey();
#endif
        }

        private void OnFastRunItemConfigChanged(FastRunConfigItem fastRunItem)
        {
            //TODO refresh part
            //TODO dynamice refresh
            var fastRunList = GlobalConfig.Instance.ToolsConfig.FastRunConfig.FastRunList;
            fastRunList[0].Path = fastrun_item1.FastRunPath;
            fastRunList[1].Path = fastrun_item2.FastRunPath;
            fastRunList[2].Path = fastrun_item3.FastRunPath;
            fastRunList[3].Path = fastrun_item4.FastRunPath;
            fastRunList[0].Name = fastrun_item1.FastRunName;
            fastRunList[1].Name = fastrun_item2.FastRunName;
            fastRunList[2].Name = fastrun_item3.FastRunName;
            fastRunList[3].Name = fastrun_item4.FastRunName;
#if FASTRUN_PREVIOUS
            fastRun.LoadFastRunList();
#else
            fastRunNew.LoadFastRunList();
#endif
        }
    }
}
