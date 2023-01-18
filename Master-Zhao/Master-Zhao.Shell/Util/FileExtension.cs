using System;
using System.Collections.Generic;
using System.Text;

namespace Master_Zhao.Shell.Util
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
    }
}
