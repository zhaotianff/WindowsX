using Master_Zhao.Shell.Model.BossKey;
using Master_Zhao.Shell.Model.Process;
using Master_Zhao.Shell.PInvoke;
using Master_Zhao.Shell.Util;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Master_Zhao.Shell.View.Utility.Windows
{
    /// <summary>
    /// BossKey.xaml 的交互逻辑
    /// </summary>
    public partial class BossKey : Window
    {
        public BossKeyType BossKeyType { get; set; }    
        public string ExecPath { get; set; }
        public string AutoCodingContent { get; set; }
        public ProcessInfo SwitchProcess { get; set; }
        public ProcessInfo KillProcess{ get; set; }


        public BossKey()
        {
            InitializeComponent();
        }

        private IntPtr HwndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case PInvoke.User32.WM_INPUT:
                    if (InputTool.IsKeyPressed(InputTool.VK_CONTROL) && Keyboard.IsKeyDown(Key.I))
                    {
                        ExecuteBossKey();
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void ExecuteBossKey()
        {
            switch(BossKeyType)
            {
                case BossKeyType.SwitchToDesktop:
                    SwitchToDesktop();
                    break;
                case BossKeyType.SwitchToTask:
                    SwitchToTask();
                    break;
                case BossKeyType.KillTask:
                    KillTask();
                    break;
                case BossKeyType.Exec:
                    Exec();
                    break;
                case BossKeyType.AutoCoding:
                    AutoCoding();
                    break;
            }
        }

        private void SwitchToDesktop()
        {
            PInvoke.DesktopTool.SwitchToDesktop();
        }

        private void SwitchToTask()
        {
            if(SwitchProcess != null)
                PInvoke.DesktopTool.SwitchToWindow(SwitchProcess.MainWindowHwnd);
        }

        private void KillTask()
        {

        }

        private void Exec()
        {
            if (!string.IsNullOrEmpty(ExecPath))
            {
                ProcessHelper.Execute(ExecPath);
            }
        }

        private void AutoCoding()
        {

        }

        public void StartBossKey()
        {
            this.Show();
            var hwnd = new WindowInteropHelper(this).Handle;
            SystemTool.RegisterFastRunHotKey(hwnd);
            HwndSource hwndSource = HwndSource.FromHwnd(hwnd);
            hwndSource.AddHook(HwndProc);
            this.Hide();
        }

        public void StopBosKey()
        {
            this.Show();
            var hwnd = new WindowInteropHelper(this).Handle;
            SystemTool.UnRegisterFastRunHotKey();
            HwndSource hwndSource = HwndSource.FromHwnd(hwnd);
            hwndSource.RemoveHook(HwndProc);
            this.Hide();
        }
    }
}
