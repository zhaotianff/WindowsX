using Master_Zhao.Shell.StartMenu.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Master_Zhao.Shell.Controls
{
    public class ImageButtonForStartMenu : Control
    {
        public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register("IconSource", typeof(ImageSource), typeof(ImageButtonForStartMenu));
        public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register("DisplayName", typeof(string), typeof(ImageButtonForStartMenu));
        public static readonly DependencyProperty ExecPathProperty = DependencyProperty.Register("ExecPath", typeof(string), typeof(ImageButtonForStartMenu),new PropertyMetadata(string.Empty));

        public StartMenuItemBase StartMenuItemData { get; set; }

        static ImageButtonForStartMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageButtonForStartMenu), new FrameworkPropertyMetadata(typeof(ImageButtonForStartMenu)));
        }

        public ImageSource IconSource
        {
            get => GetValue(IconSourceProperty) as ImageSource;
            set => SetValue(IconSourceProperty, value);
        }

        public string DisplayName
        {
            get => GetValue(DisplayNameProperty).ToString();
            set => SetValue(DisplayNameProperty, value);
        }

        public string ExecPath
        {
            get => GetValue(ExecPathProperty).ToString();
            set => SetValue(ExecPathProperty, value);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Util.ProcessHelper.Execute(ExecPath);
        }
    }
}
