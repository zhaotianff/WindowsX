using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Master_Zhao.IO;

using static Master_Zhao.IO.Commands.DynamicWallpaperCommands;

namespace Master_Zhao.DynamicDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int S_OK = 0;
        private const int S_FALSE = 1;

        private const int WM_CLOSE = 0x0010;

        private string previousVideoPath = "";
        private string currentVideoPath = "";

        private AnonymousPipeClient client = new AnonymousPipeClient();



        public MainWindow()
        {
            InitializeComponent();
    
            SetMute(App.Mute);
            SetRepeat(App.Repeat);
            SetVideo(App.VideoPath);

            InitAnoymousPepeClient(App.PipeHandleAsString);
        }

        private void InitAnoymousPepeClient(string handleAsString)
        {
            client.SetReceiveMessageHandler(DealWithMessage);
            client.StartClient(handleAsString);
        }

        private void CloseAnoymousPepeClient()
        {
            client?.CloseClient();
        }

        public void DealWithMessage(string message)
        {
            switch(message)
            {
                case DYWALLPAPER_EXIT:           
                    break;
                case DYWALLPAPER_MUTE:
                    break;
                case DYWALLPAPER_PAUSE:
                    break;
                case DYWALLPAPER_PLAY:
                    break;
                case DYWALLPAPER_RECOVERLAST:
                    RecoverLast();
                    break;
                case DYWALLPAPER_REPEAT:
                    break;
                case DYWALLPAPER_SETVIDEO:
                    break;
                default:
                    break;
            }

            HandleExceptionMessage(message);
        }

        public void HandleExceptionMessage(string message)
        {
            if(string.IsNullOrEmpty(message))
            {
                Environment.Exit(0);
            }
        }

        private void RecoverLast()
        {
            if (string.IsNullOrEmpty(previousVideoPath))
            {
                Environment.Exit(0);
            }
            else
            {
                SetVideo(previousVideoPath);
            }
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
                if (!string.IsNullOrEmpty(currentVideoPath))
                    previousVideoPath = currentVideoPath;

                if (this.Visibility == Visibility.Hidden)
                    this.Visibility = Visibility.Visible;
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

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            HwndSource hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            hwndSource.AddHook(HwndSourceHook);
        }

        private IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch(msg)
            {
                case WM_CLOSE:
                    CloseAnoymousPepeClient();
                    break;
            }
            return IntPtr.Zero;
        }
    }
}
