using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Master_Zhao.Shell.Model.SystemMgmt
{
    public class FileAssocItem
    {
        public string Extension { get; set; }

        public string FriendlyName { get; set; }

        public string Executable { get; set; }

        public ImageSource Icon { get; set; }

        public float Percentage { get; set; }

        public string PercentageText { get; set; }

        public int Count { get; set; }

        public List<string> AllFiles { get; set; } = new List<string>();
    }
}
