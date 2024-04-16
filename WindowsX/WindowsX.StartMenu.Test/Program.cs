using System;
using System.IO;

namespace WindowsX.StartMenu.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
            //PrintPath(startMenuPath);

            var programPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms);
            PrintPath(programPath);
        }

        static void PrintPath(string path)
        {
            if (System.IO.Directory.Exists(path) == false)
                return;

            DirectoryInfo dir = new DirectoryInfo(path);

            var fileEntry = dir.GetFileSystemInfos();

            foreach (var file in fileEntry)
            {
                var fileinfo = file as FileInfo;

                if (fileinfo != null)
                    Console.WriteLine(fileinfo.FullName);
                else
                    PrintPath((file as DirectoryInfo).FullName);
            }
        }
    }
}
