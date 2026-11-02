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
            get => GetValue(TitleProperty) as string ?? string.Empty;
            set => SetValue(TitleProperty, value);
        }

        public string Icon
        {
            get => GetValue(IconProperty) as string ?? string.Empty;
            set => SetValue(IconProperty, value);
        }

        private Model.SystemMgmt.SystemInformation systemInformation = new Model.SystemMgmt.SystemInformation();
        private bool isUpdating;

        public Model.SystemMgmt.SystemInformation SystemInformation
        {
            get => systemInformation;
            set
            {
                systemInformation = value ?? new Model.SystemMgmt.SystemInformation();
                UpdateSystemInfoToUi();
            }
        }

        public static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private void UpdateSystemInfoToUi()
        {
            LoadSystemInformationTypeList();
            RefreshSystemInformation();
        }

        private void LoadSystemInformationTypeList()
        {
            isUpdating = true;
            try
            {
                var count = SystemInformation.SystemInformationKeyValueList?.Count ?? 0;
                var names = SystemInformation.SystemInformationTypeList;
                cbx_TypeList.ItemsSource = Enumerable.Range(0, count)
                    .Select(index => names != null && index < names.Count && !string.IsNullOrWhiteSpace(names[index]) ? names[index] : $"{Title}{index + 1}").ToList();
                cbx_TypeList.Visibility = count > 1 ? Visibility.Visible : Visibility.Collapsed;
                cbx_TypeList.SelectedIndex = count > 0 ? 0 : -1;
            }
            finally
            {
                isUpdating = false;
            }
        }

        private void RefreshSystemInformation(int index = 0)
        {
            for (int i = this.stack.Children.Count - 1; i >= 1; i--)
            {
                this.stack.Children.RemoveAt(i);
            }

            var rows = SystemInformation.SystemInformationKeyValueList;
            if (rows == null || index < 0 || index >= rows.Count || rows[index] == null || rows[index].Count == 0)
            {
                stack.Children.Add(new TextBlock { Text = "暂无信息", Margin = new Thickness(10) });
                return;
            }

            foreach (var item in rows[index])
            {
                Grid grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                grid.Margin = new Thickness(5, 0, 0, 5);
                TextBlock textBlock = new TextBlock();
                textBlock.FontSize = 13;
                textBlock.Text = item.Key;
                textBlock.Foreground = Brushes.Black;
                Grid.SetColumn(textBlock, 0);
                TextBox textBox = new TextBox();
                textBox.Text = string.IsNullOrWhiteSpace(item.Value) ? "未提供" : item.Value;
                textBox.IsReadOnly = true;
                textBox.SetBinding(TextBox.ForegroundProperty, new Binding("Foreground") { Source = this });
                textBox.TextWrapping = TextWrapping.WrapWithOverflow;
                textBox.Background = Brushes.Transparent;
                textBox.Foreground = Brushes.Black;
                textBox.BorderThickness = new Thickness(0);
                Grid.SetColumn(textBox, 1);
                grid.Children.Add(textBlock);
                grid.Children.Add(textBox);
                stack.Children.Add(grid);
            }
        }

        private void cbx_TypeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isUpdating && sender is ComboBox comboBox && comboBox.SelectedIndex >= 0)
                RefreshSystemInformation(comboBox.SelectedIndex);
        }
    }
}
