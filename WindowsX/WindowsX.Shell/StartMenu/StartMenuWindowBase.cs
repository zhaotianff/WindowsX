using WindowsX.Shell.PInvoke;
using WindowsX.Shell.StartMenu.Data;
using WindowsX.Shell.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WindowsX.Shell.StartMenu
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
                    if((fileinfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    {
                        continue;
                    }
                    StartMenuItemBase startMenuItem = new StartMenuItemBase();
                    startMenuItem.Name = Path.GetFileNameWithoutExtension(fileinfo.Name);
                    startMenuItem.Exec = fileinfo.FullName;
                    IntPtr hIcon = IntPtr.Zero;
                    if (IconTool.ExtractFirstIconFromFile(fileinfo.FullName, true, ref hIcon))
                    {
                        startMenuItem.ImageSourceIcon = ImageHelper.GetBitmapImageFromHIcon(hIcon);
                    }
                   
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
            PowerTool.ShutdownComputer();
        }

        public virtual void ShowShutDownDialog(object sender,RoutedEventArgs e)
        {
            PowerTool.ShowShutDownDialog();
        }

        public void SwitchUser(object sender,RoutedEventArgs e)
        {
            PowerTool.SwitchUser();
        }

        public void Logoff(object sender, RoutedEventArgs e)
        {
            PowerTool.Logoff();
        }

        public void Run(object sender,RoutedEventArgs e)
        {
            PowerTool.ShowRunDialog();
        }

        public void ShowHelp(object sender,RoutedEventArgs e)
        {
            DesktopTool.OpenWindowsHelp();
        }

        public void Lock(object sender, RoutedEventArgs e)
        {
            PowerTool.LockComputer();
        }

        public void Restart(object sender, RoutedEventArgs e)
        {
            PowerTool.RestartComputer();
        }

        public void Sleep(object sender, RoutedEventArgs e)
        {
            PowerTool.SleepComputer();
        }

        public void Search(object sender,RoutedEventArgs e)
        {
            DesktopTool.OpenSearchWindow("");
        }
    }
}
