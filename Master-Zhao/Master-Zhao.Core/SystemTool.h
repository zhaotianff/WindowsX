/////////////////////////////////
// 
// SystemTool.cpp
// 
// system tool code
// 
// author:  zti
// created:2021.01.12
//  
//////////////////////////////////
#pragma once

#include"framework.h"

struct SYSTEM_TIME_INFORMATION
{
	LARGE_INTEGER liKeBootTime;
	LARGE_INTEGER liKeSystemTime;
	LARGE_INTEGER liExpTimeZoneBias;
	DWORD dwReserved;
};

typedef long(__stdcall* funNtQuerySystemInformation)(UINT, PVOID, ULONG, PULONG);
typedef void(__stdcall* funRtlGetNtVersionNumbers)(DWORD*, DWORD*, DWORD*);

SILVERAROWANACORE_API SYSTEMTIME GetUserLoginTime();
void GetVersionNumbers(DWORD*, DWORD*, DWORD*);
SILVERAROWANACORE_API BOOL IsWindows10();
SILVERAROWANACORE_API BOOL IsWindows10OrHigher();
SILVERAROWANACORE_API BOOL IsWindows11();

SILVERAROWANACORE_API BOOL RegisterFastRunHotKey(HWND hwnd);
SILVERAROWANACORE_API BOOL UnRegisterFastRunHotKey();

SILVERAROWANACORE_API int GetCPUKernelCount();
SILVERAROWANACORE_API BOOL AdjustPrivilege();
SILVERAROWANACORE_API BOOL ForceDeleteFile(LPTSTR lpszFilePah);

///system shutdown
SILVERAROWANACORE_API VOID Shutdown();
SILVERAROWANACORE_API VOID SwitchUser();
SILVERAROWANACORE_API VOID Logoff();
SILVERAROWANACORE_API VOID Lock();
SILVERAROWANACORE_API VOID Restart();
SILVERAROWANACORE_API VOID Sleep();
