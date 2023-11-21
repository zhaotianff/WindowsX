#pragma once

#include "framework.h"

#define MAX_KEY_LENGTH 255
#define MAX_VALUE_NAME 260

enum class STARTUPITEM_TYPE : INT
{
    Registry,
    ShellStartup,
    TaskScheduler
};

typedef struct tagSTARTUPITEM
{
    TCHAR szName[MAX_VALUE_NAME];
    TCHAR szPath[MAX_PATH];
    TCHAR szDescription[MAX_PATH];
    HKEY hKey;
    TCHAR szRegPath[MAX_VALUE_NAME];
    DWORD samDesired;
    BOOL bEnabled;
    STARTUPITEM_TYPE type;

} STARTUPITEM, * PSTARTUPITEM;

SILVERAROWANACORE_API BOOL IsExistStartupRun(LPTSTR lpszPath,LPTSTR* lpszLnkPath);
SILVERAROWANACORE_API BOOL CreateStartupRun(LPTSTR lpszPath);
SILVERAROWANACORE_API BOOL RemoveStartupRun(LPTSTR lpszPath);

SILVERAROWANACORE_API BOOL GetStartupItems(byte* buffer, int nSizeTarget, int* count);
SILVERAROWANACORE_API BOOL GetStartupDisabledItems(byte* buffer, int nSizeTarget, int* count);
std::pair<bool, std::vector<STARTUPITEM>> InternalGetStartupItemList(HKEY hKeyRoot, LPCTSTR szRegPath, STARTUPITEM_TYPE startupType = STARTUPITEM_TYPE::Registry);
std::vector<STARTUPITEM> InternalGetStartupItemList(HKEY hKeyStartupKey,HKEY hKeyRoot,LPCTSTR szRegPath, DWORD samDesired,BOOL bEnabled, STARTUPITEM_TYPE startupType = STARTUPITEM_TYPE::Registry);
BOOL InternalGetStartupItemFromFile(PSTARTUPITEM item, LPTSTR szFile);
std::vector<STARTUPITEM> InternalGetStartupItemListFromShell(LPTSTR szStartupPath,BOOL isEnable);
SILVERAROWANACORE_API BOOL DisableStartupItem(HKEY hKey, LPTSTR szRegPath, DWORD samDesired, LPTSTR szName,STARTUPITEM_TYPE type);
SILVERAROWANACORE_API BOOL DisableShellStartupItem(LPTSTR szName, LPTSTR szPath);
SILVERAROWANACORE_API BOOL EnableStartupItem(HKEY hKey, LPTSTR szRegPath, DWORD samDesired, LPTSTR szName,STARTUPITEM_TYPE type);
SILVERAROWANACORE_API BOOL EnableShellStartupItem(LPTSTR szName, LPTSTR szPath);
BOOL InternalGetIsEnableItem(BYTE* byteData, DWORD nSize);

#define RUN_REGPATH LR"(SOFTWARE\Microsoft\Windows\CurrentVersion\Run)"
#define RUN_ONCE_REGPATH LR"(SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce)"
#define RUN_ONCE_REGPATH_DISABLE LR"(SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce\Disabled)"
#define RUN_USER_REGPATH LR"(Software\Microsoft\Windows\CurrentVersion\Run)"

#define RUN_REGPATH_DISABLE LR"(SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run)"
#define RUN_REGPATH_DISABLE_RUN32 LR"(SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run32)"
#define RUN_REGPATH_DISABLE_STARTUPFOLDER LR"(SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\StartupFolder)"

//HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
//HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer\Run
//HKEY_CURRENT_USER\Software\Microsoft\WindowsNT\CurrentVersion\Windows
//HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon
//HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System\Shell
//HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run

//HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run
//HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run
//HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon
//HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer\Run
//HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\ShellServiceObjectDelayLoad

// shell:startup