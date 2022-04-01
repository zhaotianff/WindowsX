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
        public static BitmapImage GetResourceBitmapImage(string path)
        {
            BitmapImage bi = new BitmapImage();

            bi.BeginInit();
            bi.UriSource = new Uri(path, UriKind.Relative);
            bi.EndInit();
            return bi;
        }
    }
}
