using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Shell.Services.Beautify.Contract
{
    public interface IDesktopService
    {
        string GetCurrentWallpaperPath();

        bool EmbedDesktopWindowHandle(IntPtr hwnd);

        bool CancelEmbededDesktopWindowHandle();

        bool CloseEmbededDesktopWindowHandle();
    }
}
