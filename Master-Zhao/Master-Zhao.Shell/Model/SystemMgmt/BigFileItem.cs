using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Master_Zhao.Shell.Model.SystemMgmt
{
    public class BigFileItem
    {
        public string Extension { get; set; }

        public string FriendlyName { get; set; }

        public string Path { get; set; }

        public ImageSource Icon { get; set; }

        public long Size { get; set; }
    }
}
