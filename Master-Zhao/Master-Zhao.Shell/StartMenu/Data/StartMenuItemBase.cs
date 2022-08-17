using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;

namespace Master_Zhao.Shell.StartMenu.Data
{
    public class StartMenuItemBase
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string[] Args { get; set; }
        public BitmapSource ImageSourceIcon { get; set; }
        public string FilePathIcon { get; set; }
        public string Exec { get; set; }
        public List<StartMenuItemBase> Child { get; set; } = new List<StartMenuItemBase>();
        public bool IsSeperator => Name == "/";
    }
}
