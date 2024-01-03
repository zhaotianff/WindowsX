using Master_Zhao.Shell.Model.SystemMgmt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Master_Zhao.Shell.Util;

namespace Master_Zhao.Shell.PInvoke
{
    public class Kernel32
    {
        [DllImport("Kernel32.dll")]
        public static extern int GetLastError();

        [DllImport("Kernel32.dll")]
        public static extern IntPtr FindFirstFile([MarshalAs(UnmanagedType.LPWStr)] string lpFileName, ref WIN32_FIND_DATAA lpFindFileData);

        [DllImport("Kernel32.dll")]
        public static extern bool FindNextFile(IntPtr hFindFile, IntPtr lpFindFileData);

        [DllImport("Kernel32.dll")]
        public static extern bool FindClose(IntPtr hFindFile);

        public static readonly IntPtr INVALID_HANDLE_VALUE = (IntPtr)(-1);
        public static readonly uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

        public struct WIN32_FIND_DATAA
        {
            public uint dwFileAttributes;
            public IntPtr ftCreationTime;
            public IntPtr ftLastAccessTime;
            public IntPtr ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
            public uint dwFileType; // Obsolete. Do not use.
            public uint dwCreatorType; // Obsolete. Do not use
            public uint wFinderFlags; // Obsolete. Do not use
        }


        public static void EnumerateSubDirectory(string dir, List<DiskPath> diskPathList)
        {
            // 搜索指定类型文件
            string pszFileName;
            string pTempSrc;
            IntPtr ptrFileData = IntPtr.Zero;
            WIN32_FIND_DATAA FileData = new WIN32_FIND_DATAA();

            // 构造搜索文件类型字符串, *.*表示搜索所有文件类型
            pszFileName = $"{dir}\\*.*";

            // 搜索第一个文件
            IntPtr hFile = FindFirstFile(pszFileName, ref FileData);
            FileData = ptrFileData.PtrToStructure<WIN32_FIND_DATAA>();
            if (INVALID_HANDLE_VALUE != hFile)
            {
                do
                {
                    // 要过滤掉 当前目录"." 和 上一层目录"..", 否则会不断进入死循环遍历
                    if ('.' == FileData.cFileName[0])
                    {
                        continue;
                    }
                    // 拼接文件路径	
                    pTempSrc = $"{dir}\\{FileData.cFileName}";
                    // 判断是否是目录还是文件
                    if ((FileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) == 1)
                    {
                        // 目录, 则继续往下递归遍历文件
                        EnumerateSubDirectory(pTempSrc, diskPathList);
                        //printf("%s\n", pTempSrc);
                    }
                    else
                    {
                        // 文件
                        //printf("%s\n", pTempSrc);
                    }

                    // 搜索下一个文件
                } while (FindNextFile(hFile, ptrFileData));
            }

            // 关闭文件句柄
            FindClose(hFile);
        }
    }
}
