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
        public static readonly DependencyProperty ExecNameProperty = DependencyProperty.Register("ExecName", typeof(string), typeof(ImageButtonForStartMenu));

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

        public string ExecName
        {
            get => GetValue(ExecNameProperty).ToString();
            set => SetValue(ExecNameProperty, value);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Util.ProcessHelper.Execute(ExecName);
        }
    }
}
