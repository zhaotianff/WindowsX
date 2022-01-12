using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Master_Zhao.Shell.PInvoke;

namespace Master_Zhao.Shell.Pages
{
    /// <summary>
    /// DesktopSetting.xaml 的交互逻辑
    /// </summary>
    public partial class DesktopBeautify : Page
    {
        private ToggleButton toggleButton = null;
        private static readonly string StaticWallpaperName = "StaticWallpaper.xaml";
        private static readonly string DynamicWallpaperName = "DynamicWallpaper.xaml";
        private static readonly string TaskbarSettingName = "TaskbarSetting.xaml";

        private DynamicWallpaper dynamicWallpaper = new DynamicWallpaper();
        private StaticWallpaper staticWallpaper = new StaticWallpaper();
        private TaskbarSetting taskbarSetting = new TaskbarSetting();

        public DesktopBeautify()
        {
            InitializeComponent();
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            (System.Windows.Application.Current.MainWindow as MainWindow).EndShowMenuAnimation();
        }


        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (toggleButton != null)
                toggleButton.IsChecked = false;

            toggleButton = sender as ToggleButton;
        }

        private void btn_StaticWallpaperClick(object sender, RoutedEventArgs e)
        {
            //frame.Source = new Uri(StaticWallpaperName, UriKind.Relative);
            frame.Content = staticWallpaper;
            CloseAnonymousPipe();       
        }

        private void btn_DynamicWallpaperClick(object sender, RoutedEventArgs e)
        {
            //frame.Source = new Uri(DynamicWallpaperName, UriKind.Relative);
            frame.Content = dynamicWallpaper;
        }

        public void CloseAnonymousPipe()
        {
            dynamicWallpaper.StopDynamicWallpaperProcess();
        }

        private void btn_TaskbarSettingClick(object sender, RoutedEventArgs e)
        {
            frame.Content = taskbarSetting;
        }
    }

}

