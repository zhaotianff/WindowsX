﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Shell.Util
{
    public class ProcessHelper
    {
        public static void OpenUrl(string url)
        {
            var processStartInfo = new ProcessStartInfo(url) { UseShellExecute = true, Verb = "open" };
            System.Diagnostics.Process.Start(processStartInfo);
        }

        public static void Execute(string path,bool useShellExecute = true)
        {
            if (string.IsNullOrEmpty(path))
                return;

            var psInfo = new System.Diagnostics.ProcessStartInfo();
            psInfo.UseShellExecute = useShellExecute;
            psInfo.FileName = path;
            Process.Start(psInfo);
        }
    }
}
