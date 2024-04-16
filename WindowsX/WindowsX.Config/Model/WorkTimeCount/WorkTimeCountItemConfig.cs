using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsX.Config.Model.WorkTimeCount
{
    public class WorkTimeCountItemConfig
    {
        public DateTime Date { get; set; }

        public long EllapsedTime { get; set; }

        public string EllapsedTimeString { get; set; }

        public string Title { get; set; }
    }
}
