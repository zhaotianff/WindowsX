#include"SystemTool.h"
#include<functional>

SYSTEMTIME GetUserLoginTime()
{
	HINSTANCE hInstance = LoadLibrary(L"ntdll.dll");
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