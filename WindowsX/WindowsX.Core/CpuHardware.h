#pragma once

#include "framework.h"

#pragma pack(push, 8)
struct CPU_ID_REGISTERS
{
	DWORD Eax;
	DWORD Ebx;
	DWORD Ecx;
	DWORD Edx;
};

struct CPU_HARDWARE_SNAPSHOT
{
	DWORD MaxBasicLeaf;
	DWORD MaxExtendedLeaf;
	DWORD ProcessorGroup;
	DWORD ProcessorNumber;
	CPU_ID_REGISTERS Vendor;
	CPU_ID_REGISTERS Signature;
	CPU_ID_REGISTERS Features;
	CPU_ID_REGISTERS Frequency;
	CPU_ID_REGISTERS ExtendedFeatures;
	CPU_ID_REGISTERS AddressSizes;
	CPU_ID_REGISTERS LegacyL1;
	CPU_ID_REGISTERS LegacyL2L3;
	CPU_ID_REGISTERS Brand[3];
	CPU_ID_REGISTERS Caches[32];
	DWORD CacheCount;
	DWORD Flags;
	ULONGLONG Xcr0;
};
#pragma pack(pop)

static_assert(sizeof(CPU_HARDWARE_SNAPSHOT) == 720, "CPU snapshot ABI mismatch");

extern "C" BOOL __cdecl ReadCpuHardwareSnapshot(CPU_HARDWARE_SNAPSHOT* snapshot, DWORD size);
