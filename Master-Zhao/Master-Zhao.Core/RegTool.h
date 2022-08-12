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
SILVERAROWANACORE_API BOOL RemovRegValue(HKEY hKey, LPCTSTR lpSubKey, LPCTSTR lpValueName);

//Windows Photo Viewer
//HKLM\Software\Microsoft\Windows PhotoViewer\Capabilities\FileAssociations
// support BMP, JPEG, JPEG XR (formerly HD Photo), PNG, ICO, GIF and TIFF 
//.bmp PhotoViewer.FileAssoc.Tiff
//.jpeg PhotoViewer.FileAssoc.Tiff
//.jpg PhotoViewer.FileAssoc.Tiff
//.png PhotoViewer.FileAssoc.Tiff
//.ico PhotoViewer.FileAssoc.Tiff
//.gif PhotoViewer.FileAssoc.Tiff
//.tiff PhotoViewer.FileAssoc.Tiff

//Run history
//HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU

//Local Service
//HKLM\System\ControlSet001\Services\pcw

//Uninstall
//HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall

//Start menu color
//HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Accent

//Shortcut arrow
//remove HKCR\lnkfile\IsShortcut

//Paint version info
//HKEY_CURRENT_USER\Control Panel\Desktop\PaintDesktopVersion(0x1)
#define PAINT_VERSION_INFO_REGPATH L"Control Panel\\Desktop\\PaintDesktopVersion"

//Group policy
//Enabled
//Disabled
//Not Configured
//HKEY_CURRENT_USER\Software\Policies(preferred location)
//HKEY_LOCAL_MACHINE\Software\Policies(preferred location)
//HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies
//HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Policies

//taskband
#define TASKBAND_REGPATH L"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Taskband"
#define TASKBAR_THUMB_SIZE L"MinThumbSizePx"       //Taskbar min thumbnail size
#define TASKBAR_THUMB_MAX_SIZE "MaxThumbSizePx"    //Taskbar max thumbnail size
#define TASKBAR_THUMB_NUM_THUMB "NumThumbnails"    //specifies how many thumbnails to display for an item. 
#define TASKBAR_THUMB_TEXT_HEIGHT "TextHeightPx"   //specifies the window title text height in the thumbnail view. 
#define TASKBAR_THUMB_TOP_MARGIN "TopMarginPx"     //specifies the Top margin for stacked windows.
#define TASKBAR_THUMB_LEFT_MARGIN "LeftMarginPx"
#define TASKBAR_THUMB_RIGHT_MARGIN "RightMarginPx"
#define TASKBAR_THUMB_BOTTOM_MARGIN "BottomMarginPx"
#define TASKBAR_THUMB_THUMB_SPACING_X "ThumbSpacingXPx" //specifies the horizontal spacing between the stacked items
#define TASKBAR_THUMB_THUMB_SPACING_Y "ThumbSpacingYPx"

#define STARTUP_RUN_1 LR"(HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run)"
#define STARTUP_RUN_2 LR"(HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Run)"
#define STARTUP_RUN_3 LR"(HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer\Run)"
#define STARTUP_RUN_4 LR"(HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer\Run)"

//RunOnce 自动执行一次
#define STARTUP_RUN_5 LR"(HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\RunOnce)"
#define STARTUP_RUN_6 LR"(HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\RunOnce)"

//RunServicesOnce
#define STARTUP_RUN_7 LR"(HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\RunServicesOnce)"
#define STARTUP_RUN_8 LR"(HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\RunServicesOnce)"

//RunServices
#define STARTUP_RUN_9 LR"(HKEY_CURRENT_USER\ Software\Microsoft\Windows\CurrentVersion\RunServices)"
#define STARTUP_RUN_10 LR"(HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\RunServices)"

//load
#define STARTUP_RUN_11 LR"(HKEY_CURRENT_USER\Software\Microsoft\WindowsNT\CurrentVersion\Windows)"

//Winlogon)"
#define STARTUP_RUN_12 LR"(HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon)"
#define STARTUP_RUN_13 LR"(HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon)"

//other
#define STARTUP_RUN_14 LR"(HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System\Shell)"
#define STARTUP_RUN_15 LR"(HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\ShellServiceObjectDelayLoad)"
#define STARTUP_RUN_16 LR"(HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\System\Scripts)"
#define STARTUP_RUN_17 LR"(HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\System\Scripts)"

SILVERAROWANACORE_API BOOL ExistSubKey(HKEY hKey, LPCTSTR lpSubKey);
SILVERAROWANACORE_API BOOL ExistRegValue(HKEY hKey, LPCTSTR lpSubKey,LPCTSTR lpValueName);
SILVERAROWANACORE_API BOOL QueryDWORDValue(HKEY hKey, LPCTSTR lpSubKey, LPCTSTR lpValueName, DWORD* value);

//recent file
//HKEY_CLASS_ROOT\LocalSettings\Software\Microsoft\Windows\Shell\MuiCache
//HKEY_CURRENT_USER\SOFTWARE\Classes\LocalSettings\Software\Microsoft\Windows\Shell\MuiCache

//BIOS
//HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\BIOS

//Taskbar
//HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize
