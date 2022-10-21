﻿using Master_Zhao.Shell.Controls;
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

namespace Master_Zhao.Shell.StartMenu.WinFlat
{
    /// <summary>
    /// UcGroupedFlatStartMenuItem.xaml 的交互逻辑
    /// </summary>
    public partial class UcGroupedFlatStartMenuItem : UserControl
    {
        public static readonly DependencyProperty GroupedDataProperty = DependencyProperty.Register("GroupedData",typeof(GroupedFlatStartMenuItem),typeof(UserControl),new PropertyMetadata(OnGroupedDataChanged));

        public GroupedFlatStartMenuItem GroupedData
        {
            get => GetValue(GroupedDataProperty) as GroupedFlatStartMenuItem;
            set => SetValue(GroupedDataProperty, value);
        }

        public UcGroupedFlatStartMenuItem()
        {
            InitializeComponent();
        }

        private static void OnGroupedDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ucGroupedFlatStartMenuItem = d as UcGroupedFlatStartMenuItem;

            ucGroupedFlatStartMenuItem.wrap_List.Children.Clear();
            var groupedData = ucGroupedFlatStartMenuItem.GroupedData;
            ucGroupedFlatStartMenuItem.lbl_Title.Content = groupedData.GroupName;

            foreach (WinFlatStartMenuItem item in groupedData.MenuItems)
            {
                ImageButtonForStartMenu imageButtonForStartMenu = new ImageButtonForStartMenu();
                imageButtonForStartMenu.StartMenuItemData = item;
                imageButtonForStartMenu.Margin = new Thickness(20, 10, 10, 10);
                imageButtonForStartMenu.ExecName = item.Name;
                imageButtonForStartMenu.Width = 80;
                imageButtonForStartMenu.Height = 80;
                imageButtonForStartMenu.IconSource = item.ImageSourceIcon;
                ucGroupedFlatStartMenuItem.wrap_List.Children.Add(imageButtonForStartMenu);
            }
        }
    }
}
