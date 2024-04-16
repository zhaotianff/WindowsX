using WindowsX.Shell.PInvoke;
using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsX.Shell.Util
{
    public class IconHelper
    {
        public static string GetCachedIconPath(string path)
        {
            var temp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WindowsX");
            var iconPath = System.IO.Path.Combine(temp, System.IO.Path.GetFileNameWithoutExtension(path) + ".png");
            if (System.IO.Directory.Exists(temp) == false)
                System.IO.Directory.CreateDirectory(temp);

            if (System.IO.File.Exists(iconPath) == false)
            {
                IntPtr hIcon = IntPtr.Zero;
                if (IconTool.ExtractFirstIconFromFile(path, true, ref hIcon))
                {
                    var bi = ImageHelper.GetBitmapImageFromHIcon(hIcon);
                    ImageHelper.SaveBitmapImageToFile(bi, iconPath);
                }
            }

            return iconPath;
        }
    }
}
