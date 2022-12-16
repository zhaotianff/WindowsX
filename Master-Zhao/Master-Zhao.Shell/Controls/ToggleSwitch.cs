using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Master_Zhao.Shell.Controls
{
    public class ToggleSwitch : ToggleButton
    {
        static ToggleSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
        }

        public bool? ToggleSwitchState
        {
            get { return (bool?)GetValue(ToggleSwitchStateProperty); }
            set { SetValue(ToggleSwitchStateProperty, value); }
        }

        public static readonly DependencyProperty ToggleSwitchStateProperty =
            DependencyProperty.Register("ToggleSwitchState", typeof(bool?), typeof(ToggleSwitch), new PropertyMetadata(false));
    }
}
