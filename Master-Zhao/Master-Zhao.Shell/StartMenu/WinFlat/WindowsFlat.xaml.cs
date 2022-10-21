using Master_Zhao.Shell.PInvoke;
using Master_Zhao.Shell.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Master_Zhao.Shell.StartMenu.WinFlat
{
    /// <summary>
    /// WindowsFlat.xaml 的交互逻辑
    /// </summary>
    public partial class WindowsFlat : TianXiaTech.BlurWindow
    {
        public WindowsFlat()
        {
            InitializeComponent();
            this.Left = 10;
            this.Top = SystemParameters.WorkArea.Height - this.Height - 10;
            LoadGroupedMenu();
        }

        private void ShowMenu()
        {
            this.Visibility = Visibility.Visible;
            DoubleAnimation showAnimation = new DoubleAnimation();
            showAnimation.From = 0;
            showAnimation.To = 1;
            showAnimation.Duration = TimeSpan.FromMilliseconds(300);
            this.BeginAnimation(OpacityProperty, showAnimation);
        }

        private void HideMenu()
        {
            
        }

        private void img_poweroff_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.popup_poweroff.IsOpen = !this.popup_poweroff.IsOpen;
            popup_poweroff.Focus();
        }

        private void img_setting_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void popup_poweroff_LostFocus(object sender, RoutedEventArgs e)
        {
            popup_poweroff.IsOpen = false;
        }

        private void img_search_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PInvoke.InputTool.SendSearch();
        }

        private void LoadGroupedMenu()
        {
            this.stack.Children.Clear();

            var groupedItems = Config.GlobalConfig.Instance.MainConfig.FlatStartMenuGroupedItems;
            foreach (var item in groupedItems)
            {
                GroupedFlatStartMenuItem groupedFlatStartMenuItem = new GroupedFlatStartMenuItem();
                groupedFlatStartMenuItem.GroupName = item.GroupName;
                groupedFlatStartMenuItem.MenuItems = new List<WinFlatStartMenuItem>();

                foreach (var menuitem in item.MenuItems)
                {
                    WinFlatStartMenuItem winFlatStartMenuItem = new WinFlatStartMenuItem();
                    winFlatStartMenuItem.Name = menuitem.Name;
                    winFlatStartMenuItem.Path = menuitem.Path;
                    winFlatStartMenuItem.Exec = menuitem.Path;
                    IntPtr hIcon = IntPtr.Zero;
                    if (IconTool.ExtractFirstIconFromFile(menuitem.Path, true, ref hIcon))
                    {
                        winFlatStartMenuItem.ImageSourceIcon = ImageHelper.GetBitmapImageFromHIcon(hIcon);
                    }

                    groupedFlatStartMenuItem.MenuItems.Add(winFlatStartMenuItem);
                }

                UcGroupedFlatStartMenuItem ucGroupedFlatStartMenuItem = new UcGroupedFlatStartMenuItem();
                ucGroupedFlatStartMenuItem.GroupedData = groupedFlatStartMenuItem;
                this.stack.Children.Add(ucGroupedFlatStartMenuItem);
            }
        }
    }
}
