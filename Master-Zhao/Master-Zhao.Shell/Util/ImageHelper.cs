using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Master_Zhao.Shell.Util
{
    public class ImageHelper
    {
        public static BitmapImage GetBitmapImageFromLocalFile(string path)
        {
            return InternalGetBitmapImage(path, UriKind.Absolute);
        }

        public static BitmapImage GetBitmapImageFromResource(string path)
        {
            return InternalGetBitmapImage(path, UriKind.Relative);
        }

        private static BitmapImage InternalGetBitmapImage(string path,UriKind uriKind)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(path, uriKind);
            bi.EndInit();
            return bi;
        }

        public static BitmapSource GetBitmapImageFromHIcon(IntPtr hIcon)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(hIcon, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public static void SaveBitmapImageToFile(BitmapSource image, string filePath)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
    }
}
