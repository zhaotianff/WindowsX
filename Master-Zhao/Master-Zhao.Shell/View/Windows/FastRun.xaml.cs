using Master_Zhao.Config;
using Master_Zhao.Config.Model;
using Master_Zhao.Shell.Controls;
using Master_Zhao.Shell.PInvoke;
using Master_Zhao.Shell.Util;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Master_Zhao.Shell.Windows
{
    /// <summary>
    /// Interaction logic for FastRun.xaml
    /// </summary>
    public partial class FastRun : Window
    {
        public int SelectedIndex { get; set; } = 0;
        private bool IsFirstRun { get; set; } = true;

        public FastRun()
        {
            InitializeComponent();

            LoadFastRunList();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch
            {

            }
        }

        public int RegisterHotKey()
        {
            var result = PInvoke.SystemTool.RegisterFastRunHotKey(new WindowInteropHelper(this).Handle);
            var errorCode = PInvoke.Kernel32.GetLastError();
            return errorCode;
        }

        public int UnregisterHotKey()
        {
            var result = PInvoke.SystemTool.UnRegisterFastRunHotKey();
            var errorCode = PInvoke.Kernel32.GetLastError();
            return errorCode;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetLeft(img_DragArea, (canvas.ActualWidth - img_DragArea.Width) / 2);
            Canvas.SetTop(img_DragArea, (canvas.ActualHeight - img_DragArea.Height) / 2);
            this.Visibility = Visibility.Hidden;
        }


        private void LoadFastRunList()
        {

            /*
             * <local:FastRunButton ContentRadiusX="48" ContentRadiusY="48" Center="50,50" VerticalContentAlignment="Bottom" Foreground="White" FontSize="20" Content="abc" Width="100" Height="100" ImagePath="../Icon/system.png" HostCanvas="{Binding ElementName=canvas}">
            </local:FastRunButton>/*
            */

            canvas.Children.Clear();

            //var list = GetTestList();
            var list = GlobalConfig.Instance.ToolsConfig.FastRunConfig.FastRunList;

            foreach (var item in list)
            {
                FastRunButton fastRunButton = new FastRunButton();
                fastRunButton.FastRunItem = item;
                fastRunButton.ContentRadiusX = 48;
                fastRunButton.ContentRadiusY = 48;
                fastRunButton.Center = new Point(50, 50);
                fastRunButton.VerticalAlignment = VerticalAlignment.Bottom;
                fastRunButton.Foreground = Brushes.White;
                fastRunButton.FontSize = 20;
                fastRunButton.Content = item.Name;
                fastRunButton.Width = 100;
                fastRunButton.Height = 100;
                fastRunButton.ImagePath = @"C:\Users\Administrator\Desktop\compu.png";
                fastRunButton.HostCanvas = canvas;
                fastRunButton.Click += FastRunButton_Click;
                canvas.Children.Add(fastRunButton);
            }

            (canvas.Children[0] as FastRunButton).IsSelected = true;
        }

        private void FastRunButton_Click(object sender, RoutedEventArgs e)
        {
            var fastRunButton = sender as FastRunButton;

            if (fastRunButton != null)
                System.Diagnostics.Process.Start(fastRunButton.FastRunItem.Path);
        }

        #region Test Code
        private List<FastRunItem> GetTestList()
        {
            var list = new List<FastRunItem>();
    
            FastRunItem fastRunItem1 = new FastRunItem();
            fastRunItem1.Name = "notepad";
            fastRunItem1.RunType = FastRunType.Applicataion;
            fastRunItem1.Path = "C:\\windows\\system32\\notepad.exe";
            list.Add(fastRunItem1);

            FastRunItem fastRunItem2 = new FastRunItem();
            fastRunItem2.Name = "cmd";
            fastRunItem2.RunType = FastRunType.Applicataion;
            fastRunItem2.Path = "C:\\windows\\system32\\cmd.exe";
            list.Add(fastRunItem2);

            FastRunItem fastRunItem3 = new FastRunItem();
            fastRunItem3.Name = "control";
            fastRunItem3.RunType = FastRunType.Applicataion;
            fastRunItem3.Path = "C:\\windows\\system32\\control.exe";
            list.Add(fastRunItem3);

            FastRunItem fastRunItem4 = new FastRunItem();
            fastRunItem4.Name = "calc";
            fastRunItem4.RunType = FastRunType.Applicataion;
            fastRunItem4.Path = "C:\\windows\\system32\\calc.exe";
            list.Add(fastRunItem4);

            return list;
        }

        #endregion

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            hwndSource.AddHook(HwndProc);
        }

        private IntPtr HwndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch(msg)
            {
                case PInvoke.User32.WM_INPUT:
                    if(Keyboard.IsKeyDown(Key.LeftAlt))
                    {
                        if(this.Visibility == Visibility.Hidden)
                        {
                            this.Visibility = Visibility.Visible;
                            var point = new PInvoke.POINT();
                            PInvoke.User32.GetCursorPos(ref point);
                            this.Left = point.x - this.Width / 2;
                            this.Top = point.y - this.Height / 2;
                            break;
                        }
                    }
                    
                    if(Keyboard.IsKeyUp(Key.LeftAlt))
                    {    
                        this.Visibility = Visibility.Hidden;
                        break;
                    }

                    DealWithKeyPress();
                    break;
            }
            return IntPtr.Zero;
        }

        private void DealWithKeyPress()
        {
            if (this.Visibility == Visibility.Hidden)
                return;

            if (Keyboard.IsKeyDown(Key.D1))
            {
                SelectFastRunItem(0, true);
                this.Visibility = Visibility.Hidden;
            }

            if (Keyboard.IsKeyDown(Key.D2))
            {
                SelectFastRunItem(1, true);
                this.Visibility = Visibility.Hidden;
            }

            if (Keyboard.IsKeyDown(Key.D3))
            {
                SelectFastRunItem(2, true);
                this.Visibility = Visibility.Hidden;
            }

            if (Keyboard.IsKeyDown(Key.D4))
            {
                SelectFastRunItem(3, true);
                this.Visibility = Visibility.Hidden;
            }
        }

        private void SelectFastRunItem(int index, bool isRun = false)
        {
           for(int i = 0;i<canvas.Children.Count;i++)
            {
                if(index == i)
                {
                    var fastRunButton = (canvas.Children[i] as FastRunButton);

                    fastRunButton.IsSelected = true;

                    if (isRun)
                    {
                        fastRunButton.Dispatcher.Invoke(() => {
                            fastRunButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                            this.Visibility = Visibility.Hidden;
                        });

                    }
                }
                else
                {
                    (canvas.Children[i] as FastRunButton).IsSelected = false;
                }
            }
        }
    }
}
