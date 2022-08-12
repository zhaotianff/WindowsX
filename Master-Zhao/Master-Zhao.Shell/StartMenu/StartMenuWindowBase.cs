using System;
using System.Collections.Generic;
using System.Text;
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

        private void InitAllEvent()
        {
            this.Loaded += StartMenuWindowBase_Loaded;
        }

        private void StartMenuWindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            SetStartMenuPos();
            SetStartMenuSize();
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

        }

        public void SwitchUser(object sender,RoutedEventArgs e)
        {

        }

        public void Logoff(object sender, RoutedEventArgs e)
        {

        }

        public void Lock(object sender, RoutedEventArgs e)
        {

        }

        public void Restart(object sender, RoutedEventArgs e)
        {

        }

        public void Sleep(object sender, RoutedEventArgs e)
        {

        }
    }
}
