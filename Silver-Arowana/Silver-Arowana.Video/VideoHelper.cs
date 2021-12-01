using System;

namespace Silver_Arowana.Video
{
    public static class VideoHelper
    {
        public static string GenerateVideoThumbnail(string path)
        {
            var index = path.LastIndexOf('.');
            var thumbPath = path.Substring(0, index) + ".png";
            return thumbPath;
        }

        public static System.Windows.Media.Imaging.BitmapImage GetVideoThumbnail(string path)
        {
            return null;
        }
    }
}
