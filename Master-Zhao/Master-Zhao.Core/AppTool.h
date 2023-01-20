#pragma once

#include "framework.h"
#include "RegTool.h"

//HKEY_LOCAL_MACHINE & //HKEY_CURRENT_USER
#define APP_PATH LR"(SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\)"

SILVERAROWANACORE_API BOOL GetAppPath(LPTSTR szBuffer, DWORD nSize);
SILVERAROWANACORE_API BOOL AddAppPath(LPTSTR szName, LPTSTR szPath);