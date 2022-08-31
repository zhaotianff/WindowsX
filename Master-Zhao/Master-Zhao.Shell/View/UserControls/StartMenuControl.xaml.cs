using Master_Zhao.Shell.Util;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

namespace Master_Zhao.Shell.View.UserControls
{
    /// <summary>
    /// StartMenuControl.xaml 的交互逻辑
    /// </summary>
    public partial class StartMenuControl : UserControl
    {
        public delegate void MyDel();

        public StartMenuControl()
        {
            InitializeComponent();        
        }

        private void set_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            MyDel dd = TT; 
            var result = Master_Zhao.Shell.PInvoke.SystemTool.HookStart(Marshal.GetFunctionPointerForDelegate(dd));
        }

        public void TT()
        {
            MessageBox.Show("TT");
        }
    }
}
