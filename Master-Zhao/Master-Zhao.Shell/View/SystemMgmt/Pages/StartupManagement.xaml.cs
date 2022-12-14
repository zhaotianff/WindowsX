using Master_Zhao.Shell.PInvoke;
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

namespace Master_Zhao.Shell.View.SystemMgmt.Pages
{
    /// <summary>
    /// StartupManagement.xaml 的交互逻辑
    /// </summary>
    public partial class StartupManagement : Page
    {
        public StartupManagement()
        {
            InitializeComponent();
        }

        private void LoadStartUpItem()
        {
            int count = 0;
            var itemPtr = PInvoke.StartupTool.GetStartupItems(ref count);

            var offset = 0;
            var itemSize = Marshal.SizeOf<tagSTARTUPITEM>();
            for(int i = 0;i<count;i++)
            {
                byte[] buffer = new byte[itemSize];
                Marshal.Copy(itemPtr, buffer, offset, itemSize);

                IntPtr newPtr = Marshal.AllocHGlobal(itemSize);
                Marshal.Copy(buffer, 0, newPtr, itemSize);

                var str = Marshal.PtrToStructure<tagSTARTUPITEM>(newPtr);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStartUpItem();
        }
    }
}
