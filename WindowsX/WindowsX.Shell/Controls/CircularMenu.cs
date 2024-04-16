using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;
using System.Xml.Linq;

namespace WindowsX.Shell.Controls
{
    /// <summary>
    /// CircularMenu(Refrerence from https://github.com/WPFDevelopersOrg/WPFDevelopers,thanks @yanjinhuagood)
    /// </summary>
    [TemplatePart(Name = ItemsControlTemplateName, Type = typeof(ItemsControl))]
    [TemplatePart(Name = EllipseGeometryTemplateName, Type = typeof(EllipseGeometry))]
    public class CircularMenu : ListBox
    {
        public static readonly DependencyProperty StatusTextProperty = DependencyProperty.Register("StatusText", typeof(string), typeof(CircularMenu), new PropertyMetadata(string.Empty));

        private const string ItemsControlTemplateName = "PART_ItemsControl";
        private const string EllipseGeometryTemplateName = "PART_EllipseGeometry";
        private EllipseGeometry _ellipseGeometry;

        private ItemsControl _itemsControl;

        public string StatusText
        {
            get => GetValue(StatusTextProperty).ToString();
            set => SetValue(StatusTextProperty, value);
        }

        static CircularMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CircularMenu),
                new FrameworkPropertyMetadata(typeof(CircularMenu)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            AlternationCount = 8;
            _itemsControl = GetTemplateChild(ItemsControlTemplateName) as ItemsControl;
            if (_itemsControl != null) _itemsControl.MouseLeftButtonDown += _itemsControl_MouseLeftButtonDown;
            _ellipseGeometry = GetTemplateChild(EllipseGeometryTemplateName) as EllipseGeometry;
            if (_ellipseGeometry != null)
            {
                _ellipseGeometry.Center = new Point(Width / 2, Height / 2);
                _ellipseGeometry.RadiusX = Width;
                _ellipseGeometry.RadiusY = Height;
            }
        }

        private void _itemsControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = (e.OriginalSource as FrameworkElement).DataContext;
            SelectedItem = item;
        }
    }
}
