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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading.Tasks;
using Master_Zhao.Shell.Util;

namespace Master_Zhao.Shell.StartMenu.Win98
{
    /// <summary>
    /// Interaction logic for Win98.xaml
    /// </summary>
    public partial class Windows98 : StartMenuWindowBase
    {
        private const double StartWidthRatio = 0.136d;
        private const double TitleWidthRatio = 0.113d;
        private const double MenuItemHeightRatio = 0.0917d;

        public Windows98() : base()
        {
            InitializeComponent();
        }

        public override Task LoadStartMenuItemAsync()
        {
            var task = Task.Factory.StartNew(() =>
            {
                var menuList = GetWin98StartMenuItem();
                foreach(var menuItem in menuList)
                {
                    //TODO
                    AppendMenuItem(menuItem);
                }
            }, new System.Threading.CancellationToken(), TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
            return task;
        }

        private List<Win98StartMenuItem> GetWin98StartMenuItem()
        {
            var list = new List<Win98StartMenuItem>();

            //TODO add user config

            //Windows Update
            Win98StartMenuItem windowsUpdateItem = new Win98StartMenuItem();
            windowsUpdateItem.Name = "Windows更新";
            windowsUpdateItem.Exec = "ms-settings:windowsupdate";
            windowsUpdateItem.FilePathIcon = "./Icon/windows_update.png";
            list.Add(windowsUpdateItem);

            list.Add(Win98StartMenuItem.Seperator);

            //Programs
            Win98StartMenuItem programsItem = new Win98StartMenuItem();
            programsItem.Name = "程序";
            programsItem.FilePathIcon = "./Icon/windows_update.png";
            list.Add(programsItem);

            //Favorites
            Win98StartMenuItem favoritesItem = new Win98StartMenuItem();
            favoritesItem.Name = "收藏";
            favoritesItem.FilePathIcon = "./Icon/windows_update.png";
            list.Add(favoritesItem);

            //Documents
            Win98StartMenuItem documentsItem = new Win98StartMenuItem();
            documentsItem.Name = "文档";
            documentsItem.FilePathIcon = "./Icon/windows_update.png";
            list.Add(documentsItem);

            //Settings
            Win98StartMenuItem settingsItem = new Win98StartMenuItem();
            settingsItem.Name = "设置";
            settingsItem.FilePathIcon = "./Icon/windows_update.png";
            list.Add(settingsItem);

            //Find
            Win98StartMenuItem findItem = new Win98StartMenuItem();
            findItem.Name = "查找";
            findItem.FilePathIcon = "./Icon/windows_update.png";
            list.Add(findItem);

            //Help
            Win98StartMenuItem helpItem = new Win98StartMenuItem();
            helpItem.Name = "帮助";
            helpItem.FilePathIcon = "./Icon/windows_update.png";
            list.Add(helpItem);

            //Run
            Win98StartMenuItem runItem = new Win98StartMenuItem();
            runItem.Name = "运行";
            runItem.FilePathIcon = "./Icon/windows_update.png";
            list.Add(runItem);

            list.Add(Win98StartMenuItem.Seperator);

            //Logoff
            Win98StartMenuItem logoffItem = new Win98StartMenuItem();
            logoffItem.Name = "注销";
            logoffItem.FilePathIcon = "./Icon/windows_update.png";
            list.Add(logoffItem);

            //Shutdown
            Win98StartMenuItem shutdownItem = new Win98StartMenuItem();
            shutdownItem.Name = "关机";
            shutdownItem.FilePathIcon = "./Icon/windows_update.png";
            list.Add(shutdownItem);

            return list;
        }

        private void AppendMenuItem(Win98StartMenuItem data, MenuItem parentItem = null)
        {
            if(data.IsSeperator)
            {
                Separator separator = new Separator();
                separator.Height = 1;
                separator.BorderBrush = FindResource("ActiveBorder") as SolidColorBrush;
                separator.Background = FindResource("SeperatorBackground") as SolidColorBrush;
                separator.Width = this.menu.Width;
                this.menu.Items.Add(separator);
                return;
            }

            MenuItem item = new MenuItem();
            item.Header = data;
            item.Height = 37;
            var iconImage = new Image();
            iconImage.Stretch = Stretch.UniformToFill;
            iconImage.Source = ImageHelper.GetBitmapImageFromResource(data.FilePathIcon);
            item.Icon = iconImage;
            item.Width = this.menu.Width;
            item.HorizontalAlignment = HorizontalAlignment.Center;
            item.VerticalAlignment = VerticalAlignment.Center;

            if (parentItem == null)
                this.menu.Items.Add(item);
            else
                parentItem.Items.Add(item);
        }

        public  override void SetStartMenuSize()
        {
            this.Width = SystemParameters.PrimaryScreenWidth * StartWidthRatio;
            this.grid_title.Width = this.Width * TitleWidthRatio;
            this.grid_menu.Width = this.Width - this.grid_title.Width;
            this.menu.Width = this.grid_menu.Width;
        }
    }
}
