using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Master_Zhao.Shell.Controls.UserControls
{
    /// <summary>
    /// ThemeControl.xaml 的交互逻辑
    /// </summary>
    public partial class ThemeControl : UserControl
    {
        public static readonly DependencyProperty ImagePathProperty = DependencyProperty.Register("ImagePath", typeof(string), typeof(ThemeControl));
        public static readonly DependencyProperty ColorBrushProperty = DependencyProperty.Register("ColorBrush", typeof(Brush), typeof(ThemeControl));
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(ThemeControl));
        public static readonly DependencyProperty ThemeFileProperty = DependencyProperty.Register("ThemeFile", typeof(string), typeof(ThemeControl));
        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register("Stretch", typeof(Stretch), typeof(ThemeControl));

        public string ImagePath
        {
            get => GetValue(ImagePathProperty).ToString();
            set
            {
                SetValue(ImagePathProperty, value);
                this.image.Source = new BitmapImage(new Uri(value, UriKind.Absolute));
                this.rect.Visibility = Visibility.Hidden;
            }
        }

        public Brush ColorBrush
        {
            get => (Brush)GetValue(ColorBrushProperty);
            set
            {
                SetValue(ColorBrushProperty, value);
                this.rect.Fill = value;
                this.image.Visibility = Visibility.Hidden;
            }
        }

        public string Title
        {
            get => GetValue(TitleProperty).ToString();
            set
            {
                SetValue(TitleProperty, value);
                this.label.Content = value;
            }
        }

        public string ThemeFile
        {
            get => GetValue(ThemeFileProperty).ToString();
            set => SetValue(ThemeFileProperty, value);
        }

        public Stretch Stretch
        {
            get => (Stretch)GetValue(StretchProperty);
            set
            {
                SetValue(StretchProperty, value);
                this.image.Stretch = value;
            }
        }

        public ThemeControl()
        {
            InitializeComponent();
            this.Cursor = Cursors.Hand;
        }
    }
}
