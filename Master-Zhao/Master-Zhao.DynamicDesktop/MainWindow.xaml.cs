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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Master_Zhao.DynamicDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
    }
}
