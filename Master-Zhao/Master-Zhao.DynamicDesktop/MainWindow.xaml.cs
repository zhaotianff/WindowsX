using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        private const int WM_SETVIDEO = WM_USER + 0x103;
        private const int WM_MUTE = WM_USER + 0x104;
        private const int WM_REPEAT = WM_USER + 0x105;
        private const int WM_GETVIDEO = WM_USER + 0x106;

        private const int S_OK = 0;
        private const int S_FALSE = 1;

        private string currentVideoPath = "";

        public MainWindow()
        {
            InitializeComponent();
    
            SetMute(App.Mute);
            SetRepeat(App.Repeat);
            SetVideo(App.VideoPath);
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            media.Stop();
            media.Play();
        }   

        private void Pause()
        {
            if (media.CanPause)
                media.Pause();
        }

        private void Play()
        {
            if (media.HasVideo)
                media.Play();
        }

        private void SetVideo(string path)
        {
            if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
            {
                currentVideoPath = path;
                media.Source = new Uri(path, UriKind.Absolute);
                media.Play();
            }
        }

        private void SetMute(int isMute)
        {
            media.IsMuted = isMute == S_OK;
        }

        private void SetRepeat(int isRepeat)
        {
            if (isRepeat == S_OK)
            {
                media.MediaEnded += Media_MediaEnded;
            }
            else
            {
                //即使没有handler也不会异常
                media.MediaEnded -= Media_MediaEnded;
            }
        }
    }
}
