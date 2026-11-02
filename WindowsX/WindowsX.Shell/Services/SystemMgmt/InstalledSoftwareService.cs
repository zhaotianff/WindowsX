using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml;
using WindowsX.Shell.Model.SystemMgmt;
using WindowsX.Shell.Infrastructure.Extension;

namespace WindowsX.Shell.Services.SystemMgmt
{
    public class InstalledSoftwareService
    {
        private const string UninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        private string[] RegisterTargetKeyArray = { "DisplayName", "DisplayVersion", "Publisher", "InstallDate", "InstallLocation", "SystemComponent", "ParentKeyName", "ReleaseType", "EstimatedSize" };
        private string[] SoftwareUpdateKeywordArray = { "Update", "Hotfix", "Security Update", "Update Rollup", "Service Pack" };

        public InstalledSoftwareResult GetInstalledSoftware(CancellationToken cancellationToken)
        {
            var result = new InstalledSoftwareResult();
            Dictionary<InstalledSoftwareKey, InstalledSoftware> unique = new Dictionary<InstalledSoftwareKey, InstalledSoftware>();
            var hklmWarnings = LoadInstalledSoftwareFromRegistryHive(RegistryHive.LocalMachine, cancellationToken,unique);
            var hkcuWarnings = LoadInstalledSoftwareFromRegistryHive(RegistryHive.CurrentUser, cancellationToken,unique);
            result.Items = unique.Values.OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase).ThenBy(x => x.Version).ToList();
            result.Warnings.AddRange(hklmWarnings);
            result.Warnings.AddRange(hkcuWarnings);
            return result;
        }

        private List<string> LoadInstalledSoftwareFromRegistryHive(RegistryHive hive, CancellationToken cancellationToken, Dictionary<InstalledSoftwareKey, InstalledSoftware> unique)
        {
            var views = Environment.Is64BitOperatingSystem ? new[] { RegistryView.Registry64, RegistryView.Registry32 } : new[] { RegistryView.Registry32 };
            List<string> warnings = new List<string>();

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

                        FetchInstalledSoftware(hive, view, uninstall, name, unique);
                    }
                }
                catch (Exception ex) when (IsRegistryError(ex))
                {
                    warnings.Add($"{hive} ({view})：{ex.Message}");
                }
            }

            return warnings;
        }

        private void FetchInstalledSoftware(RegistryHive hive, RegistryView view, RegistryKey uninstall, string name, Dictionary<InstalledSoftwareKey, InstalledSoftware> unique)
        {
            using var key = uninstall.OpenSubKey(name);

            if (key == null)
                return;

            var values = new Dictionary<string, object>();
            foreach (var property in RegisterTargetKeyArray)
            {
                values[property] = key.GetValue(property);
            }

            var scope = hive == RegistryHive.LocalMachine ? "本机" : "当前用户";
            var item = GetInstalledSoftwareItem(values, scope, view);

            if (item == null)
                return;

            var id = new InstalledSoftwareKey(hive, name.ToUpperInvariant(), item.Name.ToUpperInvariant(), item.Version.ToUpperInvariant(), item.Publisher.ToUpperInvariant(), item.InstallLocation.ToUpperInvariant());
            if (!unique.TryGetValue(id, out _))
            {
                unique.Add(id, item);
            }
        }

        public InstalledSoftware GetInstalledSoftwareItem(IReadOnlyDictionary<string, object> values, string scope, RegistryView registryView)
        {
            InstalledSoftware installedSoftware = new InstalledSoftware();

            var view = registryView == RegistryView.Registry64 ? "64位" : "32位";
            var name = Read(values, "DisplayName");
            var date = Read(values, "InstallDate");
            var estimatedSize = Read(values, "EstimatedSize").ToUint32();
            var releaseType = Read(values,"ReleaseType");
            var systemComponent = Read(values, "SystemComponent") == "1";
            var parentKeyName = Read(values, "ParentKeyName");
            var isUpdate = SoftwareUpdateKeywordArray.Contains(releaseType, StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(name) || systemComponent || !string.IsNullOrEmpty(parentKeyName) || isUpdate)
                return null;

            if (DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                date = parsed.ToString("yyyy-MM-dd");
            }

            installedSoftware.Name = name;
            installedSoftware.Version = Read(values, "DisplayVersion");
            installedSoftware.InstallDate = date;
            installedSoftware.InstallLocation = Read(values, "InstallLocation");
            installedSoftware.Scope = scope;
            installedSoftware.RegistryView = view;
            installedSoftware.EstimatedSize = estimatedSize  == 0 ? "" :(estimatedSize / 1024f).ToString("0.0") + "MB";
            installedSoftware.Publisher = Read(values, "Publisher");

            return installedSoftware;
        }

        private string Read(IReadOnlyDictionary<string, object> values,string name)
        {
            return values.TryGetValue(name, out var value) ? value?.ToString()?.Trim() ?? string.Empty : string.Empty;
        }

        public static bool Matches(InstalledSoftware item, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return true;
            }
            query = query.Trim();

            return new[] { item.Name, item.Publisher, item.Version }.Any(value => value?.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0);
        }

        private bool IsRegistryError(Exception ex) => ex is SecurityException || ex is UnauthorizedAccessException || ex is IOException;
    }
}
