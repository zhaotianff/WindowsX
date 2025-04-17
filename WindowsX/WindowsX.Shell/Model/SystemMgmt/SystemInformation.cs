using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Shell.Model.SystemMgmt
{
    public class SystemInformation
    {
        public List<string> SystemInformationTypeList { get; set; }

        public List<Dictionary<string,string>> SystemInformationKeyValueList { get; set; }
    }
}
