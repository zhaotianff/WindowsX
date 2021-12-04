using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Master_Zhao.DynamicDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string VideoPath { get; set; } = "";

        public static bool Mute { get; set; } = true;

        public static bool Repeat { get; set; } = true;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //parse args
            if(e.Args.Length > 0)
            {
                VideoPath = e.Args[0];
            }
        }
    }
}
