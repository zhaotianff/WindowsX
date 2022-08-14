using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading.Tasks;
using Master_Zhao.Shell.Util;

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

        public override Task LoadStartMenuItemAsync()
        {
            var task = Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    MenuItem item = new MenuItem();
                    item.Header = "Windows Update";
                    item.Height = 37;
                    //TODO
                    item.Icon = ImageHelper.GetResourceBitmapImage("/Icon/windows_update.png");
                    item.Width = this.menu.Width;
                    item.HorizontalAlignment = HorizontalAlignment.Center;
                    this.menu.Items.Add(item);
                }
            }, new System.Threading.CancellationToken(), TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());

            //wait ui refresh
            Task.Delay(100).Wait();
            return task;
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
