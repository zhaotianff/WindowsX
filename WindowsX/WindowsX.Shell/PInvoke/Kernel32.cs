using WindowsX.Shell.Model.SystemMgmt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WindowsX.Shell.Util;
using Microsoft.VisualBasic;
using static WindowsX.Shell.PInvoke.Kernel32;
using System.IO;
using System.Collections.ObjectModel;

namespace WindowsX.Shell.PInvoke
{
    public class Kernel32
    {
        [DllImport("Kernel32.dll")]
        public static extern int GetLastError();

        [DllImport("Kernel32.dll",CharSet = CharSet.Unicode)]
        public static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("Kernel32.dll",CharSet= CharSet.Unicode)]
        public static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindFirstFileEx(string lpFileName, 
            FINDEX_INFO_LEVELS fInfoLevelId, 
            out WIN32_FIND_DATA lpFindFileData, 
            FINDEX_SEARCH_OPS fSearchOp, 
            IntPtr lpSearchFilter, 
            uint dwAdditionalFlags);

        [DllImport("Kernel32.dll")]
        public static extern bool FindClose(IntPtr hFindFile);

        [DllImport("Kernel32.dll")]
        public static extern bool FileTimeToLocalFileTime(ref FILETIME lpFileTime,out FILETIME lpLocalFileTime);

        [DllImport("Kernel32.dll")]
        public static extern bool FileTimeToSystemTime(ref FILETIME lpFileTime,out SYSTEMTIME lpSystemTime);

        public static readonly IntPtr INVALID_HANDLE_VALUE = (IntPtr)(-1);
        public static readonly uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        public static readonly uint FIND_FIRST_EX_LARGE_FETCH = 0x00000002;

        public enum FINDEX_INFO_LEVELS
        {
            FindExInfoStandard,
            FindExInfoBasic,
            FindExInfoMaxInfoLevel
        }

        public enum FINDEX_SEARCH_OPS
        {
            FindExSearchNameMatch,
            FindExSearchLimitToDirectories,
            FindExSearchLimitToDevices,
            FindExSearchMaxSearchOp
        }

        public struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }

        public struct FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;

            public DateTime ToDatetime()
            {
                if (FileTimeToLocalFileTime(ref this, out FILETIME lpLocalFileTime))
                {
                    if (FileTimeToSystemTime(ref lpLocalFileTime, out SYSTEMTIME lpSystemTime))
                        return new DateTime(lpSystemTime.wYear, lpSystemTime.wMonth, lpSystemTime.wDay, lpSystemTime.wHour, lpSystemTime.wMinute, lpSystemTime.wSecond);
                }

                return DateTime.Now;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WIN32_FIND_DATA
        {
            /// <summary>
            /// The file attributes of a file.
            /// </summary>
            /// <remarks>
            /// Although the enum we bind to here exists in the .NET Framework
            /// as System.IO.FileAttributes, it is not reliably present.
            /// Portable profiles don't include it, for example. So we have to define our own.
            /// </remarks>
            public System.IO.FileAttributes dwFileAttributes;

            /// <summary>
            /// A FILETIME structure that specifies when a file or directory was created.
            /// If the underlying file system does not support creation time, this member is zero.
            /// </summary>
            public FILETIME ftCreationTime;

            /// <summary>
            /// A FILETIME structure.
            /// For a file, the structure specifies when the file was last read from, written to, or for executable files, run.
            /// For a directory, the structure specifies when the directory is created.If the underlying file system does not support last access time, this member is zero.
            /// On the FAT file system, the specified date for both files and directories is correct, but the time of day is always set to midnight.
            /// </summary>
            public FILETIME ftLastAccessTime;

            /// <summary>
            /// A FILETIME structure.
            /// For a file, the structure specifies when the file was last written to, truncated, or overwritten, for example, when WriteFile or SetEndOfFile are used.The date and time are not updated when file attributes or security descriptors are changed.
            /// For a directory, the structure specifies when the directory is created.If the underlying file system does not support last write time, this member is zero.
            /// </summary>
            public FILETIME ftLastWriteTime;

            /// <summary>
            /// The high-order DWORD value of the file size, in bytes.
            /// This value is zero unless the file size is greater than MAXDWORD.
            /// The size of the file is equal to(nFileSizeHigh* (MAXDWORD+1)) + nFileSizeLow.
            /// </summary>
            public uint nFileSizeHigh;

            /// <summary>
            /// The low-order DWORD value of the file size, in bytes.
            /// </summary>
            public uint nFileSizeLow;

            /// <summary>
            /// If the dwFileAttributes member includes the FILE_ATTRIBUTE_REPARSE_POINT attribute, this member specifies the reparse point tag.
            /// Otherwise, this value is undefined and should not be used.
            /// For more information see Reparse Point Tags.
            /// </summary>
            public uint dwReserved0;

            /// <summary>
            /// Reserved for future use.
            /// </summary>
            public uint dwReserved1;

            /// <summary>
            /// The name of the file.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;

            /// <summary>
            /// An alternative name for the file.
            /// This name is in the classic 8.3 file name format.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }


        public static long EnumerateSubDirectory(string dir, ObservableCollection<DiskPath> diskPathList,bool isEnumFile = false,bool isEnumOnce = false)
        {
            // 搜索指定类型文件
            string pszFileName;
            string pTempSrc;
            WIN32_FIND_DATA FileData = new WIN32_FIND_DATA();
            long dirSize = 0;

            // 构造搜索文件类型字符串, *.*表示搜索所有文件类型
            pszFileName = $"{dir}\\*.*";

            // 搜索第一个文件
            IntPtr hFile = FindFirstFile(pszFileName, out FileData);
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
                    if (dir.EndsWith("\\"))
                        dir = dir.Replace("\\", "");
                    pTempSrc = $"{dir}\\{FileData.cFileName}";
                    // 判断是否是目录还是文件
                    if ((FileData.dwFileAttributes & System.IO.FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        DiskPath diskPath = new DiskPath();
                        diskPath.DiskPathType = DiskPathType.Folder;
                        diskPath.DisplayName = FileData.cFileName;
                        diskPath.Path = pTempSrc;
                        diskPath.CreationTime = FileData.ftCreationTime.ToDatetime();
                        diskPath.LastAccessTime = FileData.ftLastAccessTime.ToDatetime();
                        diskPath.LastWriteTime = FileData.ftLastWriteTime.ToDatetime();

                        // 目录, 则继续往下递归遍历文件
                        // Use FindExSearchLimitToDirectories instead
                        if (isEnumOnce == false)
                        {
                            diskPath.Children = new ObservableCollection<DiskPath>();
                            var currentDirSize = EnumerateSubDirectory(pTempSrc, diskPath.Children,true);
                            diskPath.Size = currentDirSize;
                            dirSize += currentDirSize;
                        }
                        diskPathList.Add(diskPath);
                    }
                    else
                    {
                        var fileSize = ((long)FileData.nFileSizeHigh) << 32 | ((long)FileData.nFileSizeLow & 0xFFFFFFFFL);
                        dirSize += fileSize;

                        if (isEnumFile == true)
                        {
                            DiskPath diskPath = new DiskPath();
                            diskPath.DiskPathType = DiskPathType.File;
                            diskPath.DisplayName = Path.GetFileName(FileData.cFileName);
                            diskPath.Path = pTempSrc;
                            diskPath.Size = fileSize;
                            diskPath.CreationTime = FileData.ftCreationTime.ToDatetime();
                            diskPath.LastAccessTime = FileData.ftLastAccessTime.ToDatetime();
                            diskPath.LastWriteTime = FileData.ftLastWriteTime.ToDatetime();
                            diskPathList.Add(diskPath);
                        }
                        // 文件
                        //printf("%s\n", pTempSrc);
                    }

                    // 搜索下一个文件
                } while (FindNextFile(hFile, out FileData));
            }

            // 关闭文件句柄
            FindClose(hFile);

            return dirSize;
        }

        public static long EnumerateSubDirectoryEx(string dir, ObservableCollection<DiskPath> diskPathList, bool isEnumFile = false, bool isEnumOnce = false)
        {
            // 搜索指定类型文件
            string pszFileName;
            string pTempSrc;
            WIN32_FIND_DATA FileData = new WIN32_FIND_DATA();
            long dirSize = 0;

            // 构造搜索文件类型字符串, *.*表示搜索所有文件类型
            if (dir.EndsWith("\\"))
                dir = dir.Replace("\\", "");
            pszFileName = $"{dir}\\*.*";

            // 搜索第一个文件
            IntPtr hFile = FindFirstFileEx(pszFileName, FINDEX_INFO_LEVELS.FindExInfoBasic, out FileData, FINDEX_SEARCH_OPS.FindExSearchNameMatch, IntPtr.Zero, FIND_FIRST_EX_LARGE_FETCH);
            if (INVALID_HANDLE_VALUE != hFile)
            {
                do
                {
                    // 要过滤掉 当前目录"." 和 上一层目录"..", 否则会不断进入死循环遍历
                    if ('.' == FileData.cFileName[0])
                    {
                        if (FileData.cFileName.Length == 1 || FileData.cFileName.Length == 2)
                        {
                            continue;
                        }
                    }
                    // 拼接文件路径	
                    pTempSrc = $"{dir}\\{FileData.cFileName}";
                    // 判断是否是目录还是文件
                    if ((FileData.dwFileAttributes & System.IO.FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        DiskPath diskPath = new DiskPath();
                        diskPath.DiskPathType = DiskPathType.Folder;
                        diskPath.DisplayName = FileData.cFileName;
                        diskPath.Path = pTempSrc;
                        diskPath.CreationTime = FileData.ftCreationTime.ToDatetime();
                        diskPath.LastAccessTime = FileData.ftLastAccessTime.ToDatetime();
                        diskPath.LastWriteTime = FileData.ftLastWriteTime.ToDatetime();

                        // 目录, 则继续往下递归遍历文件
                        // Use FindExSearchLimitToDirectories instead
                        if (isEnumOnce == false)
                        {
                            diskPath.Children = new ObservableCollection<DiskPath>();
                            var currentDirSize = EnumerateSubDirectoryEx(pTempSrc, diskPath.Children, true);
                            diskPath.Size = currentDirSize;
                            dirSize += currentDirSize;
                        }
                        diskPathList.Add(diskPath);
                    }
                    else
                    {
                        var fileSize = ((long)FileData.nFileSizeHigh) << 32 | ((long)FileData.nFileSizeLow & 0xFFFFFFFFL);
                        dirSize += fileSize;

                        if (isEnumFile == true)
                        {
                            DiskPath diskPath = new DiskPath();
                            diskPath.DiskPathType = DiskPathType.File;
                            diskPath.DisplayName = Path.GetFileName(FileData.cFileName);
                            diskPath.Path = pTempSrc;
                            diskPath.Size = fileSize;
                            diskPath.CreationTime = FileData.ftCreationTime.ToDatetime();
                            diskPath.LastAccessTime = FileData.ftLastAccessTime.ToDatetime();
                            diskPath.LastWriteTime = FileData.ftLastWriteTime.ToDatetime();
                            diskPathList.Add(diskPath);
                        }
                        // 文件
                        //printf("%s\n", pTempSrc);
                    }

                    // 搜索下一个文件
                } while (FindNextFile(hFile, out FileData));
            }

            // 关闭文件句柄
            FindClose(hFile);

            return dirSize;
        }
    }
}
