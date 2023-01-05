#pragma once

#include "framework.h"

#define MAX_KEY_LENGTH 255
#define MAX_VALUE_NAME 260

typedef struct tagSTARTUPITEM
{
    TCHAR szName[MAX_VALUE_NAME];
    TCHAR szPath[MAX_PATH];
    TCHAR szDescription[MAX_PATH];
    HKEY hKey;
} STARTUPITEM, * PSTARTUPITEM;

SILVERAROWANACORE_API BOOL IsExistStartupRun(LPTSTR lpszPath,LPTSTR* lpszLnkPath);
SILVERAROWANACORE_API BOOL CreateStartupRun(LPTSTR lpszPath);
SILVERAROWANACORE_API BOOL RemoveStartupRun(LPTSTR lpszPath);

SILVERAROWANACORE_API BOOL GetStartupItems(byte* buffer, int nSizeTarget, int* count);
std::vector<STARTUPITEM> InternalGetStartupItemList(HKEY hKeyStartupKey);

#define RUN_REGPATH LR"(SOFTWARE\Microsoft\Windows\CurrentVersion\Run)"
#define RUN_ONCE_REGPATH LR"(SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce)"

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