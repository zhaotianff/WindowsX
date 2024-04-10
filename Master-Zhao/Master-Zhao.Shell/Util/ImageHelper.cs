using Master_Zhao.Shell.PInvoke;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static Master_Zhao.Shell.PInvoke.Kernel32;

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

            if (uriKind == UriKind.Absolute && File.Exists(path) == false)
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

        public static void GenerateThumbnailFile(string imagePath,int width,string thumbPath)
        {
            using(System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(imagePath))
            {
                var ratio = (float)width / bitmap.Width;
                var height = (int)(bitmap.Height * ratio);
                using (var thumbBitmap = bitmap.GetThumbnailImage(width, height, null, IntPtr.Zero))
                {
                    ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
                    ImageCodecInfo jpegEncoder = GetEncoder(ImageFormat.Jpeg);
                    EncoderParameters prms = new EncoderParameters(1);
                    prms.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
                    thumbBitmap.Save(thumbPath, jpegEncoder, prms);
                }
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
