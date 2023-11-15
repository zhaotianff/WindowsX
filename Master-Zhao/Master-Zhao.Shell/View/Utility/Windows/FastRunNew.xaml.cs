using Master_Zhao.Config.Model;
using Master_Zhao.Config;
using Master_Zhao.Shell.Controls;
using Master_Zhao.Shell.PInvoke;
using Master_Zhao.Shell.Util;
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
using Master_Zhao.Shell.Model.Utility;
using System.Collections.ObjectModel;

namespace Master_Zhao.Shell.View.Utility.Windows
{
    /// <summary>
    /// FastRunNew.xaml 的交互逻辑
    /// </summary>
    public partial class FastRunNew : Window
    {
        private bool isExecuted = false;
        private bool hasPressedVKMENU = false;

        public int SelectedIndex { get; set; } = 0;
        private bool IsFirstRun { get; set; } = true;

        private System.Windows.Point dragStartPoint = new Point();

        ObservableCollection<FastRunItem> fastRunItemList = new ObservableCollection<FastRunItem>();

        public FastRunNew()
        {
            InitializeComponent();
        }

        public void LoadFastRunList(ObservableCollection<FastRunItem> fastRunItemList)
        {
            this.fastRunItemList = fastRunItemList;
            this.menu.ItemsSource = null;
            this.menu.ItemsSource = fastRunItemList;
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
            this.Visibility = Visibility.Hidden;
        }

        private void ExecuteFastRunItem(string path)
        {
            var psInfo = new System.Diagnostics.ProcessStartInfo();
            psInfo.UseShellExecute = true;
            psInfo.FileName = path;
            System.Diagnostics.Process.Start(psInfo);
            this.Visibility = Visibility.Hidden;
            isExecuted = true;
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
                        if (this.Visibility == Visibility.Hidden && isExecuted == false)
                        {
                            ShowWindowWithAnimation();
                            var point = new PInvoke.POINT();
                            PInvoke.User32.GetCursorPos(ref point);
                            this.Left = point.x - this.Width / 2;
                            this.Top = point.y - this.Height / 2;
                            hasPressedVKMENU = true;
                            break;
                        }

                        DealWithKeyPress();
                    }
                    else
                    {
                        if (hasPressedVKMENU == true)
                        {
                            this.Visibility = Visibility.Hidden;
                            isExecuted = false;
                            hasPressedVKMENU = false;
                        }
                        break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void DealWithKeyPress()
        {
            if (this.Visibility == Visibility.Hidden)
                return;

            //deal with tab pressed

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
           var item = fastRunItemList[index];
           ExecuteFastRunItem(item.Path);
        }

        public void ShowWindowWithAnimation()
        {
            this.Visibility = Visibility.Visible;
        }

        private void Window_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragStartPoint = e.GetPosition(this);
        }

        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                Point dragEndPoint = e.GetPosition(this);
                Vector vector = dragEndPoint - dragStartPoint;
                this.Left += vector.X;
                this.Top += vector.Y;
            }
        }

        private void menu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.menu.SelectedItem != null)
            {
                var selectedItem = this.menu.SelectedItem as FastRunItem;

                if(selectedItem != null)
                {
                    var index = fastRunItemList.IndexOf(selectedItem);
                    SelectFastRunItem(index, true);
                }
            }
        }

        private void menu_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point pt = e.GetPosition(this);
            var item = System.Windows.Media.VisualTreeHelper.HitTest(this, pt);

            var listBoxItem = item.VisualHit.FindParent<CircularMenuItem>();

            if (listBoxItem != null)
            {
                menu.StatusText = listBoxItem.MenuTxt;
            }
        }

        private System.Windows.Media.Animation.ColorAnimation GetAnimation(Color to, TimeSpan duration)
        {
            System.Windows.Media.Animation.ColorAnimation animation = new System.Windows.Media.Animation.ColorAnimation();
            animation.Duration = duration;
            animation.To = to;
            animation.AutoReverse = false;
            return animation;
        }

        private void menu_MouseLeave(object sender, MouseEventArgs e)
        {
            this.ColorCircle.BeginAnimation(GradientStop.ColorProperty, GetAnimation(GlobalData.Instance.BorderColorTran, TimeSpan.FromMilliseconds(300)));
        }

        private void menu_MouseEnter(object sender, MouseEventArgs e)
        {
            this.ColorCircle.BeginAnimation(GradientStop.ColorProperty, GetAnimation(GlobalData.Instance.BorderColor, TimeSpan.FromMilliseconds(300)));
        }
    }
}
