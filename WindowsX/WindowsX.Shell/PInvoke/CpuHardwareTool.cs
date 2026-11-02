using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace WindowsX.Shell.PInvoke
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct CpuIdRegisters
    {
        public uint Eax;
        public uint Ebx;
        public uint Ecx;
        public uint Edx;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct CpuHardwareSnapshot
    {
        public uint MaxBasicLeaf;
        public uint MaxExtendedLeaf;
        public uint ProcessorGroup;
        public uint ProcessorNumber;
        public CpuIdRegisters Vendor;
        public CpuIdRegisters Signature;
        public CpuIdRegisters Features;
        public CpuIdRegisters Frequency;
        public CpuIdRegisters ExtendedFeatures;
        public CpuIdRegisters AddressSizes;
        public CpuIdRegisters LegacyL1;
        public CpuIdRegisters LegacyL2L3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public CpuIdRegisters[] Brand;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public CpuIdRegisters[] Caches;
        public uint CacheCount;
        public uint Flags;
        public ulong Xcr0;

        public static CpuHardwareSnapshot Create() => new CpuHardwareSnapshot
        {
            Brand = new CpuIdRegisters[3], Caches = new CpuIdRegisters[32]
        };
    }

    public static class CpuHardwareTool
    {
        [DllImport("WindowsXCore.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.AssemblyDirectory | DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ReadCpuHardwareSnapshot([In, Out] ref CpuHardwareSnapshot snapshot, uint size);

        public static CpuHardwareSnapshot Read()
        {
            if (RuntimeInformation.ProcessArchitecture != Architecture.X64 && RuntimeInformation.ProcessArchitecture != Architecture.X86)
                throw new PlatformNotSupportedException("CPUID 汇编采集仅支持 x86/x64 进程。");
            var snapshot = CpuHardwareSnapshot.Create();
            if (!ReadCpuHardwareSnapshot(ref snapshot, (uint)Marshal.SizeOf<CpuHardwareSnapshot>()))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return snapshot;
        }
    }
}
