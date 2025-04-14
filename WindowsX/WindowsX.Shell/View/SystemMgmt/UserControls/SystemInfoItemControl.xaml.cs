using System;
using System.Collections.Generic;
using System.Linq;
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

namespace WindowsX.Shell.View.SystemMgmt.UserControls
{
    /// <summary>
    /// SystemInfoItemControl.xaml 的交互逻辑
    /// </summary>
    public partial class SystemInfoItemControl : UserControl
    {
        public SystemInfoItemControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(SystemInfoItemControl),new PropertyMetadata());
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(string), typeof(SystemInfoItemControl), new PropertyMetadata());

        public string Title
        {
            get => GetValue(TitleProperty).ToString();
            set => SetValue(TitleProperty, value);
        }

        public string Icon
        {
            get => GetValue(IconProperty).ToString();
            set => SetValue(IconProperty, value);
        }

        private Dictionary<string, string> systemInfoCollection = new Dictionary<string, string>();

        public Dictionary<string, string> SystemInfoCollection
        {
            get => systemInfoCollection;
            set
            {
                systemInfoCollection = value;
                UpdateSystemInfo();
            }
        }

        public static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private void UpdateSystemInfo()
        {
            foreach (var item in systemInfoCollection)
            {
                Grid grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1,GridUnitType.Star)});
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                grid.Margin = new Thickness(5, 0, 0, 5);
                TextBlock textBlock = new TextBlock();
                textBlock.FontSize = 13;
                textBlock.Text = item.Key;
                Grid.SetColumn(textBlock, 0);
                TextBox textBox = new TextBox();
                textBox.Text = item.Value;
                textBox.IsReadOnly = true;
                textBox.Background = Brushes.Transparent;
                textBox.BorderThickness = new Thickness(0);
                Grid.SetColumn(textBox, 1);
                grid.Children.Add(textBlock);
                grid.Children.Add(textBox);
                stack.Children.Add(grid);
            }
        }
    }
}
