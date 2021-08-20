#pragma once

#include"framework.h"

//HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\ClassicStartMenu\

//Control panel {5399E694-6CE5-4D6C-8FCE-1D8870FDCBA0}
//Computer      {20D04FE0-3AEA-1069-A2D8-08002B30309D}
//User          {59031a47-3f72-44a7-89c5-5595fe6b30ee}

SILVERAROWANACORE_API BOOL SetValue(HKEY hKey, LPCTSTR lpSubKey,LPCTSTR lpValueName,BYTE* value);