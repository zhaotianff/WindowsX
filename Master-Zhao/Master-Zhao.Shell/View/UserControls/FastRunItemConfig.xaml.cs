using Master_Zhao.Config.Model;
using Master_Zhao.Shell.Util;
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

namespace Master_Zhao.Shell.UserControls
{
    /// <summary>
    /// Interaction logic for FastRunItemConfig.xaml
    /// </summary>
    public partial class FastRunItemConfig : UserControl
    {
        public Action<FastRunItem> OnSaveFastRunConfigCallBack;

        public FastRunItemConfig()
        {
            InitializeComponent();
        }

        private void btnBrowerPath_Click(object sender, RoutedEventArgs e)
        {
            var path = DialogHelper.BrowserSingleFile("可执行程序|*.exe;*.cpl;*.msc;*.bat;*.vbs;*.lnk");

            if(!string.IsNullOrEmpty(path))
            {
                tbox_Path.Text = path;

                var fastRunItem = new FastRunItem();
                fastRunItem.Name = System.IO.Path.GetFileName(path);
                fastRunItem.Path = path;
                fastRunItem.RunType = FastRunType.Applicataion;
                OnSaveFastRunConfigCallBack?.Invoke(fastRunItem);
            }
        }
    }
}
