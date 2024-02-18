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

namespace Master_Zhao.Shell.View.Common.Loading
{
    /// <summary>
    /// CircularLoading.xaml 的交互逻辑
    /// </summary>
    public partial class CircularLoading : UserControl
    {
        public CircularLoading()
        {
            InitializeComponent();
        }

        public void SetStatusText(string status)
        {
            this.lbl_Status.Content = status;
        }
    }
}
