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

SILVERAROWANACORE_API SYSTEMTIME GetUserLoginTime();
SILVERAROWANACORE_API BOOL IsWindows10();
SILVERAROWANACORE_API BOOL IsWindows10OrHigher();
SILVERAROWANACORE_API BOOL IsWindows11();