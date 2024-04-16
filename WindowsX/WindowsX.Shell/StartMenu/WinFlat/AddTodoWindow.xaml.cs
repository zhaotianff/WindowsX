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

namespace WindowsX.Shell.StartMenu.WinFlat
{
    /// <summary>
    /// AddTodoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddTodoWindow : TianXiaTech.BlurWindow
    {
        public string TodoItem => new TextRange(tbox_Todo.Document.ContentStart, tbox_Todo.Document.ContentEnd).Text;

        public AddTodoWindow(Window window)
        {
            InitializeComponent();
            this.Owner = window;
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
