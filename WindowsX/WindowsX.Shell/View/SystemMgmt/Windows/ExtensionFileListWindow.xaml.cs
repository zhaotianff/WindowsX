using WindowsX.Shell.PInvoke;
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
using System.Windows.Shapes;

namespace WindowsX.Shell.View.SystemMgmt.Windows
{
    /// <summary>
    /// ExtensionFileListWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ExtensionFileListWindow : TianXiaTech.BlurWindow
    {
        public ExtensionFileListWindow(List<string> fileList)
        {
            InitializeComponent();

            if (fileList.Count > 100)
            {
                this.lst_File.ItemsSource = fileList.Take(100);
            }
            else
            {
                this.lst_File.ItemsSource = fileList;
            }
            this.Background = Application.Current.MainWindow.Background;
        }

        private void lst_File_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lst_File.SelectedItem == null)
                return;

            DesktopTool.SelectFile(lst_File.SelectedItem.ToString());
        }
    }
}
