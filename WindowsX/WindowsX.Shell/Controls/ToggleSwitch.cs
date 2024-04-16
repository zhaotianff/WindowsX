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

namespace WindowsX.Shell.Controls
{
    public class ToggleSwitch : ToggleButton
    {
        static ToggleSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
        }

        public static readonly DependencyProperty ToggleSwitchStateProperty =
            DependencyProperty.Register("ToggleSwitchState", typeof(bool?), typeof(ToggleSwitch), new PropertyMetadata(false,OnToggleSwitchStateChanged));
        public bool? ToggleSwitchState
        {
            get { return (bool?)GetValue(ToggleSwitchStateProperty); }
            set { SetValue(ToggleSwitchStateProperty, value); }
        }

        public static readonly RoutedEvent SwitchCheckedEvent = EventManager.RegisterRoutedEvent("SwitchChecked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToggleSwitch));

        public event RoutedEventHandler SwitchChecked
        {
            add
            {
                AddHandler(SwitchCheckedEvent, value);
            }
            remove
            {
                RemoveHandler(SwitchCheckedEvent, value);
            }
        }

        public static readonly RoutedEvent SwitchUnCheckedEvent = EventManager.RegisterRoutedEvent("SwitchUnChecked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ToggleSwitch));

        public event RoutedEventHandler SwitchUnChecked
        {
            add
            {
                AddHandler(SwitchUnCheckedEvent, value);
            }
            remove
            {
                RemoveHandler(SwitchUnCheckedEvent, value);
            }
        }

        private static void OnToggleSwitchStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var toggleSwitch = d as ToggleSwitch;

            if((bool?)e.NewValue == true)
            {
                toggleSwitch.RaiseEvent(new RoutedEventArgs(SwitchCheckedEvent));
            }
            else
            {
                toggleSwitch.RaiseEvent(new RoutedEventArgs(SwitchUnCheckedEvent));
            }
        }
    }
}
