using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace WindowsX.Shell.Util
{
    public class GlobalData
    {
        private static volatile GlobalData instance;
        private static object obj = new object();

        public static GlobalData Instance
        {
            get
            {
                lock(obj)
                {
                    if(instance == null)
                    {
                        lock(obj)
                        {
                            instance = new GlobalData();
                        }
                    }
                }
                return instance;
            }
        }


       public SolidColorBrush AccentColorBrush { get; private set; }
        public SolidColorBrush WhiteColorBrush { get; private set; }
        public SolidColorBrush AccentColorBrushTran { get; private set; }
        public SolidColorBrush AccentColorBrushShadow { get; private set; }
        public Color BorderColor { get; private set; }
        public Color BorderColorTran { get; private set; }

        public GlobalData()
        {
            RefreshGlobalData();
        }

        public void RefreshGlobalData()
        {
            RefreshBrushData();
        }

        public void RefreshBrushData()
        {
            AccentColorBrush = Application.Current.MainWindow.FindResource("AccentBaseColor") as SolidColorBrush;
            WhiteColorBrush = Application.Current.MainWindow.FindResource("WhiteColor") as SolidColorBrush;
            AccentColorBrushTran = Application.Current.MainWindow.FindResource("AccentBaseColorTran") as SolidColorBrush;
            AccentColorBrushShadow = Application.Current.MainWindow.FindResource("AccentBaseColorShadow") as SolidColorBrush;
            BorderColor = (Color)Application.Current.MainWindow.FindResource("BorderColor");
            BorderColorTran = (Color)Application.Current.MainWindow.FindResource("BorderColorTran");
        }
    }
}
