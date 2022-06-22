#include"SystemTool.h"
#include<VersionHelpers.h>
#include<functional>
#include<Winternl.h>

SYSTEMTIME GetUserLoginTime()
{
	SYSTEMTIME stSys{};

	HINSTANCE hInstance = LoadLibrary(L"ntdll.dll");
	if (hInstance == NULL)
		return stSys;

	funNtQuerySystemInformation NtQuerySystemInformation = (funNtQuerySystemInformation)GetProcAddress(hInstance,"NtQuerySystemInformation");

	if (NtQuerySystemInformation)
	{
		SYSTEM_TIME_INFORMATION tInfo{};
		auto ret = NtQuerySystemInformation(SystemTimeOfDayInformation, &tInfo, sizeof(tInfo), 0);
		FILETIME stFile = *(FILETIME*)&tInfo.liKeBootTime;

		FileTimeToLocalFileTime(&stFile, &stFile);
		FileTimeToSystemTime(&stFile, &stSys);

		FreeLibrary(hInstance);
	}
	
	return stSys;
}

void GetVersionNumbers(DWORD* pdwMajor, DWORD* pdwMinor, DWORD* pdwBuildNumber)
{
	HINSTANCE hInstance = LoadLibrary(L"ntdll.dll");
	if (hInstance == NULL)
		return;

	auto RtlGetNtVersionNumbers = (funRtlGetNtVersionNumbers)GetProcAddress(hInstance, "RtlGetNtVersionNumbers");
	if (RtlGetNtVersionNumbers)
	{
		RtlGetNtVersionNumbers(pdwMajor, pdwMinor, pdwBuildNumber);
		*pdwBuildNumber &= 0xffff;
	}
}

BOOL IsWindows10()
{
	//first windows 10 version =>Version 1507 (RTM) (OS build 10240)
	DWORD dwMajor, dwMinor, dwBuildNumber;
	GetVersionNumbers(&dwMajor, &dwMinor, &dwBuildNumber);
	return dwMajor == 10 && dwBuildNumber >= 10240 && dwBuildNumber < 22000;
}

BOOL IsWindows10OrHigher()
{
	DWORD dwMajor, dwMinor, dwBuildNumber;
	GetVersionNumbers(&dwMajor, &dwMinor, &dwBuildNumber);
	return dwMajor == 10 && dwBuildNumber >= 10240;
}

BOOL IsWindows11()
{
	//no windows 11 SDK
	

	//Windows 10 2004 =>	10.0.19041
	//Windows 11 =>         10.0.22000
	DWORD dwMajor, dwMinor, dwBuildNumber;
	GetVersionNumbers(&dwMajor, &dwMinor, &dwBuildNumber);
	return dwMajor == 10 && dwBuildNumber >= 22000;
}

BOOL RegisterFastRunHotKey(HWND hwnd)
{
	RAWINPUTDEVICE rawInputDevice{};
	rawInputDevice.usUsagePage = 0x01;
	rawInputDevice.usUsage = 0x06;
	rawInputDevice.dwFlags = RIDEV_INPUTSINK;
	rawInputDevice.hwndTarget = hwnd;

	BOOL bRet = RegisterRawInputDevices(&rawInputDevice, 1, sizeof(rawInputDevice));

	return bRet;
}

int GetCPUKernelCount()
{
	SYSTEM_INFO sinfo{};
	GetSystemInfo(&sinfo);
	return sinfo.dwNumberOfProcessors;
}

BOOL AdjustPrivilege()
{
	HANDLE hToken = NULL;
	LUID uID; //Describes a local identifier for an adapter.
	TOKEN_PRIVILEGES privValues;

	BOOL bOpen = OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken);
	if (!bOpen)
		return FALSE;

	BOOL bLooked = LookupPrivilegeValue(NULL, SE_DEBUG_NAME ,&uID);

	if (!bLooked)
	{
		CloseHandle(hToken);
		hToken = NULL;
	}

	privValues.PrivilegeCount = 1;
	privValues.Privileges[0].Luid = uID;
	privValues.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
	//The AdjustTokenPrivileges function enables or disables privileges in the specified access token. 
	//Enabling or disabling privileges in an access token requires TOKEN_ADJUST_PRIVILEGES access.
	BOOL bAdjusted = AdjustTokenPrivileges(hToken, FALSE, &privValues, sizeof(privValues), NULL, NULL);

	if (!bAdjusted)
		return FALSE;

	CloseHandle(hToken);
	hToken = NULL;
	return TRUE;
}

BOOL ForceDeleteFile(LPTSTR lpszFilePah)
{
	BOOL bResult = FALSE;
	return bResult;

	HINSTANCE hInstance = LoadLibrary(L"ntdll.dll");
	if (hInstance == NULL)
		return bResult;

	funNtQuerySystemInformation NtQuerySystemInformation = (funNtQuerySystemInformation)GetProcAddress(hInstance, "NtQuerySystemInformation");

	if (NtQuerySystemInformation)
	{
		int nCount = 0;
		SYSTEM_PROCESS_INFORMATION* pSystemProcess = NULL;
		DWORD dwBufferSize = 0xFFFFF;
		pSystemProcess = (SYSTEM_PROCESS_INFORMATION*)malloc(dwBufferSize);
		DWORD dwRet = NtQuerySystemInformation(SystemProcessInformation, pSystemProcess, dwBufferSize, &dwBufferSize);

		if (dwRet != 0)
		{
			return bResult;
		}

		while (pSystemProcess->NextEntryOffset)
		{
			if (0 != pSystemProcess->UniqueProcessId)
			{
				//pSystemProcess->ImageName
				//pSystemProcess->UniqueProcessId
				//add to list
			}

			pSystemProcess = (SYSTEM_PROCESS_INFORMATION*)(((PUCHAR)pSystemProcess) + pSystemProcess->NextEntryOffset);
		}

		bResult = true;

		if (pSystemProcess)
			free(pSystemProcess);
	}

	return bResult;
}



