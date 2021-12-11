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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Master_Zhao.DynamicDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WM_USER = 0x0400;
        private const int WM_PAUSE = WM_USER + 0x100;
        private const int WM_PLAY = WM_USER + 0x101;
        private const int WM_EXIT = WM_USER + 0x102;

        public MainWindow()
        {
            InitializeComponent();
         
            media.IsMuted = App.Mute;
            
            if(App.Repeat)
            {
                media.MediaEnded += Media_MediaEnded;
            }

            if (!string.IsNullOrEmpty(App.VideoPath) && System.IO.File.Exists(App.VideoPath))
            {
                media.Source = new Uri(App.VideoPath, UriKind.Absolute);
                media.Play();
            }
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            media.Stop();
            media.Play();
        }

        public override void BeginInit()
        {
            HwndSource hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            hwndSource.AddHook(HwndSourceHook);
            base.BeginInit();
        }

        public IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //TODO 
            return IntPtr.Zero;
        }

    }
}
