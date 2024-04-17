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

typedef void(*FUNC)();

LRESULT WINAPI KbLLProc(int code, WPARAM wParam, LPARAM lParam);
LRESULT WINAPI MsLLProc(int code, WPARAM wParam, LPARAM lParam);
SILVERAROWANACORE_API BOOL HookStart(HWND hwnd);
SILVERAROWANACORE_API BOOL UnHookStart();
SILVERAROWANACORE_API BOOL ShowCustomStart();
SILVERAROWANACORE_API BOOL HideCustomStart();
SILVERAROWANACORE_API VOID CloseCustomStart();

SILVERAROWANACORE_API BOOL RegisterBossKeyHotKey(HWND hwnd, UINT modifier, UINT vkCode, UINT nHotKeyId);
SILVERAROWANACORE_API BOOL UnRegisterBossKeyHotKey(HWND hwnd, UINT nHotKeyId);

SILVERAROWANACORE_API BOOL GetUserProfilePicturePath(LPTSTR buf, DWORD nSize);

LPTSTR GetShellPropertyStringFromPath(LPCWSTR pszPath, PROPERTYKEY const& key);
VOID MakeFullPath(LPCWSTR pszPath, LPTSTR szFullPath, DWORD nFullPathSize);

SILVERAROWANACORE_API LPTSTR GetFileDescrption(LPTSTR pszFilePath);
SILVERAROWANACORE_API HRESULT GetShortcutPath(LPTSTR szLnkPath, LPTSTR szAbsoluatePath, DWORD nBufferSize);
SILVERAROWANACORE_API BOOL GetSpeicalFolder(DWORD csidl, LPTSTR szBuffer);

SILVERAROWANACORE_API BOOL IsProcessContainModule(LPCTSTR lpszProcessName, LPCTSTR lpszModuleName);
SILVERAROWANACORE_API BOOL IsProcessIdContainModule(DWORD dwProcessId, LPCTSTR lpszModuleName);

SILVERAROWANACORE_API BOOL CreateRemoteThreadInject(DWORD dwProcessId, LPCTSTR lpszModulePath);

