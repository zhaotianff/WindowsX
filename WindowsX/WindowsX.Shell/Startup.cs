using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace WindowsX.Shell
{
    class Startup
    {
        private static readonly string MutexName = "MMutex";

        [STAThread]
        static void Main(string[] args)
        {
            if (Array.Exists(args, arg => string.Equals(arg, "--autostart", StringComparison.OrdinalIgnoreCase)))
                Environment.CurrentDirectory = AppContext.BaseDirectory;

            bool createdNew;
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, MutexName, out createdNew);
            if (createdNew)
            {
                WindowsX.Shell.App app = new WindowsX.Shell.App();
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
