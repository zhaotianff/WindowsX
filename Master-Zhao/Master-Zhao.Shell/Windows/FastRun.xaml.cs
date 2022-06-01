using Master_Zhao.Shell.Controls;
using Master_Zhao.Shell.Model.FastRun;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetLeft(img_DragArea, (canvas.ActualWidth - img_DragArea.Width) / 2);
            Canvas.SetTop(img_DragArea, (canvas.ActualHeight - img_DragArea.Height) / 2);
        }

        private void LoadFastRunList()
        {

            /*
             * <local:FastRunButton ContentRadiusX="48" ContentRadiusY="48" Center="50,50" VerticalContentAlignment="Bottom" Foreground="White" FontSize="20" Content="abc" Width="100" Height="100" ImagePath="../Icon/system.png" HostCanvas="{Binding ElementName=canvas}">
            </local:FastRunButton>/*
            */

            canvas.Children.Clear();

            var list = GetTestList();

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

            abc.Source = ImageHelper.GetResourceBitmapImage("../Icon/logo.png");
        }

        private void FastRunButton_Click(object sender, RoutedEventArgs e)
        {
            var fastRunButton = sender as FastRunButton;

            if (fastRunButton != null)
                System.Diagnostics.Process.Start(fastRunButton.FastRunItem.Path);
        }

        private List<FastRunItem> GetTestList()
        {
            var list = new List<FastRunItem>();

            for (int i = 0; i < 4; i++)
            {
                FastRunItem fastRunItem = new FastRunItem();
                fastRunItem.Name = "test" + i;
                fastRunItem.RunType = FastRunType.Applicataion;
                fastRunItem.Path = "C:\\windows\\system32\\notepad.exe";
                list.Add(fastRunItem);
            }

            return list;
        }

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
                //TODO
                case PInvoke.User32.WM_INPUT:
                    MessageBox.Show("wm_input");
                    break;
            }
            return IntPtr.Zero;
        }
    }
}
