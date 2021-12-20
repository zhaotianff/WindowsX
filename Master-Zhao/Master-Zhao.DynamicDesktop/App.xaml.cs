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
        public static string PipeHandleAsString { get; set; } = "";

        public static string VideoPath { get; set; } = "";

        public static int Mute { get; set; }

        public static int Repeat { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ParseArgs(e.Args);
        }

        private void ParseArgs(string[] args)
        {
            if (args.Length > 0)
                PipeHandleAsString = args[0];

            if (args.Length > 1)
                VideoPath = args[1];

            if (args.Length > 2)
            {
                int.TryParse(args[2], out int tempMute);
                Mute = tempMute;
            }

            if(args.Length > 3)
            {
                int.TryParse(args[3], out int tempRepeat);
                Repeat = tempRepeat;
            }
        }
    }
}
