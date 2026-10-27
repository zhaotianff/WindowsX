using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using WindowsX.Shell.Model.SystemMgmt;
using WindowsX.Shell.Services.SystemMgmt.Contract;

namespace WindowsX.Shell.Services.SystemMgmt
{
    public interface ISystemWmiReader
    {
        List<Dictionary<string, object>> Query(string scope, string className, CancellationToken cancellationToken);
    }

    internal sealed class SystemWmiReader : ISystemWmiReader
    {
        public List<Dictionary<string, object>> Query(string scope, string className, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var managementScope = new ManagementScope(scope, new ConnectionOptions { Timeout = TimeSpan.FromSeconds(10) });
            using var searcher = new ManagementObjectSearcher(managementScope, new ObjectQuery("SELECT * FROM " + className),
                new EnumerationOptions { ReturnImmediately = true, Rewindable = false, Timeout = TimeSpan.FromSeconds(10) });
            using var results = searcher.Get();
            var rows = new List<Dictionary<string, object>>();
            foreach (ManagementObject item in results)
            {
                using (item)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    foreach (PropertyData property in item.Properties)
                        row[property.Name] = property.Value;
                    rows.Add(row);
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
            return rows;
        }
    }

    public class SystemHardwareInformation
    {
        public Dictionary<string, SystemInformation> Categories { get; } = new Dictionary<string, SystemInformation>();
        public List<string> Warnings { get; } = new List<string>();
    }

    public class SystemInformationService
    {
        public const string Unavailable = "未提供";
        private readonly ISystemWmiReader reader;
        private readonly ICpuHardwareReader cpuReader;

        public SystemInformationService() : this(new SystemWmiReader()) { }

        public SystemInformationService(ISystemWmiReader reader) : this(reader, new CpuHardwareService()) { }

        public SystemInformationService(ISystemWmiReader reader, ICpuHardwareReader cpuReader)
        {
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
            this.cpuReader = cpuReader ?? throw new ArgumentNullException(nameof(cpuReader));
        }

        public SystemHardwareInformation GetHardwareInformation(CancellationToken cancellationToken, IProgress<string> progress = null)
        {
            var result = new SystemHardwareInformation();
            Load("System", "操作系统", () => ReadSystem(cancellationToken));
            Load("MotherBoard", "主板", () => ReadMotherBoard(cancellationToken));
            Load("Cpu", "处理器", () => ReadCpu(cancellationToken, result.Warnings));
            Load("Memory", "内存", () => ReadMemory(cancellationToken));
            Load("Disk", "磁盘", () => ReadDisks(cancellationToken));
            Load("Graphics", "显卡", () => ReadGraphics(cancellationToken));
            Load("Monitor", "显示器", () => ReadMonitors(cancellationToken));
            Load("Network", "网络适配器", () => ReadNetwork(cancellationToken));
            return result;

            void Load(string key, string title, Func<SystemInformation> collect)
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress?.Report("正在读取" + title + "…");
                try
                {
                    var information = collect();
                    if (information.SystemInformationKeyValueList.Count == 0)
                    {
                        information = Message("未检测到设备，或设备未提供信息");
                        result.Warnings.Add(title + "：未检测到设备，或设备未提供信息");
                    }
                    result.Categories[key] = information;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    result.Categories[key] = Message("读取失败：" + ex.Message);
                    result.Warnings.Add(title + "：" + ex.Message);
                }
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public static SystemInformation Message(string message) => new SystemInformation
        {
            SystemInformationKeyValueList = new List<Dictionary<string, string>> { new Dictionary<string, string> { ["状态"] = message } }
        };

        private List<Dictionary<string, object>> Query(string name, CancellationToken token, string scope = @"root\cimv2") => reader.Query(scope, name, token);

        private SystemInformation ReadSystem(CancellationToken token)
        {
            var os = Query("Win32_OperatingSystem", token).FirstOrDefault();
            if (os == null)
                return Message("操作系统未提供 WMI 信息");
            var computer = Query("Win32_ComputerSystem", token).FirstOrDefault();
            var processors = Query("Win32_Processor", token);
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var build = Text(os, "BuildNumber");
            if (key?.GetValue("UBR") is int revision)
                build += "." + revision;
            return Single(new Dictionary<string, string>
            {
                ["系统"] = Text(os, "Caption"), ["版本"] = Value(key?.GetValue("DisplayVersion") ?? key?.GetValue("ReleaseId")),
                ["版本号"] = build, ["系统类型"] = Text(os, "OSArchitecture"),
                ["物理内存（系统报告）"] = FormatBytes(Get(computer, "TotalPhysicalMemory")),
                ["处理器"] = Join(processors.Select(x => Text(x, "Name"))),
                ["计算机名称"] = Environment.MachineName, ["系统型号"] = Text(computer, "Model"),
                ["系统制造商"] = Text(computer, "Manufacturer"), ["安装目录"] = Text(os, "WindowsDirectory"),
                ["安装日期"] = FormatWmiDate(Get(os, "InstallDate")),
                ["上次开机时间"] = FormatWmiDate(Get(os, "LastBootUpTime")),
                ["上次记录关机时间"] = ReadShutdownTime()
            });
        }

        private static string ReadShutdownTime()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Windows");
                if (key?.GetValue("ShutdownTime") is byte[] bytes && bytes.Length == 8)
                {
                    var fileTime = BitConverter.ToInt64(bytes, 0);
                    if (fileTime > 0)
                        return DateTime.FromFileTime(fileTime).ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            catch (Exception ex) when (ex is ArgumentException || ex is System.Security.SecurityException || ex is UnauthorizedAccessException) { }
            return Unavailable;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetFirmwareType(out uint firmwareType);

        private SystemInformation ReadMotherBoard(CancellationToken token)
        {
            var bios = Query("Win32_BIOS", token).FirstOrDefault();
            var computer = Query("Win32_ComputerSystem", token).FirstOrDefault();
            var firmware = GetFirmwareType(out var type) ? type switch { 1 => "Legacy BIOS", 2 => "UEFI", _ => Unavailable } : Unavailable;
            return Devices(Query("Win32_BaseBoard", token), "主板", x => Text(x, "Product"), x => new Dictionary<string, string>
            {
                ["制造商"] = Text(x, "Manufacturer"), ["型号"] = Text(x, "Product"), ["版本"] = Text(x, "Version"),
                ["序列号"] = Text(x, "SerialNumber"), ["BIOS模式"] = firmware,
                ["BIOS版本"] = Text(bios, "SMBIOSBIOSVersion"), ["BIOS日期"] = FormatWmiDate(Get(bios, "ReleaseDate")),
                ["唤醒方式"] = Number(Get(computer, "WakeUpType")) switch
                {
                    3 => "APM 定时器", 4 => "调制解调器", 5 => "网络", 6 => "电源开关", 7 => "PCI PME", 8 => "交流电恢复", _ => Unavailable
                }
            });
        }

        private SystemInformation ReadCpu(CancellationToken token, List<string> warnings)
        {
            var information = new SystemInformation();
            var native = cpuReader.Read(token);
            if (!string.IsNullOrEmpty(native.Warning))
                warnings.Add("处理器（CPUID）：" + native.Warning);
            if (native.IsAvailable)
            {
                information.SystemInformationTypeList.Add("CPUID - 当前逻辑处理器");
                information.SystemInformationKeyValueList.Add(native.Details);
            }
            string wmiError = null;
            try
            {
                var wmi = Devices(Query("Win32_Processor", token), "WMI 处理器", x => Text(x, "Name"), x => new Dictionary<string, string>
                {
                    ["数据来源"] = "Windows WMI（按处理器插槽）",
                    ["制造商"] = Text(x, "Manufacturer"), ["型号"] = Text(x, "Name"), ["插槽"] = Text(x, "SocketDesignation"),
                    ["最大时钟频率"] = Unit(Get(x, "MaxClockSpeed"), "MHz"), ["当前时钟频率"] = Unit(Get(x, "CurrentClockSpeed"), "MHz"),
                    ["核心数"] = Unit(Get(x, "NumberOfCores"), "核"), ["线程数"] = Unit(Get(x, "NumberOfLogicalProcessors"), "线程"),
                    ["L2缓存"] = Unit(Get(x, "L2CacheSize"), "KB"), ["L3缓存"] = Unit(Get(x, "L3CacheSize"), "KB")
                });
                information.SystemInformationTypeList.AddRange(wmi.SystemInformationTypeList);
                information.SystemInformationKeyValueList.AddRange(wmi.SystemInformationKeyValueList);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                wmiError = "WMI 读取失败：" + ex.Message;
                warnings.Add("处理器：" + wmiError);
            }
            token.ThrowIfCancellationRequested();
            if (information.SystemInformationKeyValueList.Count == 0 && (!string.IsNullOrEmpty(wmiError) || !string.IsNullOrEmpty(native.Warning)))
                return Message("读取失败：" + string.Join("；", new[] { native.Warning, wmiError }.Where(x => !string.IsNullOrEmpty(x))));
            return information;
        }

        private SystemInformation ReadMemory(CancellationToken token) => Devices(Query("Win32_PhysicalMemory", token), "内存", x => Text(x, "DeviceLocator"), x => new Dictionary<string, string>
        {
            ["插槽"] = Text(x, "DeviceLocator"), ["容量"] = FormatBytes(Get(x, "Capacity")), ["制造商"] = Text(x, "Manufacturer"),
            ["型号"] = Text(x, "PartNumber"), ["序列号"] = Text(x, "SerialNumber"),
            ["标称速率"] = Unit(Get(x, "Speed"), "MT/s"), ["配置速率"] = Unit(Get(x, "ConfiguredClockSpeed"), "MT/s"),
            ["配置电压"] = Unit(Get(x, "ConfiguredVoltage"), "mV")
        });

        private SystemInformation ReadDisks(CancellationToken token)
        {
            var disks = Query("Win32_DiskDrive", token);
            var partitions = Query("Win32_DiskDriveToDiskPartition", token);
            var volumes = Query("Win32_LogicalDiskToPartition", token);
            var logicalDisks = Query("Win32_LogicalDisk", token);
            return Devices(disks, "磁盘", x => Text(x, "Model"), x =>
            {
                var partitionIds = partitions.Where(p => ReferenceId(Get(p, "Antecedent")).Equals(Text(x, "DeviceID"), StringComparison.OrdinalIgnoreCase))
                    .Select(p => ReferenceId(Get(p, "Dependent"))).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var volumeIds = volumes.Where(v => partitionIds.Contains(ReferenceId(Get(v, "Antecedent"))))
                    .Select(v => ReferenceId(Get(v, "Dependent"))).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var mountedVolumes = logicalDisks.Where(v => volumeIds.Contains(Text(v, "DeviceID"))).ToList();
                return new Dictionary<string, string>
                {
                    ["型号"] = Text(x, "Model"), ["制造商"] = Text(x, "Manufacturer"), ["序列号"] = Text(x, "SerialNumber"),
                    ["接口"] = Text(x, "InterfaceType"), ["容量"] = FormatBytes(Get(x, "Size")),
                    ["固件版本"] = Text(x, "FirmwareRevision"), ["分区数"] = Text(x, "Partitions"),
                    ["卷及可用空间"] = mountedVolumes.Count == 0 ? "无已挂载卷或未提供" : string.Join(Environment.NewLine,
                        mountedVolumes.Select(v => Text(v, "DeviceID") + " " + FormatBytes(Get(v, "FreeSpace"), true))),
                    ["状态"] = Text(x, "Status")
                };
            });
        }

        private static string ReferenceId(object reference)
        {
            var text = reference?.ToString();
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            var start = text.IndexOf('"');
            var end = text.LastIndexOf('"');
            return start >= 0 && end > start ? text.Substring(start + 1, end - start - 1).Replace("\\\\", "\\").Replace("\\\"", "\"") : string.Empty;
        }

        private SystemInformation ReadGraphics(CancellationToken token) => Devices(Query("Win32_VideoController", token), "显卡", x => Text(x, "Name"), x => new Dictionary<string, string>
        {
            ["型号"] = Text(x, "Name"), ["制造商"] = Text(x, "AdapterCompatibility"),
            ["显存（WMI报告）"] = FormatBytes(Get(x, "AdapterRAM")), ["显存说明"] = "WMI 字段仅32位，大容量显存可能截断；不作为准确容量依据",
            ["驱动版本"] = Text(x, "DriverVersion"), ["驱动日期"] = FormatWmiDate(Get(x, "DriverDate")),
            ["当前分辨率"] = Resolution(x, "CurrentHorizontalResolution", "CurrentVerticalResolution"),
            ["当前刷新率"] = Unit(Get(x, "CurrentRefreshRate"), "Hz"), ["状态"] = Text(x, "Status")
        });

        private SystemInformation ReadMonitors(CancellationToken token)
        {
            var monitors = Query("WmiMonitorID", token, @"root\wmi");
            var sizes = Query("WmiMonitorBasicDisplayParams", token, @"root\wmi");
            var desktops = Query("Win32_DesktopMonitor", token);
            return Devices(monitors, "显示器", x => DecodeEdid(Get(x, "UserFriendlyName")), x =>
            {
                var instance = Text(x, "InstanceName");
                var size = sizes.FirstOrDefault(s => Text(s, "InstanceName").Equals(instance, StringComparison.OrdinalIgnoreCase));
                var suffix = instance.LastIndexOf('_');
                var deviceId = suffix > 0 ? instance.Substring(0, suffix) : instance;
                var desktop = desktops.FirstOrDefault(d => Text(d, "PNPDeviceID").Equals(deviceId, StringComparison.OrdinalIgnoreCase));
                var width = Number(Get(size, "MaxHorizontalImageSize"));
                var height = Number(Get(size, "MaxVerticalImageSize"));
                return new Dictionary<string, string>
                {
                    ["名称"] = DecodeEdid(Get(x, "UserFriendlyName")), ["制造商代码"] = DecodeEdid(Get(x, "ManufacturerName")),
                    ["序列号"] = DecodeEdid(Get(x, "SerialNumberID")),
                    ["分辨率（WMI报告）"] = Resolution(desktop, "ScreenWidth", "ScreenHeight"),
                    ["屏幕尺寸（EDID）"] = width > 0 && height > 0 ? $"{Math.Sqrt(width * width + height * height) / 2.54:0.0} 英寸（{width} × {height} cm）" : Unavailable,
                    ["生产年份"] = Unit(Get(x, "YearOfManufacture"), "年"), ["生产周"] = Unit(Get(x, "WeekOfManufacture"), "周"),
                    ["活动状态"] = Get(x, "Active") is bool active ? (active ? "活动" : "非活动") : Unavailable
                };
            });
        }

        private static SystemInformation ReadNetwork(CancellationToken token)
        {
            var information = new SystemInformation();
            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                token.ThrowIfCancellationRequested();
                var row = new Dictionary<string, string>
                {
                    ["名称"] = adapter.Name, ["描述"] = adapter.Description,
                    ["网卡类型"] = adapter.NetworkInterfaceType.ToString(), ["连接状态"] = adapter.OperationalStatus.ToString(),
                    ["MAC地址"] = FormatMacAddress(adapter.GetPhysicalAddress().GetAddressBytes())
                };
                try
                {
                    row["连接速度"] = adapter.Speed > 0 ? $"{adapter.Speed / 1000000d:0.##} Mbit/秒" : Unavailable;
                    var properties = adapter.GetIPProperties();
                    row["IP地址"] = Join(properties.UnicastAddresses.Select(a => a.Address.ToString()));
                    row["IPv4子网掩码"] = Join(properties.UnicastAddresses.Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork).Select(a => a.IPv4Mask.ToString()));
                    row["网关"] = Join(properties.GatewayAddresses.Select(a => a.Address.ToString()));
                    row["DNS"] = Join(properties.DnsAddresses.Select(a => a.ToString()));
                }
                catch (NetworkInformationException ex)
                {
                    row["状态"] = "部分信息读取失败：" + ex.Message;
                }
                information.SystemInformationTypeList.Add(adapter.Name);
                information.SystemInformationKeyValueList.Add(row);
            }
            return information;
        }

        private static SystemInformation Devices(IEnumerable<Dictionary<string, object>> rows, string category,
            Func<Dictionary<string, object>, string> name, Func<Dictionary<string, object>, Dictionary<string, string>> map)
        {
            var information = new SystemInformation();
            foreach (var row in rows)
            {
                information.SystemInformationTypeList.Add($"{category}{information.SystemInformationTypeList.Count + 1} - {name(row)}");
                information.SystemInformationKeyValueList.Add(map(row));
            }
            return information;
        }

        private static SystemInformation Single(Dictionary<string, string> row) => new SystemInformation
        {
            SystemInformationKeyValueList = new List<Dictionary<string, string>> { row }
        };

        private static object Get(Dictionary<string, object> row, string key) => row != null && row.TryGetValue(key, out var value) ? value : null;
        private static string Text(Dictionary<string, object> row, string key) => Value(Get(row, key));
        private static string Value(object value) => string.IsNullOrWhiteSpace(value?.ToString()) ? Unavailable : value.ToString().Trim();
        private static double Number(object value) => double.TryParse(value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var number) ? number : 0;
        private static string Unit(object value, string unit) => Number(value) > 0 ? Value(value) + " " + unit : Unavailable;
        private static string Join(IEnumerable<string> values, string separator = ", ")
        {
            var text = string.Join(separator, values.Where(v => !string.IsNullOrWhiteSpace(v)).Distinct());
            return string.IsNullOrEmpty(text) ? Unavailable : text;
        }
        private static string Resolution(Dictionary<string, object> row, string width, string height) =>
            Number(Get(row, width)) > 0 && Number(Get(row, height)) > 0 ? $"{Text(row, width)} × {Text(row, height)}" : Unavailable;

        public static string FormatBytes(object value, bool allowZero = false)
        {
            if (value == null || !double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var bytes) ||
                double.IsNaN(bytes) || double.IsInfinity(bytes) || bytes < 0 || (!allowZero && bytes == 0))
                return Unavailable;
            var units = new[] { "B", "KB", "MB", "GB", "TB", "PB" };
            var index = 0;
            while (bytes >= 1024 && index < units.Length - 1)
            {
                bytes /= 1024;
                index++;
            }
            return bytes.ToString("0.##", CultureInfo.CurrentCulture) + " " + units[index];
        }

        public static string FormatWmiDate(object value)
        {
            var text = value?.ToString();
            if (string.IsNullOrWhiteSpace(text) || text.Contains('*'))
                return Unavailable;
            try { return ManagementDateTimeConverter.ToDateTime(text).ToString("yyyy-MM-dd HH:mm:ss"); }
            catch (ArgumentException) { return Unavailable; }
        }

        public static string FormatMacAddress(byte[] bytes) => bytes == null || bytes.Length == 0 ? Unavailable :
            string.Join(":", bytes.Select(b => b.ToString("X2")));

        public static string DecodeEdid(object value)
        {
            if (value is not ushort[] characters)
                return Unavailable;
            return Value(new string(characters.TakeWhile(c => c != 0).Select(c => (char)c).ToArray()));
        }
    }
}
