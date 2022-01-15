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

bool IsWindows10()
{
	return IsWindows10OrGreater();
}

bool IsWindows11()
{
	//no windows 11 SDK
	OSVERSIONINFOEX info{};

	DWORDLONG dwlConditionMask = 0;
	VerifyVersionInfo(&info, VER_MINORVERSION, dwlConditionMask);

	//Windows 10 2004 =>	10.0.19041
	//Windows 11 =>         10.0.22000
	return info.dwMinorVersion >= 22000;
}
