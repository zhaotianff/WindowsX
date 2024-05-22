using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsX.Shell.Util
{
    public class ProcessHelper
    {
        public static readonly int INVALID_PROCESS_ID = -1;

        public static void OpenUrl(string url)
        {
            var processStartInfo = new ProcessStartInfo(url) { UseShellExecute = true, Verb = "open" };
            System.Diagnostics.Process.Start(processStartInfo);
        }

        public static void OpenWindowsHelp()
        {
            var url = "https://go.microsoft.com/fwlink/?LinkId=528884";
            OpenUrl(url);
        }

        public static Process Execute(string path,bool useShellExecute = true)
        {
            return ExecuteInternal(path, null, useShellExecute);
        }

        public static Process Execute(string path,string[] args)
        {
            return ExecuteInternal(path, args, true);
        }

        private static Process ExecuteInternal(string path,string[] args,bool useShellExecute)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var psInfo = new System.Diagnostics.ProcessStartInfo();
            psInfo.UseShellExecute = useShellExecute;
            if (args != null)
            {
                foreach (var arg in args)
                {
                    psInfo.ArgumentList.Add(arg);
                }
            }
            psInfo.FileName = path;
            return Process.Start(psInfo);
        }

        public static void KillProcess(string processName)
        {
            var process = Process.GetProcesses().FirstOrDefault(x => x.ProcessName == processName);
            process?.Kill();
        }

        public static IntPtr FindProcessWindow(string processName)
        {
             var process = Process.GetProcesses().FirstOrDefault(x => x.ProcessName == processName);

            if (process != null)
                return process.MainWindowHandle;

            return IntPtr.Zero;
        }

        public static Process FindProcess(string processName)
        {
            return Process.GetProcesses().FirstOrDefault(x => x.ProcessName == processName);
        }

        public static void OpenModernSetting()
        {
            Execute("ms-settings:main");
        }

        public async static Task ExecuteAdminTask(string[] args, string adminTaskExePath = null)
        {
            if (string.IsNullOrEmpty(adminTaskExePath))
                adminTaskExePath = Environment.CurrentDirectory + "\\WindowsX.AdminTask.exe";

            if (System.IO.File.Exists(adminTaskExePath))
            {
                var process = Execute(adminTaskExePath, args);

                if (process != null)
                {
                    await Task.Delay(1000);
                    process.Kill();
                }
            }
        }

        public static int GetProcessId(string processName)
        {
            var findProcess = Process.GetProcesses().FirstOrDefault(x => x.ProcessName.ToUpper() == processName.ToUpper());
            if (findProcess != null)
                return findProcess.Id;

            return INVALID_PROCESS_ID;
        }

        public static int GetExplorerProcessId()
        {
            var explorerProcessName = "explorer";
            return GetProcessId(explorerProcessName);
        }
    }
}
