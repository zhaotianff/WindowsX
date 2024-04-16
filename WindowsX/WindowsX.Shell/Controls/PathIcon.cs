using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace WindowsX.Shell.Controls
{
    public class PathIcon : Control
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(Geometry), typeof(PathIcon));

        public Geometry Data
        {
            get { return (Geometry)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        static PathIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PathIcon), new FrameworkPropertyMetadata(typeof(PathIcon)));
        }
    }
}
