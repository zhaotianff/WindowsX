using System;
using System.Collections.Generic;
using System.Text;

namespace Master_Zhao.Shell.Model.Process
{
    public class ProcessInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string MainWindowText { get; set; }
        public IntPtr MainWindowHwnd { get; set; }
        public int Pid { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
