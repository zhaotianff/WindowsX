#pragma once

#include"framework.h"

//HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel\

//Control panel {5399E694-6CE5-4D6C-8FCE-1D8870FDCBA0} 
//Computer      {20D04FE0-3AEA-1069-A2D8-08002B30309D} 
//User          {59031a47-3f72-44a7-89c5-5595fe6b30ee}
//Recycle       {645FF040-5081-101B-9F08-00AA002F954E}
//Network       {F02C1A0D-BE21-4350-88B0-7367FC96EF3C}

SILVERAROWANACORE_API BOOL SetDWORDValue(HKEY hKey, LPCTSTR lpSubKey,LPCTSTR lpValueName,DWORD value);
SILVERAROWANACORE_API BOOL SetSZValue(HKEY hKey, LPCTSTR lpSubKey, LPCTSTR lpValueName, TCHAR* value);

//Windows Photo Viewer
//HKLM\Software\Microsoft\Windows PhotoViewer\Capabilities\FileAssociations
//.jpg
//.png
//.bmp
//.jpeg

//Run history
//HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU

//Local Service
//HKLM\System\ControlSet001\Services\pcw

//Uninstall
//HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall

//Start menu color
//HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Accent

BOOL ExistSubKey(HKEY hKey, LPCTSTR lpSubKey);
