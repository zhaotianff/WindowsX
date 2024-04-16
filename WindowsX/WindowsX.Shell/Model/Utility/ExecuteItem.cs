using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace WindowsX.Shell.Model.Utility
{
    public class ExecuteItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public ImageSource Icon { get; set; }
        public ExecuteItemType ItemType { get; set; }
    }

    public enum ExecuteItemType
    {
        EXE,
        MMC, 
        CPL, 
        SCRIPT, 
        DLL, 
        MSSETTING,
        SHELL
    }
}
