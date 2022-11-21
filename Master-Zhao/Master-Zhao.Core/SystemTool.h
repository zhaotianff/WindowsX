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
typedef HRESULT(__stdcall* funSHGetUserPicturePath)(LPCWSTR pwszPicOrUserName, DWORD sguppFlags, LPWSTR pwszPicPath, UINT picPathLen);

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

typedef void(*FUNC)();

LRESULT KbLLProc(int code, WPARAM wParam, LPARAM lParam);
LRESULT MsLLProc(int code, WPARAM wParam, LPARAM lParam);
SILVERAROWANACORE_API BOOL HookStart(HWND hwnd);
SILVERAROWANACORE_API BOOL UnHookStart();
SILVERAROWANACORE_API BOOL ShowCustomStart();
SILVERAROWANACORE_API BOOL HideCustomStart();
SILVERAROWANACORE_API VOID CloseCustomStart();

SILVERAROWANACORE_API BOOL RegisterBossKeyHotKey(HWND hwnd, UINT modifier, UINT vkCode, UINT nHotKeyId);
SILVERAROWANACORE_API BOOL UnRegisterBossKeyHotKey(HWND hwnd, UINT nHotKeyId);

SILVERAROWANACORE_API BOOL GetUserProfilePicturePath(LPTSTR buf, DWORD nSize);

