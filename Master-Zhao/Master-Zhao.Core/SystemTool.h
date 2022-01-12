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

//refrence https://github.com/TranslucentTB/TranslucentT
struct ACCENTPOLICY
{
	int nAccentState;
	int nFlags;
	int nColor;
	int nAnimationId;
};

struct WINCOMPATTRDATA
{
	int nAttribute;
	PVOID pData;
	ULONG ulDataSize;
};

enum AccentState
{
	ACCENT_ENABLE_GRADIENT = 1, 
	ACCENT_ENABLE_TRANSPARENTGRADIENT = 2, 
	ACCENT_ENABLE_BLURBEHIND = 3,
	ACCENT_DISABLED = 4, 
	ACCENT_ENABLE_TINTED = 5,
	ACCENT_NORMAL_GRADIENT = 6 
};



typedef long(__stdcall* funNtQuerySystemInformation)(UINT, PVOID, ULONG, PULONG);

SILVERAROWANACORE_API SYSTEMTIME GetUserLoginTime();