#include"SystemTool.h"
#include<VersionHelpers.h>
#include<functional>

SYSTEMTIME GetUserLoginTime()
{
	HINSTANCE hInstance = LoadLibrary(L"ntdll.dll");
	if (hInstance == NULL)
		return SYSTEMTIME();

	auto NtQuerySystemInformation = (funNtQuerySystemInformation)GetProcAddress(hInstance, "NtQuerySystemInformation");

	if (NtQuerySystemInformation)
	{
		SYSTEM_TIME_INFORMATION tInfo{};
		auto ret = NtQuerySystemInformation(3, &tInfo, sizeof(tInfo), 0);
		FILETIME stFile = *(FILETIME*)&tInfo.liKeBootTime;
		SYSTEMTIME stSys;
		FileTimeToLocalFileTime(&stFile, &stFile);
		FileTimeToSystemTime(&stFile, &stSys);

		FreeLibrary(hInstance);
		return stSys;
	}
	return SYSTEMTIME();
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
