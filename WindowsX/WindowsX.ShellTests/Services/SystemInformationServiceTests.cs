using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using WindowsX.Shell.Model.SystemMgmt;
using WindowsX.Shell.Services.SystemMgmt;
using WindowsX.Shell.Services.SystemMgmt.Contract;

namespace WindowsX.ShellTests.Services
{
    [TestClass]
    public class SystemInformationServiceTests
    {
        private sealed class EmptyCpuReader : ICpuHardwareReader
        {
            public CpuHardwareInformation Read(CancellationToken cancellationToken) => new CpuHardwareInformation();
        }

        private static SystemInformationService CreateService(ISystemWmiReader reader) => new SystemInformationService(reader, new EmptyCpuReader());

        private sealed class FakeWmiReader : ISystemWmiReader
        {
            public Dictionary<string, List<Dictionary<string, object>>> Rows { get; } = new();
            public string? FailingClass { get; set; }
            public int Calls { get; private set; }

            public List<Dictionary<string, object>> Query(string scope, string className, CancellationToken cancellationToken)
            {
                Calls++;
                cancellationToken.ThrowIfCancellationRequested();
                if (className == FailingClass)
                    throw new UnauthorizedAccessException("测试访问被拒绝");
                return Rows.TryGetValue(className, out var rows) ? rows : new();
            }
        }

        [DataTestMethod]
        [DataRow(null, "未提供")]
        [DataRow("0", "未提供")]
        [DataRow("-1", "未提供")]
        [DataRow("invalid", "未提供")]
        [DataRow("NaN", "未提供")]
        [DataRow("1024", "1 KB")]
        [DataRow("8589934592", "8 GB")]
        [DataRow("1099511627776", "1 TB")]
        public void FormatBytesHandlesMissingAndLargeValues(string? value, string expected)
        {
            Assert.AreEqual(expected, SystemInformationService.FormatBytes(value!));
        }

        [TestMethod]
        public void FormatBytesPreservesZeroFreeSpace()
        {
            Assert.AreEqual("0 B", SystemInformationService.FormatBytes(0, true));
        }

        [TestMethod]
        public void WmiDatesHandleInvalidAndUnknownValues()
        {
            Assert.AreEqual("未提供", SystemInformationService.FormatWmiDate(null!));
            Assert.AreEqual("未提供", SystemInformationService.FormatWmiDate("not-a-date"));
            Assert.AreEqual("未提供", SystemInformationService.FormatWmiDate("2024**********.******+***"));
            Assert.IsTrue(DateTime.TryParseExact(SystemInformationService.FormatWmiDate("20240102030405.000000+000"),
                "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out _));
        }

        [TestMethod]
        public void EdidTextStopsAtNullAndHandlesMissingValues()
        {
            Assert.AreEqual("AB", SystemInformationService.DecodeEdid(new ushort[] { 65, 66, 0, 67 }));
            Assert.AreEqual("未提供", SystemInformationService.DecodeEdid(Array.Empty<ushort>()));
            Assert.AreEqual("未提供", SystemInformationService.DecodeEdid(null!));
        }

        [TestMethod]
        public void MacAddressPreservesRepeatedAndZeroBytes()
        {
            Assert.AreEqual("00:AA:AA:00:BB:BB", SystemInformationService.FormatMacAddress(new byte[] { 0, 170, 170, 0, 187, 187 }));
            Assert.AreEqual("未提供", SystemInformationService.FormatMacAddress(Array.Empty<byte>()));
        }

        [TestMethod]
        public void HardwareMapsCpuAndMultipleMemoryDevicesWithoutDemoValues()
        {
            var reader = new FakeWmiReader();
            reader.Rows["Win32_Processor"] = new()
            {
                new() { ["Name"] = "Test CPU", ["NumberOfCores"] = 8, ["NumberOfLogicalProcessors"] = 16 }
            };
            reader.Rows["Win32_PhysicalMemory"] = new()
            {
                new() { ["DeviceLocator"] = "DIMM A", ["Capacity"] = 8589934592UL },
                new() { ["DeviceLocator"] = "DIMM B", ["Capacity"] = 17179869184UL }
            };
            var result = CreateService(reader).GetHardwareInformation(CancellationToken.None);
            var cpu = result.Categories["Cpu"].SystemInformationKeyValueList.Single();
            Assert.AreEqual("Test CPU", cpu["型号"]);
            Assert.AreEqual("8 核", cpu["核心数"]);
            Assert.AreEqual("16 线程", cpu["线程数"]);
            Assert.AreEqual("未提供", cpu["L3缓存"]);
            var memory = result.Categories["Memory"];
            Assert.AreEqual(2, memory.SystemInformationTypeList.Count);
            Assert.AreEqual("8 GB", memory.SystemInformationKeyValueList[0]["容量"]);
            Assert.AreEqual("16 GB", memory.SystemInformationKeyValueList[1]["容量"]);
            Assert.AreEqual(8, result.Categories.Count);
        }

        [TestMethod]
        public void FailedCategoryDoesNotSuppressOtherCategories()
        {
            var reader = new FakeWmiReader { FailingClass = "Win32_Processor" };
            reader.Rows["Win32_PhysicalMemory"] = new() { new() { ["Capacity"] = 8589934592UL } };
            var result = CreateService(reader).GetHardwareInformation(CancellationToken.None);
            StringAssert.Contains(result.Categories["Cpu"].SystemInformationKeyValueList.Single()["状态"], "读取失败");
            Assert.AreEqual("8 GB", result.Categories["Memory"].SystemInformationKeyValueList.Single()["容量"]);
            Assert.IsTrue(result.Warnings.Any(w => w.Contains("处理器")));
        }

        [TestMethod]
        public void MissingDevicesProduceExplicitStatusInsteadOfExamples()
        {
            var result = CreateService(new FakeWmiReader()).GetHardwareInformation(CancellationToken.None);
            StringAssert.Contains(result.Categories["Graphics"].SystemInformationKeyValueList.Single()["状态"], "未检测到");
            Assert.AreEqual(0, result.Categories["Graphics"].SystemInformationTypeList.Count);
        }

        [TestMethod]
        public void CancelledHardwareLoadDoesNotQueryWmi()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var reader = new FakeWmiReader();
            Assert.ThrowsException<OperationCanceledException>(() => CreateService(reader).GetHardwareInformation(cancellation.Token));
            Assert.AreEqual(0, reader.Calls);
        }

        [TestMethod]
        public void DiskFreeSpaceIsMappedToItsOwnVolumes()
        {
            var reader = new FakeWmiReader();
            reader.Rows["Win32_DiskDrive"] = new() { new() { ["DeviceID"] = @"\\.\PHYSICALDRIVE0", ["Model"] = "Test disk", ["Size"] = 1073741824UL } };
            reader.Rows["Win32_DiskDriveToDiskPartition"] = new()
            {
                new() { ["Antecedent"] = "Win32_DiskDrive.DeviceID=\"\\\\\\\\.\\\\PHYSICALDRIVE0\"", ["Dependent"] = "Win32_DiskPartition.DeviceID=\"Disk #0, Partition #0\"" }
            };
            reader.Rows["Win32_LogicalDiskToPartition"] = new()
            {
                new() { ["Antecedent"] = "Win32_DiskPartition.DeviceID=\"Disk #0, Partition #0\"", ["Dependent"] = "Win32_LogicalDisk.DeviceID=\"C:\"" }
            };
            reader.Rows["Win32_LogicalDisk"] = new()
            {
                new() { ["DeviceID"] = "C:", ["FreeSpace"] = 0UL },
                new() { ["DeviceID"] = "D:", ["FreeSpace"] = 999UL }
            };
            var result = CreateService(reader).GetHardwareInformation(CancellationToken.None);
            Assert.AreEqual("C: 0 B", result.Categories["Disk"].SystemInformationKeyValueList.Single()["卷及可用空间"]);
        }

        [TestMethod]
        public void SoftwareMapsRegisteredValuesAndNormalizesDate()
        {
            var item = InstalledSoftwareService.CreateSoftware(new Dictionary<string, object>
            {
                ["DisplayName"] = " Test Tool ", ["DisplayVersion"] = "2.3", ["Publisher"] = "Test Publisher",
                ["InstallDate"] = "20240229", ["InstallLocation"] = @"C:\Tools"
            }, "当前用户", "64位");
            Assert.IsNotNull(item);
            Assert.AreEqual("Test Tool", item.Name);
            Assert.AreEqual("2024-02-29", item.InstallDate);
            Assert.AreEqual("当前用户", item.Scope);
            Assert.AreEqual(@"C:\Tools", item.InstallLocation);
        }

        [TestMethod]
        public void SoftwareExcludesUnnamedComponentsAndChildUpdates()
        {
            Assert.IsNull(InstalledSoftwareService.CreateSoftware(new Dictionary<string, object>(), "本机", "32位"));
            Assert.IsNull(InstalledSoftwareService.CreateSoftware(new Dictionary<string, object> { ["DisplayName"] = "Hidden", ["SystemComponent"] = 1 }, "本机", "32位"));
            Assert.IsNull(InstalledSoftwareService.CreateSoftware(new Dictionary<string, object> { ["DisplayName"] = "Update", ["ParentKeyName"] = "Parent" }, "本机", "32位"));
            Assert.IsNull(InstalledSoftwareService.CreateSoftware(new Dictionary<string, object> { ["DisplayName"] = "Patch", ["ReleaseType"] = "Security Update" }, "本机", "32位"));
        }

        [TestMethod]
        public void SoftwareKeepsUnrecognizedDateWithoutInventingOne()
        {
            var item = InstalledSoftwareService.CreateSoftware(new Dictionary<string, object> { ["DisplayName"] = "Tool", ["InstallDate"] = "unknown" }, "本机", "64位");
            Assert.AreEqual("unknown", item.InstallDate);
            Assert.AreEqual(string.Empty, item.Version);
        }

        [TestMethod]
        public void SoftwareSearchMatchesNamePublisherAndVersion()
        {
            var item = InstalledSoftwareService.CreateSoftware(new Dictionary<string, object>
            {
                ["DisplayName"] = "Editor", ["Publisher"] = "Example", ["DisplayVersion"] = "5.2"
            }, "本机", "64位");
            Assert.IsTrue(InstalledSoftwareService.Matches(item, " EDIT "));
            Assert.IsTrue(InstalledSoftwareService.Matches(item, "example"));
            Assert.IsTrue(InstalledSoftwareService.Matches(item, "5.2"));
            Assert.IsTrue(InstalledSoftwareService.Matches(item, "  "));
            Assert.IsFalse(InstalledSoftwareService.Matches(item, "missing"));
        }

        [TestMethod]
        public void CancelledSoftwareLoadDoesNotEnumerateRegistry()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Assert.ThrowsException<OperationCanceledException>(() => new InstalledSoftwareService().GetInstalledSoftware(cancellation.Token));
        }
    }
}
