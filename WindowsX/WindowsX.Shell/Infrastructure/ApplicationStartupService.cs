using Microsoft.Win32;
using System;
using System.IO;

namespace WindowsX.Shell.Infrastructure
{
    public interface IApplicationStartupService
    {
        bool IsEnabled();
        void SetEnabled(bool enabled);
    }

    public sealed class ApplicationStartupService : IApplicationStartupService
    {
        public const string ValueName = "WindowsX";
        private readonly string registryPath;
        private readonly string command;

        public ApplicationStartupService() : this(
            Path.Combine(AppContext.BaseDirectory, typeof(ApplicationStartupService).Assembly.GetName().Name + ".exe"),
            @"Software\Microsoft\Windows\CurrentVersion\Run")
        {
        }

        public ApplicationStartupService(string executablePath, string registryPath)
        {
            if (string.IsNullOrWhiteSpace(executablePath) || !Path.IsPathFullyQualified(executablePath) || executablePath.Contains('"'))
                throw new ArgumentException("启动程序必须使用有效的绝对路径。", nameof(executablePath));
            if (string.IsNullOrWhiteSpace(registryPath))
                throw new ArgumentException("注册表路径不能为空。", nameof(registryPath));

            this.registryPath = registryPath;
            command = "\"" + executablePath + "\" --autostart";
        }

        public bool IsEnabled()
        {
            using var key = Registry.CurrentUser.OpenSubKey(registryPath);
            return string.Equals(key?.GetValue(ValueName) as string, command, StringComparison.OrdinalIgnoreCase);
        }

        public void SetEnabled(bool enabled)
        {
            if (enabled)
            {
                using var key = Registry.CurrentUser.CreateSubKey(registryPath, true);
                key.SetValue(ValueName, command, RegistryValueKind.String);
            }
            else
            {
                using var key = Registry.CurrentUser.OpenSubKey(registryPath, true);
                key?.DeleteValue(ValueName, false);
            }
        }
    }
}
