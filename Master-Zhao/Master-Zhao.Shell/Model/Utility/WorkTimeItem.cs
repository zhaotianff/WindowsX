using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Master_Zhao.Shell.Model.Utility
{
    public class WorkTimeItem : INotifyPropertyChanged
    {
        public string Title { get; set; }

        private long ellapsedTime;
        public long EllapsedTime 
        {
            get => ellapsedTime;
            set
            {
                ellapsedTime = value;
                RaisedChange("EllapsedTime");
                EllapsedTimeString = GetDisplayTimeString(ellapsedTime);
            }
        }

        private string ellapsedTimeString;
        public string EllapsedTimeString
        {
            get => ellapsedTimeString;
            set
            {
                ellapsedTimeString = value;
                RaisedChange("EllapsedTimeString");
            }
        }

        public bool IsWorking { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisedChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string GetDisplayTimeString(long ellapsedTime)
        {
            if (ellapsedTime == 0)
                return "";

            TimeSpan ts = TimeSpan.FromSeconds(ellapsedTime);
            if (ts.Days > 0)
                return $"{ts.Days}天 {ts.Hours}时 {ts.Minutes}分";

            return $"{ts.Hours}时 {ts.Minutes}分";
        }
    }
}
