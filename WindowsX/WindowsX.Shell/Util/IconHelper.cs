using WindowsX.Shell.PInvoke;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace WindowsX.Shell.Util
{
    public class IconHelper
    {
        private static ImageSource defaultIcon;

        private static Dictionary<string, ImageSource> iconCacheDic = new Dictionary<string, ImageSource>();

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

        public static ImageSource GetExtensionIcon(string fileExtension)
        {
            if (iconCacheDic.ContainsKey(fileExtension))
                return iconCacheDic[fileExtension];

            IntPtr hIcon = IntPtr.Zero;
            IconTool.GetFileExtensionAssocIcon(fileExtension, ref hIcon);
            if (hIcon != IntPtr.Zero)
            {
                ImageSource imageSource = ImageHelper.GetBitmapImageFromHIcon(hIcon);
                IconTool.DestroyIcon(hIcon);

                iconCacheDic[fileExtension] = imageSource;

                return imageSource;
            }
            else
            {
                if (defaultIcon == null)
                {
                    IconTool.GetShell32Icon(0, ref hIcon);
                    defaultIcon = ImageHelper.GetBitmapImageFromHIcon(hIcon);
                    IconTool.DestroyIcon(hIcon);
                }

                return defaultIcon;
            }
        }

        public static void ClearIconCache()
        {
            //TODO 
            //clear cache
            iconCacheDic.Clear();
        }
    }
}
