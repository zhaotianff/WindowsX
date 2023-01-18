using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master_Zhao.Shell.Util
{
    /// <summary>
    /// from https://github.com/zhaotianff/Windows-run-tool/blob/main/Windows-run-tool/Helper/DirectoryExtension.cs 
    /// </summary>
    public class DirectoryExtension
    {
        public static List<string> GetFiles(string path, params string[] extensions)
        {
            var list = new List<string>();

            try
            {
                var files = System.IO.Directory.GetFiles(path);

                foreach (var item in files)
                {
                    var extension = System.IO.Path.GetExtension(item);

                    if (extensions.Contains(extension))
                        list.Add(item);
                }

                return list;
            }
            catch
            {
                return list;
            }
        }
    }
}
