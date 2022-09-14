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
        private BossKey bossKey;

        public BossKeySetting()
        {
            InitializeComponent();
        }

        private void cbx_Running_Checked(object sender, RoutedEventArgs e)
        {
            LoadProcessList(list_TasksRunning);
            if (bossKey != null)
            {
                bossKey.BossKeyType = BossKeyType.SwitchToTask;
            }
        }

        private void cbx_Kill_Checked(object sender, RoutedEventArgs e)
        {
            LoadProcessList(list_TasksKill);
            if (bossKey != null)
            {
                bossKey.BossKeyType = BossKeyType.KillTask;
            }
        }

        private void list_TasksKill_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(bossKey != null && list_TasksKill.SelectedIndex > -1)
            {
                bossKey.KillProcess = list_TasksKill.SelectedItem as ProcessInfo;
            }
        }

        private void list_TasksRunning_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (bossKey != null && list_TasksRunning.SelectedIndex > -1)
            {
                bossKey.SwitchProcess = list_TasksRunning.SelectedItem as ProcessInfo;
            }
        }

        private void cbx_SwitchToDesktop_Checked(object sender, RoutedEventArgs e)
        {
            if (bossKey != null)
            {
                bossKey.BossKeyType = BossKeyType.SwitchToDesktop;
            }
        }

        private void cbx_Execute_Checked(object sender, RoutedEventArgs e)
        {
            if (bossKey != null)
            {
                bossKey.BossKeyType = BossKeyType.Exec;
                bossKey.ExecPath = this.tbox_ExecPath.Text.Trim();
            }
        }

        private void tbox_ExecPath_LostFocus(object sender, RoutedEventArgs e)
        {
            if(bossKey != null)
                bossKey.ExecPath = this.tbox_ExecPath.Text.Trim();
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (bossKey != null)
            {
                bossKey.ExecPath = this.tbox_ExecPath.Text.Trim();
            }
        }

        private void browseProgram_Click(object sender, RoutedEventArgs e)
        {
            bossKey.ExecPath = DialogHelper.BrowserSingleFile();
            tbox_ExecPath.Text = bossKey.ExecPath;
        }

        private void cbx_AutoCoding_Checked(object sender, RoutedEventArgs e)
        {
            if (bossKey != null)
            {
                bossKey.BossKeyType = BossKeyType.AutoCoding;
                TextRange tr = new TextRange(rtbox.Document.ContentStart, rtbox.Document.ContentEnd);
                bossKey.AutoCodingContent = tr.Text;
            }
        }

        private void rtbox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextRange tr = new TextRange(rtbox.Document.ContentStart, rtbox.Document.ContentEnd);
            bossKey.AutoCodingContent = tr.Text;
        }

        private void cbx_EnableBossKey_Checked(object sender, RoutedEventArgs e)
        {
            if (bossKey == null)
                bossKey = new BossKey();

            bossKey.StartBossKey();
            bossKey.BossKeyType = BossKeyType.SwitchToDesktop; //default
        }

        private void cbx_EnableBossKey_Unchecked(object sender, RoutedEventArgs e)
        {
            bossKey.StopBosKey();
            bossKey.Close();
            bossKey = null;
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
                            var processInfo = new ProcessInfo()
                            {
                                Name = processName,
                                Path = strArray[0],
                                MainWindowHwnd = process.MainWindowHandle,
                                MainWindowText = process.MainWindowTitle,
                                Pid = process.Id
                            };
                            listBox.Items.Add(processInfo);
                        }
                    }
                }
            }
        }

        public void CloseBossKey()
        {
            bossKey?.Close();
        }
    }
}
