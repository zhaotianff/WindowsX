using WindowsX.Shell.MessageBoxEx;
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

namespace WindowsX.Shell.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var result = await WaitMessageBox.Show("标题", "内容", "保留");
            MessageBox.Show(result.ToString());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var exePath = System.IO.Path.GetFullPath("../../../../WindowsX.DynamicDesktop/bin/Debug/net5.0-windows/WindowsX.DynamicDesktop.exe");
            var videoPath = System.IO.Path.GetFullPath("../../../../WindowsX.Shell/bin/Debug/net5.0-windows/res/dynamic wallpaper/default/default.mp4");

            System.Diagnostics.Process.Start(exePath, "\"" + videoPath + "\"");
        }
    }
}
