using System;
using System.Collections.Generic;
using System.Text;

namespace Master_Zhao.Shell.Model.SystemMgmt
{
    public class StartupItem
    {
        public IntPtr HKey { get; set; }
        public string RegPath { get; set; }
        public uint SamDesired { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public string Path { get; set; }
        public Version Version { get; set; }
        public StartupItemType StartupItemType { get; set; }
    }
}
