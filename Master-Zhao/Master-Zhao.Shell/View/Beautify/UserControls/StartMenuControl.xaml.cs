using Master_Zhao.Shell.Util;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Master_Zhao.Shell.View.UserControls
{
    /// <summary>
    /// StartMenuControl.xaml 的交互逻辑
    /// </summary>
    public partial class StartMenuControl : UserControl
    {
        private SolidColorBrush AccentBaseColor { get; set; }
        public bool IsChecked { get; set; } = false;
        public delegate void StartMenuChangeEventHandler(object sender, string menuName);
        public event StartMenuChangeEventHandler OnSet;
        public event EventHandler OnSelect;

        public static readonly DependencyProperty MenuThumbnailProperty = 
            DependencyProperty.Register("MenuThumbnail", 
                typeof(string), 
                typeof(StartMenuControl), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnMenuThumbnailChanged, null), null);

        public string MenuThumbnail
        {
            get => GetValue(MenuThumbnailProperty).ToString();
            set
            {
                SetValue(MenuThumbnailProperty, value);
                this.image.Source = ImageHelper.GetBitmapImageFromResource(value);
            }
        }

        private static void OnMenuThumbnailChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StartMenuControl startMenuControl = (StartMenuControl)d;
            startMenuControl.MenuThumbnail = e.NewValue.ToString();
        }

        public static readonly DependencyProperty MenuNameProperty =
           DependencyProperty.Register("MenuName",
               typeof(string),
               typeof(StartMenuControl));

        public static readonly DependencyProperty DisplayNameProperty =
         DependencyProperty.Register("DisplayName",
             typeof(string),
             typeof(StartMenuControl),new PropertyMetadata(OnDisplayNameChanged));

        public string MenuName
        {
            get => GetValue(MenuNameProperty).ToString();
            set
            {
                SetValue(MenuNameProperty, value);
            }
        }

        public string DisplayName
        {
            get => GetValue(DisplayNameProperty).ToString();
            set
            {
                SetValue(DisplayNameProperty, value);
                this.title.Content = value;
            }
        }

        private static void OnDisplayNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StartMenuControl startMenuControl = (StartMenuControl)d;
            startMenuControl.DisplayName = e.NewValue.ToString();
        }

        public StartMenuControl()
        {
            InitializeComponent();
            AccentBaseColor = this.TryFindResource("AccentBaseColor") as SolidColorBrush;
        }


        private void set_Click(object sender, RoutedEventArgs e)
        {
            OnSet?.Invoke(sender, MenuName);
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            SetBorderBrush();
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsChecked == false)
            {
                ResetBorderBrush();
            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
            OnSelect?.Invoke(sender, e);
            SetBorderBrush();
            IsChecked = true;
        }

        private void SetBorderBrush()
        {
            BorderBrush = AccentBaseColor;
        }

        public void ResetBorderBrush()
        {
            BorderBrush = Brushes.Transparent;
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            ResetBorderBrush();
        }
    }
}
