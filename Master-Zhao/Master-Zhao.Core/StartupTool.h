#pragma once

#include "framework.h"

#define MAX_KEY_LENGTH 255
#define MAX_VALUE_NAME 16383

typedef struct tagSTARTUPITEM
{
    TCHAR szName[MAX_VALUE_NAME];
    TCHAR szPath[MAX_PATH];
} STARTUPITEM, * PSTARTUPITEM;

TCHAR  achValue[MAX_VALUE_NAME];
DWORD cchValue = MAX_VALUE_NAME;

SILVERAROWANACORE_API BOOL IsExistStartupRun(LPTSTR lpszPath,LPTSTR* lpszLnkPath);
SILVERAROWANACORE_API BOOL CreateStartupRun(LPTSTR lpszPath);
SILVERAROWANACORE_API BOOL RemoveStartupRun(LPTSTR lpszPath);

SILVERAROWANACORE_API BOOL GetStartupItems(tagSTARTUPITEM** items, int count);
std::vector<STARTUPITEM> InternalGetStartupItemList(HKEY hKeyStartupKey);

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