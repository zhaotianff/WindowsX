using Master_Zhao.Shell.PInvoke;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Master_Zhao.Shell.Util
{
    public class ImageHelper
    {
        public static BitmapImage GetBitmapImageFromLocalFile(string path,bool useStream = false)
        {
            return InternalGetBitmapImage(path, UriKind.Absolute,useStream);
        }

        public static BitmapImage GetBitmapImageFromResource(string path)
        {
            return InternalGetBitmapImage(path, UriKind.Relative);
        }

        private static BitmapImage InternalGetBitmapImage(string path,UriKind uriKind,bool useStream = false)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (File.Exists(path) == false)
                return null;

            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            if(useStream)
            {
                var buffer = System.IO.File.ReadAllBytes(path);
                var ms = new MemoryStream(buffer);
                bi.StreamSource = ms;
            }
            else
            {
                bi.UriSource = new Uri(path, uriKind);
            }
            bi.EndInit();
            return bi;
        }

        public static BitmapSource GetBitmapImageFromHIcon(IntPtr hIcon)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(hIcon, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public static BitmapSource GetIconImageFromFile(string filePath)
        {
            if (System.IO.File.Exists(filePath) == false)
                return null;

            IntPtr hIcon = IntPtr.Zero;
            IconTool.ExtractFirstIconFromFile(filePath, true, ref hIcon);

            if (hIcon != IntPtr.Zero)
            {
                return GetBitmapImageFromHIcon(hIcon);
            }

            return null;
        }


        public static BitmapSource GetBitmapImageFromHBitmap(IntPtr hBitmap)
        {
            try
            {
                var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                return source;
            }
            finally
            {
                GDIPlus.DeleteObject(hBitmap);
            }
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

        public static void SaveImageSourceAsFile(BitmapSource imageSource, string fileName)
        {
            BitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageSource));
            using (Stream stream = File.Create(fileName))
            {
                encoder.Save(stream);
            }
        }
    }
}
