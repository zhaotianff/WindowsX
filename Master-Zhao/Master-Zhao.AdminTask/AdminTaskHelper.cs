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


        public static void DoRegisterTask(string[] args)
        {
            if(args.Length > 0)
            {
                if (args[0] == "register")
                {
                    RegisterWindowsPhotoViewerFormat();
                }
                else
                {
                    UnregisterWindowsPhotoViewerFormat();
                }
            }
            

        }
    }
}
