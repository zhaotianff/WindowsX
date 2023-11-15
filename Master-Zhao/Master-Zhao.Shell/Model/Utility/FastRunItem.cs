using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Media;

namespace Master_Zhao.Shell.Model.Utility
{

    public class FastRunItem : INotifyPropertyChanged
    {
        public double Angle { get; set; }

        private string displayName;
        public string DisplayName
        {
            get => displayName;
            set
            {
                displayName = value;
                RaiseChanged("DisplayName");
            }
        }

        private string path;

        public string Path
        {
            get => path;
            set
            {
                path = value;
                RaiseChanged("Path");
            }
        }

        private ImageSource icon;

        public ImageSource Icon
        {
            get => icon;
            set
            {
                icon = value;
                RaiseChanged("Icon");
            }
        }

        public string[] Args { get; set; }
        public int[] HotKey { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaiseChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
