#include "CpuHardware.h"

#if defined(_M_X64)
extern "C" void CpuIdQueryAsm(DWORD leaf, DWORD subleaf, CPU_ID_REGISTERS* registers);
extern "C" ULONGLONG ReadXcr0Asm();
#elif defined(_M_IX86)
#include <intrin.h>
static void CpuIdQueryAsm(DWORD leaf, DWORD subleaf, CPU_ID_REGISTERS* registers)
{
	int values[4];
	__cpuidex(values, static_cast<int>(leaf), static_cast<int>(subleaf));
	memcpy(registers, values, sizeof(values));
}

static ULONGLONG ReadXcr0Asm()
{
	return _xgetbv(0);
}
#endif

#if defined(_M_X64) || defined(_M_IX86)
static void CollectCpuSnapshot(CPU_HARDWARE_SNAPSHOT* snapshot)
{
	CpuIdQueryAsm(0, 0, &snapshot->Vendor);
	snapshot->MaxBasicLeaf = snapshot->Vendor.Eax;
	CPU_ID_REGISTERS extended = {};
	CpuIdQueryAsm(0x80000000, 0, &extended);
	snapshot->MaxExtendedLeaf = extended.Eax >= 0x80000000 ? extended.Eax : 0;
	if (snapshot->MaxBasicLeaf >= 1)
	{
		CpuIdQueryAsm(1, 0, &snapshot->Signature);
		const DWORD xsaveMask = (1u << 26) | (1u << 27);
		if ((snapshot->Signature.Ecx & xsaveMask) == xsaveMask)
		{
			snapshot->Xcr0 = ReadXcr0Asm();
			snapshot->Flags |= 1;
		}
	}
	if (snapshot->MaxBasicLeaf >= 7)
		CpuIdQueryAsm(7, 0, &snapshot->Features);
	if (snapshot->MaxBasicLeaf >= 0x16)
		CpuIdQueryAsm(0x16, 0, &snapshot->Frequency);
	if (snapshot->MaxExtendedLeaf >= 0x80000001)
		CpuIdQueryAsm(0x80000001, 0, &snapshot->ExtendedFeatures);
	if (snapshot->MaxExtendedLeaf >= 0x80000004)
		for (DWORD index = 0; index < 3; index++)
			CpuIdQueryAsm(0x80000002 + index, 0, &snapshot->Brand[index]);
	if (snapshot->MaxExtendedLeaf >= 0x80000005)
		CpuIdQueryAsm(0x80000005, 0, &snapshot->LegacyL1);
	if (snapshot->MaxExtendedLeaf >= 0x80000006)
		CpuIdQueryAsm(0x80000006, 0, &snapshot->LegacyL2L3);
	if (snapshot->MaxExtendedLeaf >= 0x80000008)
		CpuIdQueryAsm(0x80000008, 0, &snapshot->AddressSizes);

	const bool intel = snapshot->Vendor.Ebx == 0x756e6547 && snapshot->Vendor.Edx == 0x49656e69 && snapshot->Vendor.Ecx == 0x6c65746e;
	const bool amd = snapshot->Vendor.Ebx == 0x68747541 && snapshot->Vendor.Edx == 0x69746e65 && snapshot->Vendor.Ecx == 0x444d4163;
	const bool hygon = snapshot->Vendor.Ebx == 0x6f677948 && snapshot->Vendor.Edx == 0x6e65476e && snapshot->Vendor.Ecx == 0x656e6975;
	DWORD cacheLeaf = 0;
	if ((amd || hygon) && snapshot->MaxExtendedLeaf >= 0x8000001d && (snapshot->ExtendedFeatures.Ecx & (1u << 22)) != 0)
		cacheLeaf = 0x8000001d;
	else if (intel && snapshot->MaxBasicLeaf >= 4)
		cacheLeaf = 4;
	if (cacheLeaf != 0)
	{
		for (DWORD index = 0; index < 32; index++)
		{
			CPU_ID_REGISTERS cache = {};
			CpuIdQueryAsm(cacheLeaf, index, &cache);
			if ((cache.Eax & 0x1f) == 0)
				break;
			snapshot->Caches[snapshot->CacheCount++] = cache;
		}
	}
}
#endif

BOOL __cdecl ReadCpuHardwareSnapshot(CPU_HARDWARE_SNAPSHOT* snapshot, DWORD size)
{
	if (snapshot == nullptr || size != sizeof(CPU_HARDWARE_SNAPSHOT))
	{
		SetLastError(ERROR_INVALID_PARAMETER);
		return FALSE;
	}
	ZeroMemory(snapshot, sizeof(*snapshot));
#if defined(_M_X64) || defined(_M_IX86)
	PROCESSOR_NUMBER processor = {};
	GetCurrentProcessorNumberEx(&processor);
	if (processor.Number >= sizeof(KAFFINITY) * 8)
	{
		SetLastError(ERROR_NOT_SUPPORTED);
		return FALSE;
	}
	GROUP_AFFINITY previous = {};
	GROUP_AFFINITY target = {};
	target.Group = processor.Group;
	target.Mask = static_cast<KAFFINITY>(1) << processor.Number;
	HANDLE thread = GetCurrentThread();
	if (!SetThreadGroupAffinity(thread, &target, &previous))
		return FALSE;

	BOOL success = FALSE;
	DWORD error = ERROR_SUCCESS;
	__try
	{
		__try
		{
			snapshot->ProcessorGroup = processor.Group;
			snapshot->ProcessorNumber = processor.Number;
			CollectCpuSnapshot(snapshot);
			success = TRUE;
		}
		__except (GetExceptionCode() == EXCEPTION_ILLEGAL_INSTRUCTION || GetExceptionCode() == EXCEPTION_PRIV_INSTRUCTION ? EXCEPTION_EXECUTE_HANDLER : EXCEPTION_CONTINUE_SEARCH)
		{
			error = ERROR_NOT_SUPPORTED;
		}
	}
	__finally
	{
		if (!SetThreadGroupAffinity(thread, &previous, nullptr))
		{
			error = GetLastError();
			success = FALSE;
		}
	}
	if (!success)
		ZeroMemory(snapshot, sizeof(*snapshot));
	SetLastError(error);
	return success;
#else
	SetLastError(ERROR_NOT_SUPPORTED);
	return FALSE;
#endif
}
