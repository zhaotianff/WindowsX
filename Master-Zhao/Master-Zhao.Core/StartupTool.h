#pragma once

#include "framework.h"

typedef struct tagStartupItem
{
    TCHAR szName[128];
    TCHAR szPath[MAX_PATH];
} StartupItem, * PStartupItem;

SILVERAROWANACORE_API BOOL IsExistStartupRun(LPTSTR lpszPath,LPTSTR* lpszLnkPath);
SILVERAROWANACORE_API BOOL CreateStartupRun(LPTSTR lpszPath);
SILVERAROWANACORE_API BOOL RemoveStartupRun(LPTSTR lpszPath);

SILVERAROWANACORE_API BOOL GetStartupItems(tagStartupItem** items, int count);

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