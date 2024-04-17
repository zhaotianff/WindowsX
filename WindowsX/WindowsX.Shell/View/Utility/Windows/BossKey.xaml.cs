using WindowsX.Shell.Model.BossKey;
using WindowsX.Shell.PInvoke;
using WindowsX.Shell.Util;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace WindowsX.Shell.View.Utility.Windows
{
    /// <summary>
    /// BossKey.xaml 的交互逻辑
    /// </summary>
    public partial class BossKey : Window
    {
        public BossKeyType BossKeyType { get; set; }    
        public string ExecPath { get; set; }
        public string AutoCodingContent { get; set; }
        public Process SwitchProcess { get; set; }
        public Process KillProcess { get; set; }


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
                case PInvoke.User32.WM_HOTKEY:
                    //wParam is hot key id
                    ExecuteBossKey(wParam);
                    break;
            }
            return IntPtr.Zero;
        }

        private void ExecuteBossKey(IntPtr wParam = default(IntPtr))
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
                    if ((int)wParam == 1)
                    {
                        AutoCoding();
                    }
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
                PInvoke.DesktopTool.SwitchToWindow(SwitchProcess.MainWindowHandle);  //TODO: minimize state 
        }

        private void KillTask()
        {
            if (KillProcess != null)
                KillProcess.Kill();
        }

        private void Exec()
        {
            if (!string.IsNullOrEmpty(ExecPath))
            {
                try
                {
                    ProcessHelper.Execute(ExecPath);
                }
                catch
                {
                    //log
                }
            }
        }

        private void AutoCoding()
        {
            PInvoke.InputTool.AutoCode(AutoCodingContent);
        }

        public void StartBossKey()
        {
            //Use RegisterHotKey instead
            this.Show();
            var hwnd = new WindowInteropHelper(this).Handle;
            //SystemTool.RegisterFastRunHotKey(hwnd);
            SystemTool.RegisterBossKeyHotKey(hwnd,SystemTool.MOD_CONTROL,'I',1);
            SystemTool.RegisterBossKeyHotKey(hwnd, SystemTool.MOD_ALT, 'Q',2);
            HwndSource hwndSource = HwndSource.FromHwnd(hwnd);
            hwndSource.AddHook(HwndProc);
            this.Hide();
        }

        public void StopBosKey()
        {
            this.Show();
            var hwnd = new WindowInteropHelper(this).Handle;
            //SystemTool.UnRegisterFastRunHotKey();
            SystemTool.UnRegisterBossKeyHotKey(hwnd,1);
            SystemTool.UnRegisterBossKeyHotKey(hwnd, 2);
            HwndSource hwndSource = HwndSource.FromHwnd(hwnd);
            hwndSource.RemoveHook(HwndProc);
            this.Hide();
        }
    }
}
