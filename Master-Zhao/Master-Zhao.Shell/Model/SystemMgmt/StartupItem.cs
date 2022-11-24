using System;
using System.Collections.Generic;
using System.Text;

namespace Master_Zhao.Shell.Model.SystemMgmt
{
    public class StartupItem
    {
        public string Name { get; set; }

        public string Author { get; set; }

        public bool IsEnabled { get; set; }

        public string Path { get; set; }

        public Version Version { get; set; }
    }
}
