using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace Master_Zhao.Shell.Model.Utility
{

    public class FastRunItem
    {
        public double Angle { get; set; }
        public string DisplayName { get; set; }
        public string Path { get; set; }
        public ImageSource Icon { get; set; }
        public string[] Args { get; set; }
        public int[] HotKey { get; set; }
    }
}
