using Master_Zhao.Shell.Model.BossKey;
using Master_Zhao.Shell.Model.Process;
using Master_Zhao.Shell.Util;
using Master_Zhao.Shell.View.Utility.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Master_Zhao.Shell.View.Pages
{
    /// <summary>
    /// BossKey.xaml 的交互逻辑
    /// </summary>
    public partial class BossKeySetting : Page
    {
        private BossKeyType bossKeyType = BossKeyType.SwitchToDesktop;
        private bool isEnableBossKey = false;
        private BossKey bossKey = new BossKey();
        private List<ProcessInfo> runningProcessList = new List<ProcessInfo>();
        private List<ProcessInfo> killProcessList = new List<ProcessInfo>();

        public BossKeySetting()
        {
            InitializeComponent();
        }

        private void cbx_Running_Checked(object sender, RoutedEventArgs e)
        {
            LoadProcessList(list_TasksRunning);
            bossKeyType = BossKeyType.SwitchToTask;
        }

        private void cbx_Kill_Checked(object sender, RoutedEventArgs e)
        {
            LoadProcessList(list_TasksKill);
            bossKeyType = BossKeyType.KillTask;
        }

        private void cbx_SwitchToDesktop_Checked(object sender, RoutedEventArgs e)
        {
            bossKeyType = BossKeyType.SwitchToDesktop;
        }

        private void cbx_Execute_Checked(object sender, RoutedEventArgs e)
        {
            bossKeyType = BossKeyType.Exec;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bossKey.ExecPath = this.tbox_ExecPath.Text.Trim();
        }

        private void cbx_EnableBossKey_Checked(object sender, RoutedEventArgs e)
        {
            isEnableBossKey = !isEnableBossKey;
            if(isEnableBossKey)
            {
                bossKey.StartBossKey();
            }
            else
            {
                bossKey.StopBosKey();
            }
        }

        private void LoadProcessList(ListBox listBox)
        {
            var processes = Process.GetProcesses();

            listBox.Items.Clear();            

            foreach (var process in processes)
            {
                if (PInvoke.DesktopTool.CanAddToTaskBar(process.MainWindowHandle))
                {
                    IntPtr ptr = PInvoke.DesktopTool.GetProcessNameFomrHwnd(process.MainWindowHandle);

                    if (ptr == IntPtr.Zero)
                        continue;

                    var str = Marshal.PtrToStringAuto(ptr);

                    if (!string.IsNullOrEmpty(str))
                    {
                        var strArray = str.Split(";");
                        if (strArray.Length > 1)
                        {
                            var processName = strArray[1];
                            if (string.IsNullOrEmpty(processName))
                            {
                                processName = System.IO.Path.GetFileNameWithoutExtension(strArray[0]);
                            }
                            var processInfo = new ProcessInfo() { Name = processName, Path = strArray[0] };
                            listBox.Items.Add(processInfo);
                        }
                    }
                }
            }
        }
    }
}
