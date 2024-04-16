using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using WindowsX.IO;

using static WindowsX.IO.Commands.DynamicWallpaperCommands;

namespace WindowsX.DynamicDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            client.SetReceiveMessageHandler(DispatcherHandleMessage);
            client.StartClient(handleAsString);
        }

        private void CloseAnoymousPepeClient()
        {
            client?.CloseClient();
        }

        public void DispatcherHandleMessage(string message)
        {
            this.Dispatcher.Invoke(new Action<string>(HandleMessage), new object[] { message });
        }

        public void HandleMessage(string message)
        {
            HandleExceptionMessage(message);

            (string cmd, string[] values) = ParseMessage(message);

            //MessageBox.Show(cmd);
            //MessageBox.Show(values[0]);

            switch(cmd)
            {
                case DYWALLPAPER_EXIT:           
                    break;
                case DYWALLPAPER_MUTE:
                    SetMute(values[0]);
                    break;
                case DYWALLPAPER_PAUSE:
                    Pause();
                    break;
                case DYWALLPAPER_PLAY:
                    Play();
                    break;
                case DYWALLPAPER_RECOVERLAST:
                    RecoverLast();
                    break;
                case DYWALLPAPER_REPEAT:
                    SetRepeat(values[0]);
                    break;
                case DYWALLPAPER_SETVIDEO:
                    SetVideo(values[0]);
                    break;
                default:
                    break;
            }    
        }

        public Tuple<string,string[]> ParseMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return new Tuple<string, string[]>("", new string[] { });

            var array = message.Split(COMMANDSPLITCHAR);
            if (array.Length < 1)
                return new Tuple<string, string[]>("", new string[] { });

            var tempArray = new string[array.Length - 1];
            Array.Copy(array, 1, tempArray, 0, array.Length - 1);
            return new Tuple<string, string[]>(array[0], tempArray);
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

        private void SetMute(string isMute)
        {
            media.IsMuted = isMute == S_OK;
        }

        private void SetRepeat(string isRepeat)
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
