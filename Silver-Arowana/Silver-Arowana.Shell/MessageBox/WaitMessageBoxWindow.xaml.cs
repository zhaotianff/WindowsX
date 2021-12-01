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

namespace Silver_Arowana.Shell.Dialog
{
    /// <summary>
    /// Interaction logic for WaitMessageBoxWindow.xaml
    /// </summary>
    public partial class WaitMessageBoxWindow : TianXiaTech.BlurWindow
    {
        public WaitMessageBoxWindow()
        {
            InitializeComponent();
        }

        private void btn_Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public void SetMessageBox(string title,string content)
        {
            lbl_Title.Content = title;
            tbk_Content.Text = content;
        }

        public void SetConfirmText(string content)
        {
            btn_Ok.Content = Content;
        }
    }
}
