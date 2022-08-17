using Master_Zhao.Shell.PInvoke;
using Master_Zhao.Shell.StartMenu.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Master_Zhao.Shell.StartMenu
{
    public class StartMenuWindowBase : System.Windows.Window
    {
        public StartMenuWindowBase()
        {
            InitAllEvent();
            InitWindows();
        }

        public virtual void SetStartMenuSize() 
        { 
        }

        public virtual void SetStartMenuPos()
        {
            this.Left = 0;
            this.Top = SystemParameters.WorkArea.Height - this.Height;
        }

        public virtual List<StartMenuItemBase> GetPrograms(string programPath)
        {
            var list = new List<StartMenuItemBase>();
            RecursiveGetPrograms(programPath, ref list);
            return list;
        }

        private void RecursiveGetPrograms(string path,ref List<StartMenuItemBase> menuList)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            var fileEntry = dir.GetFileSystemInfos();
            foreach (var file in fileEntry)
            {
                var fileinfo = file as FileInfo;

                if (fileinfo != null)
                {
                    StartMenuItemBase startMenuItem = new StartMenuItemBase();
                    startMenuItem.Name = fileinfo.Name;
                    startMenuItem.Exec = fileinfo.FullName;
                    //TODO get icon
                    startMenuItem.FilePathIcon = "./Icon/programs.png";
                    menuList.Add(startMenuItem);
                }
                else
                {
                    var dirInfo = file as DirectoryInfo;
                    StartMenuItemBase startMenuItem = new StartMenuItemBase();
                    startMenuItem.Name = dirInfo.Name;
                    startMenuItem.Exec = dirInfo.FullName;
                    startMenuItem.FilePathIcon = "./Icon/programs.png";
                    var list = new List<StartMenuItemBase>();
                    RecursiveGetPrograms(dirInfo.FullName, ref list);
                    startMenuItem.Child = list;
                    menuList.Add(startMenuItem);
                }
            }
        }

        public virtual Task LoadStartMenuItemAsync()
        {
            return Task.Delay(0);
        }

        private void InitAllEvent()
        {
            this.Loaded += StartMenuWindowBase_Loaded;
        }

        private async void StartMenuWindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            SetStartMenuSize();
            await LoadStartMenuItemAsync();
            //TODO wait ui refresh
            await Task.Delay(100);
            SetStartMenuPos();
        }

        private void InitWindows()
        {
            this.WindowStyle = WindowStyle.None;
            this.ShowInTaskbar = false;
            this.ResizeMode = ResizeMode.NoResize;
            this.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        public void Shutdown(object sender,RoutedEventArgs e)
        {
            PowerTool.Shutdown();
        }

        public void SwitchUser(object sender,RoutedEventArgs e)
        {
            PowerTool.SwitchUser();
        }

        public void Logoff(object sender, RoutedEventArgs e)
        {
            PowerTool.Logoff();
        }

        public void Lock(object sender, RoutedEventArgs e)
        {
            PowerTool.Lock();
        }

        public void Restart(object sender, RoutedEventArgs e)
        {
            PowerTool.Restart();
        }

        public void Sleep(object sender, RoutedEventArgs e)
        {
            PowerTool.Sleep();
        }
    }
}
