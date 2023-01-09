using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Master_Zhao.AdminTask
{
    public class AdminTaskHelper
    {
        [DllImport("MasterZhaoCore.dll")]
        public static extern void RegisterWindowsPhotoViewerFormat();

        [DllImport("MasterZhaoCore.dll")]
        public static extern void UnregisterWindowsPhotoViewerFormat();

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool DisableStartupItem(IntPtr hKey, [MarshalAs(UnmanagedType.LPWStr)] string szRegPath, uint samDesired, [MarshalAs(UnmanagedType.LPWStr)] string szName, [MarshalAs(UnmanagedType.LPWStr)] string szPath);

        [DllImport("MasterZhaoCore.dll")]
        public static extern bool EnableStartupItem(IntPtr hKey, [MarshalAs(UnmanagedType.LPWStr)]string szRegPath, uint samDesired, [MarshalAs(UnmanagedType.LPWStr)] string szName, [MarshalAs(UnmanagedType.LPWStr)] string szPath);


        public static void DoRegisterTask(string[] args)
        {
            if(args.Length > 0)
            {
                switch(args[0])
                {
                    case "register":
                        RegisterWindowsPhotoViewerFormat();
                        break;
                    case "unregister":
                        UnregisterWindowsPhotoViewerFormat();
                        break;
                    case "startup":
                        StartupTask(args);
                        break;
                }
            }
        }

        private static void StartupTask(string[] args)
        {
            var flag = args[1] == "-enable";
            var hKey = new IntPtr(Convert.ToUInt32(args[2]));
            var regPath = args[3];
            var samDesired = Convert.ToUInt32(args[4]);
            var name = args[5];
            var path = args[6];

            if(flag)
            {
                var result = EnableStartupItem(hKey, regPath, samDesired, name, path);
            }
            else
            {
                DisableStartupItem(hKey, regPath, samDesired, name, path);
            }
        }
    }
}
