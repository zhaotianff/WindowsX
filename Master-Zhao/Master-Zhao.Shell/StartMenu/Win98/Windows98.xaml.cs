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
using System.Windows.Shapes;

namespace Master_Zhao.Shell.StartMenu.Win98
{
    /// <summary>
    /// Interaction logic for Win98.xaml
    /// </summary>
    public partial class Windows98 : StartMenuWindowBase
    {
        private const double StartWidthRatio = 0.136d;
        private const double TitleWidthRatio = 0.113d;
        private const double MenuItemHeightRatio = 0.0917d;

        public Windows98() : base()
        {
            InitializeComponent();
        }

        private void StartMenuWindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStartMenuItem();
        }

        private void LoadStartMenuItem()
        {
            for(int i = 0;i<10;i++)
            {
                MenuItem item = new MenuItem();
                item.Header = "Windows Update";
                item.Height = 37;
                item.Width = this.menu.Width;
                this.menu.Items.Add(item);
            }
        }

        public  override void SetStartMenuSize()
        {
            this.Width = SystemParameters.PrimaryScreenWidth * StartWidthRatio;
            this.grid_title.Width = this.Width * TitleWidthRatio;
            this.grid_menu.Width = this.Width - this.grid_title.Width;
            this.menu.Width = this.grid_menu.Width;
        }
    }
}
