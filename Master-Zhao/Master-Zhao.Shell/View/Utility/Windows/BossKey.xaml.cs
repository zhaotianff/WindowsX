using Master_Zhao.Shell.Model.BossKey;
using Master_Zhao.Shell.Model.Process;
using Master_Zhao.Shell.PInvoke;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Master_Zhao.Shell.View.Utility.Windows
{
    /// <summary>
    /// BossKey.xaml 的交互逻辑
    /// </summary>
    public partial class BossKey : Window
    {
        private bool isExecuted = false;
        private BossKeyType bossKeyType;
        
        public string ExecPath { get; set; }
        public List<ProcessInfo> SwitchProcessList { get; set; } = new List<ProcessInfo>();
        public int SwitchProcessIndex { get; set; }
        public List<Tuple<string, string>> KillProcessList { get; set; } = new List<Tuple<string, string>>();


        public BossKey()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            hwndSource.AddHook(HwndProc);
        }

        private IntPtr HwndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //bug : first run
            switch (msg)
            {
                case PInvoke.User32.WM_INPUT:
                    if (InputTool.IsKeyPressed(InputTool.VK_MENU))
                    {
                        if (isExecuted == false)
                        {
                            
                            break;
                        }
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        public void StartBossKey()
        {

        }

        public void StopBosKey()
        {

        }
    }
}
