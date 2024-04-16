using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WindowsX.DynamicDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string PipeHandleAsString { get; set; } = "";

        public static string VideoPath { get; set; } = "";

        public static string Mute { get; set; } = WindowsX.IO.Commands.DynamicWallpaperCommands.S_OK;

        public static string Repeat { get; set; } = WindowsX.IO.Commands.DynamicWallpaperCommands.S_OK;

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
                Mute = args[2];
            }

            if(args.Length > 3)
            {
                Repeat = args[3];
            }
        }
    }
}
