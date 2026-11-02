using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsX.Shell.Infrastructure.DesignPattern;
using WindowsX.Shell.PInvoke;
using WindowsX.Shell.Services.Beautify.Contract;

namespace WindowsX.Shell.Services.Beautify
{
    public class DesktopService : SingletonBase<DesktopService>, IDesktopService
    {
        public bool CancelEmbededDesktopWindowHandle()
        {
            throw new NotImplementedException();
        }

        public bool CloseEmbededDesktopWindowHandle()
        {
            throw new NotImplementedException();
        }

        public bool EmbedDesktopWindowHandle(IntPtr hwnd)
        {
            throw new NotImplementedException();
        }

        public string GetCurrentWallpaperPath()
        {
            var sb = new StringBuilder(DesktopTool.MAX_PATH);
            if (DesktopTool.GetBackground(sb))
            {
                return sb.ToString();
                
            }

            throw new Exception("Get current wallpaper failed!");
        }
    }
}
