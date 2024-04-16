using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsX.Shell.Util
{
    public class FileExtension
    {
        public static string GetFileDescription(string filePath)
        {
            try
            {
                return System.Diagnostics.FileVersionInfo.GetVersionInfo(filePath).FileDescription;
            }
            catch
            {
                return "";
            }
        }

        public static string GetFileVersion(string filePath)
        {
            try
            {
                if (System.IO.File.Exists(filePath) == false)
                    return "";

                return System.Diagnostics.FileVersionInfo.GetVersionInfo(filePath).FileVersion;
            }
            catch
            {
                return "";
            }
        }
    }
}
