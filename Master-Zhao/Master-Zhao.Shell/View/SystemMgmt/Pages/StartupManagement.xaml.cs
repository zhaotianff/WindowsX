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
            //temp 10
            int count = 10;
            var itemSize = Marshal.SizeOf<tagSTARTUPITEM>();
            int size = count * itemSize;
            IntPtr buffer = Marshal.AllocHGlobal(size);
            var result = PInvoke.StartupTool.GetStartupItems(buffer, size, ref count);

            if (result == false)
                return;

            for(int i = 0;i<count;i++)
            {
                byte[] startupItemBuffer = new byte[itemSize];
                Marshal.Copy(buffer, startupItemBuffer, 0, itemSize);

                IntPtr newPtr = Marshal.AllocHGlobal(itemSize);
                Marshal.Copy(startupItemBuffer, 0, newPtr, itemSize);

                var str = Marshal.PtrToStructure<tagSTARTUPITEM>(newPtr);
                
                buffer = new IntPtr(buffer.ToInt64() + itemSize);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStartUpItem();
        }
    }
}
