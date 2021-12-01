using Silver_Arowana.Shell.PInvoke;
using Silver_Arowana.Shell.Util;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Silver_Arowana.Shell.Windows
{
    /// <summary>
    /// ImageWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ImageWindow : TianXiaTech.BlurWindow
    {
        System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();

        private string currentBackground = "";

        public ImageWindow()
        {
            InitializeComponent();
            InitTimer();
        }

        private void InitTimer()
        {
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder(currentBackground);
            DesktopTool.SetBackground(sb);
            this.Activate();
            timer.IsEnabled = false;
        }

        public void SetImageUrl(string url,string currentBackground)
        {       
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(url);
            bi.EndInit();
            image.Source = bi;

            this.currentBackground = currentBackground;
        }

        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            if (image.Source == null)
                return;

            var filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + $"\\Temp\\silver_{Guid.NewGuid()}.jpg";
            UIElementHelper.SaveImageSourceAsFile((BitmapSource)image.Source, filePath);

            //TODO
            StringBuilder sb = new StringBuilder(filePath);
            DesktopTool.SetBackground(sb);
            DesktopTool.SwitchToDesktop();

            timer.IsEnabled = true;
        }

        private void btnSet_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
