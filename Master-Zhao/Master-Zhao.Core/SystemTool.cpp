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

BOOL IsWindows10()
{
	return TRUE;
}

BOOL IsWindows10OrHigher()
{
	//https://docs.microsoft.com/en-us/windows/win32/api/versionhelpers/nf-versionhelpers-iswindows10orgreater?redirectedfrom=MSDN
	//return IsWindows10OrGreater();
	return TRUE;
}

BOOL IsWindows11()
{
	//no windows 11 SDK
	

	//Windows 10 2004 =>	10.0.19041
	//Windows 11 =>         10.0.22000
	return TRUE;
}
