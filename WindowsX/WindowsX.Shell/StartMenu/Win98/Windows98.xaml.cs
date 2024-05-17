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
using WindowsX.Shell.Util;
using System.Linq;
using WindowsX.Shell.StartMenu.Data;

namespace WindowsX.Shell.StartMenu.Win98
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
                AppendMenuItem(menuList);          
            }, new System.Threading.CancellationToken(), TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
            return task;
        }

        private List<StartMenuItemBase> GetWin98StartMenuItem()
        {
            var list = new List<StartMenuItemBase>();

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
            programsItem.FilePathIcon = "./Icon/programs.png";
            programsItem.Child = GetPrograms(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms));
            list.Add(programsItem);

            //Favorites
            Win98StartMenuItem favoritesItem = new Win98StartMenuItem();
            favoritesItem.Name = "收藏";
            favoritesItem.Exec = Environment.GetFolderPath(Environment.SpecialFolder.Favorites);
            favoritesItem.FilePathIcon = "./Icon/favorites.png";
            list.Add(favoritesItem);

            //Documents
            Win98StartMenuItem documentsItem = new Win98StartMenuItem();
            documentsItem.Name = "文档";
            documentsItem.Exec = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            documentsItem.FilePathIcon = "./Icon/document.png";
            list.Add(documentsItem);

            //Settings
            Win98StartMenuItem settingsItem = new Win98StartMenuItem();
            settingsItem.Name = "设置";
            settingsItem.Exec = "ms-settings:main";
            settingsItem.FilePathIcon = "./Icon/settings.png";
            list.Add(settingsItem);

            //Find
            Win98StartMenuItem findItem = new Win98StartMenuItem();
            findItem.Name = "查找";
            findItem.Handler = Search;
            findItem.FilePathIcon = "./Icon/find.png";
            list.Add(findItem);

            //Help
            Win98StartMenuItem helpItem = new Win98StartMenuItem();
            helpItem.Name = "帮助";
            //TODO f1
            helpItem.Handler = ShowHelp;
            helpItem.FilePathIcon = "./Icon/help.png";
            list.Add(helpItem);

            //Run
            Win98StartMenuItem runItem = new Win98StartMenuItem();
            runItem.Name = "运行";
            runItem.Handler = Run;
            runItem.FilePathIcon = "./Icon/run.png";
            list.Add(runItem);

            list.Add(Win98StartMenuItem.Seperator);

            //Logoff
            Win98StartMenuItem logoffItem = new Win98StartMenuItem();
            logoffItem.Name = "注销";
            logoffItem.FilePathIcon = "./Icon/logoff.png";
            logoffItem.Handler = Logoff;
            list.Add(logoffItem);

            //Shutdown
            Win98StartMenuItem shutdownItem = new Win98StartMenuItem();
            shutdownItem.Name = "关机";
            shutdownItem.Handler = ShowShutDownDialog;
            shutdownItem.FilePathIcon = "./Icon/shutdown.png";
            list.Add(shutdownItem);

            return list;
        }

        private void AppendMenuItem(List<StartMenuItemBase> menuItemList,MenuItem subMenu = null)
        {
            foreach (var menuItem in menuItemList)
            {
                if(menuItem.Child.Count == 0)
                {
                    if (menuItem.IsSeperator)
                    {
                        Separator separator = new Separator();
                        separator.Height = 1;
                        separator.BorderBrush = FindResource("ActiveBorder") as SolidColorBrush;
                        separator.Background = FindResource("SeperatorBackground") as SolidColorBrush;
                        separator.Width = this.menu.Width;

                        if (subMenu == null)
                            this.menu.Items.Add(separator);
                        else
                            subMenu.Items.Add(separator);
                        continue;
                    }

                    var item = GetMenuItem(menuItem,subMenu);

                    if (subMenu == null)
                        this.menu.Items.Add(item);
                    else
                        subMenu.Items.Add(item);
                }
                else
                {
                    var item = GetMenuItem(menuItem,subMenu);

                    if (subMenu == null)
                        this.menu.Items.Add(item);
                    else
                        subMenu.Items.Add(item);

                    //TODO
                    AppendMenuItem(menuItem.Child, item);
                }
            }
        }

        private MenuItem GetMenuItem(StartMenuItemBase menuItem,MenuItem subMenu)
        {
            MenuItem item = new MenuItem();
            item.Header = menuItem;
            item.Height = 37;
            var iconImage = new Image();
            iconImage.Stretch = Stretch.UniformToFill;
            if (menuItem.ImageSourceIcon == null)
                iconImage.Source = ImageHelper.GetBitmapImageFromResource(menuItem.FilePathIcon);
            else
                iconImage.Source = menuItem.ImageSourceIcon;
            item.Icon = iconImage;

            if(subMenu == null)
            {
                item.Width = this.Width - this.Width * 0.19f;  //TODO
            }
            else
            {
                item.Width = 300; //TODO
            }
            
            item.HorizontalAlignment = HorizontalAlignment.Left;
            item.VerticalAlignment = VerticalAlignment.Center;
            item.Click += menuItem_Click;

            //TODO
            if(menuItem.Name == "程序")
            {
                item.MouseEnter += (a, b) => { item.IsSubmenuOpen = true; };

                item.PreviewMouseDown += async (sender, e) =>
                {
                    await Task.Delay(10);
                    item.IsSubmenuOpen = true;

                };
            }

            return item;
        }

        private void menuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if(item != null)
            {
                var startMenuItem = item.Header as StartMenuItemBase;

                if (startMenuItem.Child.Count == 0)
                {
                    StartMenuManager.HideStartMenu();

                    if (startMenuItem.Handler != null)
                    {
                        startMenuItem.Handler.Invoke(sender, e);
                    }
                    else
                    {
                        ProcessHelper.Execute(startMenuItem?.Exec);
                    }
                }
            }
        }

        public  override void SetStartMenuSize()
        {
            this.Width = SystemParameters.PrimaryScreenWidth * StartWidthRatio;
            this.grid_title.Width = this.Width * TitleWidthRatio;
            this.grid_menu.Width = this.Width - this.grid_title.Width;
            this.menu.Width = this.grid_menu.Width;


            //TODO hook start menu button or hook key
        }

        private void StartMenuWindowBase_LostFocus(object sender, RoutedEventArgs e)
        {
            //StartMenuManager.HideStartMenu();
        }
    }
}
