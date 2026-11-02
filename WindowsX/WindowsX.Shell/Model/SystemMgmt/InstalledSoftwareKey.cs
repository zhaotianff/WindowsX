using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Shell.Model.SystemMgmt
{
    public record struct InstalledSoftwareKey
    {
        public RegistryHive Hive { get; }
        public string Key { get; }
        public string Name { get; }
        public string Version { get; }
        public string Publisher { get; }
        public string Location { get; }

        public InstalledSoftwareKey(RegistryHive hive, string key, string name, string version, string publisher, string location)
        {
            Hive = hive;
            Key = key;
            Name = name;
            Version = version;
            Publisher = publisher;
            Location = location;
        }
    }
}
