using Microsoft.Win32;
using System;
using System.IO;
using WindowsX.Shell.Services.Application.Contracts;

namespace WindowsX.Shell.Services.Application
{
    public sealed class ApplicationStartupService : IApplicationStartupService
    {
        public const string ApplicationName = "WindowsX";

        private readonly string executablePath;
        private readonly string registryPath;
        private readonly string command;

        public ApplicationStartupService()
        {
            executablePath = Path.Combine(AppContext.BaseDirectory, typeof(ApplicationStartupService).Assembly.GetName().Name + ".exe");
            registryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
            command = "\"" + executablePath + "\" --autostart";
        }

        public bool IsEnabled()
        {
            using var key = Registry.CurrentUser.OpenSubKey(registryPath);
            return string.Equals(key?.GetValue(ApplicationName) as string, command, StringComparison.OrdinalIgnoreCase);
        }

        public void SetEnabled(bool enabled)
        {
            if (enabled)
            {
                using var key = Registry.CurrentUser.CreateSubKey(registryPath, true);
                key.SetValue(ApplicationName, command, RegistryValueKind.String);
            }
            else
            {
                using var key = Registry.CurrentUser.OpenSubKey(registryPath, true);
                key?.DeleteValue(ApplicationName, false);
            }
        }
    }
}
