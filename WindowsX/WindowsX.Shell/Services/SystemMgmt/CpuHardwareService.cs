using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using WindowsX.Shell.Model.SystemMgmt;
using WindowsX.Shell.PInvoke;
using WindowsX.Shell.Services.SystemMgmt.Contracts;

namespace WindowsX.Shell.Services.SystemMgmt
{
    public class CpuHardwareService : ICpuHardwareReader
    {
        public CpuHardwareService()
        { 

        }

        public CpuHardwareInformation Read(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var snapshot = CpuHardwareTool.Read();
                cancellationToken.ThrowIfCancellationRequested();
                return Decode(snapshot);
            }
            catch (Exception ex) when (ex is DllNotFoundException || ex is EntryPointNotFoundException ||
                ex is BadImageFormatException || ex is Win32Exception || ex is PlatformNotSupportedException)
            {
                return new CpuHardwareInformation { Warning = "用户态汇编采集不可用：" + ex.Message };
            }
        }

        public CpuHardwareInformation Decode(CpuHardwareSnapshot snapshot)
        {
            var result = new CpuHardwareInformation();

            if (snapshot.Brand == null || snapshot.Brand.Length != 3 || snapshot.Caches == null ||
                snapshot.Caches.Length != 32 || snapshot.CacheCount > 32 || snapshot.MaxBasicLeaf < 1)
            {
                result.Warning = "CPUID 快照不完整或格式不匹配";
                return result;
            }

            var vendor = DecodeText(new[] { snapshot.Vendor.Ebx, snapshot.Vendor.Edx, snapshot.Vendor.Ecx });

            if (string.IsNullOrWhiteSpace(vendor))
            {
                result.Warning = "处理器未提供有效的 CPUID 厂商信息";
                return result;
            }

            var brand = snapshot.MaxExtendedLeaf >= 0x80000004
                ? DecodeText(snapshot.Brand.SelectMany(r => new[] { r.Eax, r.Ebx, r.Ecx, r.Edx })) : string.Empty;

            var row = result.Details;
            row["数据来源"] = "用户态汇编 CPUID / XGETBV";
            row["权限要求"] = "普通用户；无需管理员权限或驱动";
            row["制造商"] = vendor switch { "GenuineIntel" => "Intel", "AuthenticAMD" => "AMD", "HygonGenuine" => "Hygon", _ => vendor };
            row["厂商标识"] = vendor;
            row["型号"] = string.IsNullOrWhiteSpace(brand) ? SystemInformationService.Unavailable : brand;
            row["采样逻辑处理器"] = $"处理器组 {snapshot.ProcessorGroup} / 编号 {snapshot.ProcessorNumber}";
            row["采样范围"] = "当前逻辑处理器；缓存按单个缓存实例报告，不是整机合计。多插槽/混合架构请同时查看 WMI 条目。";

            var signature = snapshot.Signature.Eax;
            var baseFamily = (signature >> 8) & 0xf;
            var family = baseFamily == 0xf ? baseFamily + ((signature >> 20) & 0xff) : baseFamily;
            var model = (signature >> 4) & 0xf;
            if (baseFamily == 6 || baseFamily == 0xf)
                model |= ((signature >> 16) & 0xf) << 4;
            row["Family / Model / Stepping"] = $"{family} / {model} / {signature & 0xf}";
            row["Hypervisor"] = Bit(snapshot.Signature.Ecx, 31) ? "检测到（部分硬件特性可能被屏蔽）" : "未报告";
            
            if (snapshot.MaxBasicLeaf >= 7 && vendor == "GenuineIntel")
            {
                row["混合架构标志"] = Bit(snapshot.Features.Edx, 15) ? "是" : "否";
            }

            var ecx = snapshot.Signature.Ecx;
            var edx = snapshot.Signature.Edx;
            var features = snapshot.MaxBasicLeaf >= 7 ? snapshot.Features : default;
            var hardware = DecodeISAInfo(snapshot, vendor, ecx, edx, features);

            row["硬件支持的指令集"] =  hardware.Count == 0 ? "未报告" : string.Join(", ", hardware); ;
            row["系统已启用的 AVX 系列"] = DecodeAVXInfo(ecx, snapshot, features, hardware);

            if (snapshot.MaxBasicLeaf >= 0x16 && vendor == "GenuineIntel")
            {
                CheckAndAddFrequency(row, "基准频率（CPUID）", snapshot.Frequency.Eax & 0xffff);
                CheckAndAddFrequency(row, "最大频率（CPUID）", snapshot.Frequency.Ebx & 0xffff);
                CheckAndAddFrequency(row, "参考频率（CPUID）", snapshot.Frequency.Ecx & 0xffff);
            }
            if (snapshot.MaxExtendedLeaf >= 0x80000008)
            {
                var physical = snapshot.AddressSizes.Eax & 0xff;
                var linear = (snapshot.AddressSizes.Eax >> 8) & 0xff;
                if (physical > 0 && physical <= 64) row["物理地址位数"] = physical + " 位";
                if (linear > 0 && linear <= 64) row["线性地址位数"] = linear + " 位";
            }
            DecodeCachesInfo(snapshot, result, vendor);
            return result;
        }

        private void CheckAndAddFrequency(Dictionary<string,string> row, string name, uint frequency)
        {
            if (frequency > 0)
            {
                row[name] = frequency + " MHz（非实时测量）";
            }
        }

        private List<string> DecodeISAInfo(CpuHardwareSnapshot snapshot,string vendor,uint ecx,uint edx,CpuIdRegisters features)
        {
            var hardware = new List<string>();
            void Add(bool supported, string feature) { if (supported) hardware.Add(feature); }
          
            Add(Bit(edx, 23), "MMX");
            Add(Bit(edx, 25), "SSE");
            Add(Bit(edx, 26), "SSE2");
            Add(Bit(ecx, 0), "SSE3");
            Add(Bit(ecx, 9), "SSSE3");
            Add(Bit(ecx, 19), "SSE4.1");
            Add(Bit(ecx, 20), "SSE4.2");
            Add(Bit(ecx, 1), "PCLMULQDQ");
            Add(Bit(ecx, 23), "POPCNT");
            Add(Bit(ecx, 25), "AES");
            Add(Bit(ecx, 30), "RDRAND");
            Add(Bit(ecx, 28), "AVX");
            Add(Bit(ecx, 12), "FMA");
            Add(Bit(ecx, 29), "F16C");
          
            Add(Bit(features.Ebx, 3), "BMI1");
            Add(Bit(features.Ebx, 8), "BMI2");
            Add(Bit(features.Ebx, 18), "RDSEED");
            Add(Bit(features.Ebx, 19), "ADX");
            Add(Bit(features.Ebx, 29), "SHA");
            Add(Bit(features.Ebx, 5), "AVX2");
            Add(Bit(features.Ebx, 16), "AVX512F");
            Add(Bit(features.Ebx, 17), "AVX512DQ");
            Add(Bit(features.Ebx, 21), "AVX512IFMA");
            Add(Bit(features.Ebx, 28), "AVX512CD");
            Add(Bit(features.Ebx, 30), "AVX512BW");
            Add(Bit(features.Ebx, 31), "AVX512VL");
            if (snapshot.MaxExtendedLeaf >= 0x80000001)
            {
                Add(Bit(snapshot.ExtendedFeatures.Edx, 29), "x86-64");
                Add(Bit(snapshot.ExtendedFeatures.Edx, 20), "NX");
                Add(Bit(snapshot.ExtendedFeatures.Ecx, 5), "LZCNT");
                if (vendor == "AuthenticAMD" || vendor == "HygonGenuine")
                    Add(Bit(snapshot.ExtendedFeatures.Ecx, 6), "SSE4a");
            }
            return hardware;
        }

        private string DecodeAVXInfo(uint ecx,CpuHardwareSnapshot snapshot,CpuIdRegisters features,List<string> hardware)
        {
            var avxEnabled = Bit(ecx, 28) && Bit(ecx, 26) && Bit(ecx, 27) && (snapshot.Flags & 1) != 0 && (snapshot.Xcr0 & 6) == 6;
            var enabled = new List<string>();
            if (avxEnabled)
            {
                enabled.Add("AVX");
                if (Bit(ecx, 12)) enabled.Add("FMA");
                if (Bit(ecx, 29)) enabled.Add("F16C");
                if (Bit(features.Ebx, 5)) enabled.Add("AVX2");
                if (Bit(features.Ebx, 16) && (snapshot.Xcr0 & 0xe6) == 0xe6)
                    enabled.AddRange(hardware.Where(f => f.StartsWith("AVX512", StringComparison.Ordinal)));
            }

            return enabled.Count == 0 ? "未启用或未报告" : string.Join(", ", enabled);
        }

        private void DecodeCachesInfo(CpuHardwareSnapshot snapshot, CpuHardwareInformation result, string vendor)
        {
            var row = result.Details;
            var found = false;

            for (var index = 0; index < snapshot.CacheCount; index++)
            {
                var cache = snapshot.Caches[index];
                var type = cache.Eax & 0x1f;
                var level = (cache.Eax >> 5) & 7;
                if (type < 1 || type > 3 || level == 0)
                    continue;
                var lineSize = (cache.Ebx & 0xfff) + 1UL;
                var partitions = ((cache.Ebx >> 12) & 0x3ff) + 1UL;
                var ways = ((cache.Ebx >> 22) & 0x3ff) + 1UL;
                var sets = cache.Ecx + 1UL;
                ulong bytes;
                try { bytes = checked(lineSize * partitions * ways * sets); }
                catch (OverflowException)
                {
                    result.Warning = "CPUID 返回的部分缓存大小超出有效范围，已跳过";
                    continue;
                }
                var kind = type == 1 ? "数据" : type == 2 ? "指令" : "统一";
                var sharing = ((cache.Eax >> 14) & 0xfff) + 1;
                row[$"L{level} {kind}缓存（CPUID #{index + 1}）"] = $"{SystemInformationService.FormatBytes(bytes)}；缓存行 {lineSize} B；最多 {sharing} 个逻辑处理器共享";
                found = true;
            }

            if (!found && (vendor == "AuthenticAMD" || vendor == "HygonGenuine"))
            {
                if (snapshot.MaxExtendedLeaf >= 0x80000005)
                {
                    CheckAndAddLegency(row,"L1 数据缓存（扩展 CPUID）", (snapshot.LegacyL1.Ecx >> 24) * 1024UL,ref found);
                    CheckAndAddLegency(row, "L1 指令缓存（扩展 CPUID）", (snapshot.LegacyL1.Edx >> 24) * 1024UL, ref found);
                }
                if (snapshot.MaxExtendedLeaf >= 0x80000006)
                {
                    CheckAndAddLegency(row, "L2 缓存（扩展 CPUID）", (snapshot.LegacyL2L3.Ecx >> 16) * 1024UL, ref found);
                    CheckAndAddLegency(row, "L3 缓存（扩展 CPUID）", (snapshot.LegacyL2L3.Edx >> 18) * 512UL * 1024, ref found);
                }
            }

            if (!found)
            {
                row["缓存（CPUID）"] = SystemInformationService.Unavailable;
            }  
        }

        private void CheckAndAddLegency(Dictionary<string, string> row, string label, ulong bytes, ref bool found)
        {
            if (bytes == 0)
            {
                return;
            }

            row[label] = SystemInformationService.FormatBytes(bytes);
            found = true;
        }

        private bool Bit(uint value, int bit) => (value & (1u << bit)) != 0;

        private string DecodeText(IEnumerable<uint> values)
        {
            return Encoding.ASCII.GetString(values.SelectMany(BitConverter.GetBytes).ToArray()).TrimEnd('\0').Trim();
        }
    }
}
