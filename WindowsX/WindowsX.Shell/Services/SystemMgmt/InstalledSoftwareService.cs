using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using WindowsX.Shell.Model.SystemMgmt;

namespace WindowsX.Shell.Services.SystemMgmt
{
    public class InstalledSoftwareResult
    {
        public List<InstalledSoftware> Items { get; set; } = new List<InstalledSoftware>();
        public List<string> Warnings { get; } = new List<string>();
    }

    public class InstalledSoftwareService
    {
        private const string UninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

        public InstalledSoftwareResult GetInstalledSoftware(CancellationToken cancellationToken)
        {
            var result = new InstalledSoftwareResult();
            var unique = new Dictionary<(RegistryHive Hive, string Key, string Name, string Version, string Publisher, string Location), InstalledSoftware>();
            var views = Environment.Is64BitOperatingSystem ? new[] { RegistryView.Registry64, RegistryView.Registry32 } : new[] { RegistryView.Registry32 };
            foreach (var hive in new[] { RegistryHive.LocalMachine, RegistryHive.CurrentUser })
            {
                foreach (var view in views)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        using var baseKey = RegistryKey.OpenBaseKey(hive, view);
                        using var uninstall = baseKey.OpenSubKey(UninstallKey);
                        if (uninstall == null)
                            continue;
                        foreach (var name in uninstall.GetSubKeyNames())
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            try
                            {
                                using var key = uninstall.OpenSubKey(name);
                                if (key == null)
                                    continue;
                                var values = new Dictionary<string, object>();
                                foreach (var property in new[] { "DisplayName", "DisplayVersion", "Publisher", "InstallDate", "InstallLocation", "SystemComponent", "ParentKeyName", "ReleaseType" })
                                    values[property] = key.GetValue(property);
                                var item = CreateSoftware(values, hive == RegistryHive.LocalMachine ? "本机" : "当前用户", view == RegistryView.Registry64 ? "64位" : "32位");
                                if (item == null)
                                    continue;
                                var id = (hive, name.ToUpperInvariant(), item.Name.ToUpperInvariant(), item.Version.ToUpperInvariant(), item.Publisher.ToUpperInvariant(), item.InstallLocation.ToUpperInvariant());
                                if (unique.TryGetValue(id, out var existing))
                                {
                                    if (!existing.RegistryView.Contains(item.RegistryView))
                                        existing.RegistryView += " / " + item.RegistryView;
                                }
                                else
                                {
                                    unique.Add(id, item);
                                }
                            }
                            catch (Exception ex) when (IsRegistryError(ex))
                            {
                                result.Warnings.Add($"{hive} ({view}) {name}：{ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex) when (IsRegistryError(ex))
                    {
                        result.Warnings.Add($"{hive} ({view})：{ex.Message}");
                    }
                }
            }
            result.Items = unique.Values.OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase).ThenBy(x => x.Version).ToList();
            return result;
        }

        public static InstalledSoftware CreateSoftware(IReadOnlyDictionary<string, object> values, string scope, string view)
        {
            string Read(string name) => values.TryGetValue(name, out var value) ? value?.ToString()?.Trim() ?? string.Empty : string.Empty;
            var name = Read("DisplayName");
            var releaseType = Read("ReleaseType");
            var isUpdate = new[] { "Update", "Hotfix", "Security Update", "Update Rollup", "Service Pack" }
                .Contains(releaseType, StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(name) || Read("SystemComponent") == "1" || !string.IsNullOrEmpty(Read("ParentKeyName")) || isUpdate)
                return null;
            var date = Read("InstallDate");
            if (DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                date = parsed.ToString("yyyy-MM-dd");
            return new InstalledSoftware
            {
                Name = name, Version = Read("DisplayVersion"), Publisher = Read("Publisher"), InstallDate = date,
                InstallLocation = Read("InstallLocation"), Scope = scope, RegistryView = view
            };
        }

        public static bool Matches(InstalledSoftware item, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return true;
            query = query.Trim();
            return new[] { item.Name, item.Publisher, item.Version }.Any(value => value?.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0);
        }

        private static bool IsRegistryError(Exception ex) => ex is SecurityException || ex is UnauthorizedAccessException || ex is IOException;
    }
}
