using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace Master_Zhao.Shell.Util
{
    /// <summary>
    /// from https://github.com/zhaotianff/Windows-run-tool/blob/main/Windows-run-tool/Helper/DirectoryExtension.cs 
    /// </summary>
    public static class DirectoryExtension
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

        public static bool CanAccess(this DirectoryInfo directoryInfo)
        {
            try
            {
                var accessControl = directoryInfo.GetAccessControl();
                AuthorizationRuleCollection rules = accessControl.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
                WindowsIdentity identity = WindowsIdentity.GetCurrent();

                foreach (FileSystemAccessRule rule in rules)
                {
                    if (identity.Groups.Contains(rule.IdentityReference))
                    {
                        if ((rule.FileSystemRights & FileSystemRights.CreateFiles) == FileSystemRights.CreateFiles
                            || (rule.FileSystemRights & FileSystemRights.CreateDirectories) == FileSystemRights.CreateDirectories
                            || (rule.FileSystemRights & FileSystemRights.ListDirectory) == FileSystemRights.ListDirectory)
                        {
                            if (rule.AccessControlType == AccessControlType.Allow)
                                return true;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return false;

        }
    }
}
