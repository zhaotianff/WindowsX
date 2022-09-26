using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Master_Zhao.Shell
{
    class Startup
    {
        private static readonly string MutexName = "MMutex";
        private static readonly string MasterZhaoProcessName = "Master-Zhao.Shell";

        [STAThread]
        static void Main(string[] args)
        {
            bool createdNew;
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, MutexName, out createdNew);
            if (createdNew)
            {
                Master_Zhao.Shell.App app = new Master_Zhao.Shell.App();
                app.InitializeComponent();
                app.Run();
            }
            else
            {
                //TODO activate window
            }
        }
    }
}
