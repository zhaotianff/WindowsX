using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsX.Config.Model.WorkTimeCount
{
    public class WorkTimeCountConfig
    {
        public bool IsDocking { get; set; }

        public bool IsAdsorption { get; set; }

        public List<WorkTimeCountItemConfig> WorkTimeCountItemList{ get; set; }
    }
}
