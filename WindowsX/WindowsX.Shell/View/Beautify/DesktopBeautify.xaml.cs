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
using WindowsX.Shell.Infrastructure.Navigation;
using WindowsX.Shell.PInvoke;
using WindowsX.Shell.View.Beautify.Pages;
using WindowsX.Shell.View.Pages;

namespace WindowsX.Shell.Pages
{
    /// <summary>
    /// DesktopSetting.xaml 的交互逻辑
    /// </summary>
    public partial class DesktopBeautify : Page, IPageAction
    {
        private ToggleButton toggleButton = null;
     
        public DynamicWallpaper dynamicWallpaper = new DynamicWallpaper();
        private StaticWallpaper staticWallpaper = new StaticWallpaper();
        private MouseEffect mouseEffect = new MouseEffect();
        private StartMenuSetting startMenuSetting = new StartMenuSetting();
        private TaskbarSetting taskbarSetting = new TaskbarSetting();
        private ExplorerSetting explorerSetting = new ExplorerSetting();
        private ContextMenuManagement menuManagement = new ContextMenuManagement();
        private BootImageManagement bootImageManagement = new BootImageManagement();
        private OtherSetting otherSetting = new OtherSetting();

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

        private void btn_MouseEffectClick(object sender, RoutedEventArgs e)
        {
            frame.Content = mouseEffect;
        }

        public void CloseAnonymousPipe()
        {
            dynamicWallpaper.StopDynamicWallpaperProcess();
        }

        private void btn_TaskbarSettingClick(object sender, RoutedEventArgs e)
        {
            taskbarSetting.CheckCurrentSystem();
            frame.Content = taskbarSetting;
        }

        private void btn_ExplorerSettingClick(object sender, RoutedEventArgs e)
        {
            frame.Content = explorerSetting;
        }

        private void btn_OtherSettingClick(object sender, RoutedEventArgs e)
        {
            frame.Content = otherSetting;
        }

        private void btn_ContextMenuSettingClick(object sender, RoutedEventArgs e)
        {
            frame.Content = menuManagement;
        }

        private void btn_BootImageSettingClick(object sender, RoutedEventArgs e)
        {
            frame.Content = bootImageManagement;
        }

        private void btn_StartMenuSettingClick(object sender, RoutedEventArgs e)
        {
            frame.Content = startMenuSetting;
        }

        public void Terminate()
        {
            mouseEffect.CloseMouseEffectWindow();
        }

        public void ShowDefaultPage()
        {
            
        }
    }

}

