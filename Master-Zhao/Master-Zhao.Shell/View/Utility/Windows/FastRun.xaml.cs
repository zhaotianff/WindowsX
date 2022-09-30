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
        private bool isExecuted = false;
        private bool hasPressedVKMENU = false;

        public int SelectedIndex { get; set; } = 0;
        private bool IsFirstRun { get; set; } = true;

        private System.Windows.Point dragStartPoint = new Point();

        public FastRun()
        {
            InitializeComponent();

            LoadFastRunList();
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

        public void LoadFastRunList()
        {

            var list = GlobalConfig.Instance.ToolsConfig.FastRunConfig.FastRunList;

            for(int i = 0;i<list.Count;i++)
            {
                if(i < grid_item.Children.Count)
                {
                    var pathButton = grid_item.Children[i] as PathButton;
                    pathButton.FastRunItem = list[i];
                    pathButton.MouseDown -= PathButton_MouseDown;
                    pathButton.MouseDown += PathButton_MouseDown;
                    pathButton.MouseEnter -= PathButton_MouseEnter;
                    pathButton.MouseEnter += PathButton_MouseEnter;
                    pathButton.MouseLeave -= PathButton_MouseLeave;
                    pathButton.MouseLeave += PathButton_MouseLeave;

                    var image = grid_icon.Children[i] as Image;
                    image.Source = ImageHelper.GetBitmapImageFromLocalFile(GetCachedIconPath(list[i].Path));
                }
                else
                {
                    break;
                }
            }
        }

        private void PathButton_MouseLeave(object sender, MouseEventArgs e)
        {
            var pathButton = sender as PathButton;

            if (pathButton != null)
            {
                this.ColorCircle.BeginAnimation(GradientStop.ColorProperty, GetAnimation(GlobalData.Instance.BorderColorTran, TimeSpan.FromMilliseconds(300)));
            }
        }

        private void PathButton_MouseEnter(object sender, MouseEventArgs e)
        {
            var pathButton = sender as PathButton;

            if(pathButton != null)
            {
                this.lbl_Name.Content = pathButton.FastRunItem.Name; 
                this.ColorCircle.BeginAnimation(GradientStop.ColorProperty, GetAnimation(GlobalData.Instance.BorderColor, TimeSpan.FromMilliseconds(300)));
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

        private void PathButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pathButton = sender as PathButton;

            if (pathButton != null && e.LeftButton == MouseButtonState.Pressed)
            {
                ExecuteFastRunItem(pathButton.FastRunItem.Path);
            }
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
      

        private string GetCachedIconPath(string path)
        {
            var temp = System.IO.Path.Combine(System.IO.Path.GetTempPath(),"Master-Zhao");
            var iconPath = System.IO.Path.Combine(temp, System.IO.Path.GetFileNameWithoutExtension(path) + ".png");
            if (System.IO.Directory.Exists(temp) == false)
                System.IO.Directory.CreateDirectory(temp);

            if(System.IO.File.Exists(iconPath) == false)
            {
                IntPtr hIcon = IntPtr.Zero;
                if(IconTool.ExtractFirstIconFromFile(path, true, ref hIcon))
                {
                    var bi = ImageHelper.GetBitmapImageFromHIcon(hIcon);
                    ImageHelper.SaveBitmapImageToFile(bi, iconPath);
                }          
            }

            return iconPath;
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
            //bug : first run
            switch(msg)
            {
                case PInvoke.User32.WM_INPUT:
                    if(InputTool.IsKeyPressed(InputTool.VK_MENU))
                    {
                        if(this.Visibility == Visibility.Hidden && isExecuted == false)
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
            for (int i = 0; i < grid_item.Children.Count; i++)
            {
                if (index == i)
                {
                    var pathButton = grid_item.Children[i] as PathButton;

                    pathButton.IsSelected = true;
                    if (isRun)
                    {
                        pathButton.Dispatcher.Invoke(()=> {
                            ExecuteFastRunItem(pathButton.FastRunItem.Path);
                        });
                    }
                    pathButton.IsSelected = false;
                }
                else
                {
                    (grid_item.Children[i] as PathButton).IsSelected = false;
                }
            }
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
            if(e.RightButton == MouseButtonState.Pressed)
            {
                Point dragEndPoint = e.GetPosition(this);
                Vector vector = dragEndPoint - dragStartPoint;
                this.Left += vector.X;
                this.Top += vector.Y;
            }
        }
    }
}
