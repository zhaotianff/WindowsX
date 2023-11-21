using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Master_Zhao.Shell.Themes
{
    public class ThemeManager
    {
        public static void SwitchTheme(LocalTheme localTheme)
        {
            var rd = Application.Current.Resources.MergedDictionaries[1];
            rd.Source = new Uri($"pack://application:,,,/Themes/{localTheme.ToString()}.xaml", UriKind.Absolute);
        }
    }

    public enum LocalTheme
    {
        Default,
        Night1
    }
}
