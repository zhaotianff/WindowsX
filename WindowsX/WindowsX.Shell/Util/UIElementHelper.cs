using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WindowsX.Shell.Util
{
    public class UIElementHelper
    {
        public static string ProImgPath { get; private set; }

        public static void SaveUIElementAsFile(FrameworkElement element, string fileName)
        {
            var render = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96, 96, PixelFormats.Default);
            render.Render(element);
            BitmapEncoder encoder = new JpegBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(render));
            using (FileStream fs = File.Create(fileName))
            {
                encoder.Save(fs);
            }
        }
    }
}
