using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.IO.Commands
{
    public class DynamicWallpaperCommands
    {
        public const string DYWALLPAPER_PAUSE = "10086";
        public const string DYWALLPAPER_PLAY = "10087";
        public const string DYWALLPAPER_SETVIDEO = "10088";
        public const string DYWALLPAPER_MUTE = "10089";
        public const string DYWALLPAPER_REPEAT = "10090";
        public const string DYWALLPAPER_RECOVERLAST = "10091";
        public const string DYWALLPAPER_EXIT = "10092";

        public const char COMMANDSPLITCHAR = '_';
        public const string S_OK = "0";
        public const string S_FALSE = "1";
    }
}
