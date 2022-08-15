using Master_Zhao.Shell.StartMenu.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master_Zhao.Shell.StartMenu.Win98
{
    public class Win98StartMenuItem : StartMenuItemBase
    {
        public static Win98StartMenuItem Seperator => new Win98StartMenuItem() { Name = "/" };

        public override string ToString()
        {
            return Name;
        }
    }
}
