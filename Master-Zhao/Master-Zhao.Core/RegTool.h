#pragma once

#include"framework.h"

//HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel\

//Control panel {5399E694-6CE5-4D6C-8FCE-1D8870FDCBA0} 
//Computer      {20D04FE0-3AEA-1069-A2D8-08002B30309D} 
//User          {59031a47-3f72-44a7-89c5-5595fe6b30ee}
//Recycle       {645FF040-5081-101B-9F08-00AA002F954E}
//Network       {F02C1A0D-BE21-4350-88B0-7367FC96EF3C}

SILVERAROWANACORE_API BOOL SetDWORDValue(HKEY hKey, LPCTSTR lpSubKey,LPCTSTR lpValueName,DWORD value);
SILVERAROWANACORE_API BOOL SetSZValue(HKEY hKey, LPCTSTR lpSubKey, LPCTSTR lpValueName,LPCTSTR value);
SILVERAROWANACORE_API BOOL RemovRegValue(HKEY hKey, LPCTSTR lpSubKey, LPCTSTR lpValueName);

//Windows Photo Viewer
//HKLM\Software\Microsoft\Windows PhotoViewer\Capabilities\FileAssociations
// support BMP, JPEG, JPEG XR (formerly HD Photo), PNG, ICO, GIF and TIFF 
//.bmp PhotoViewer.FileAssoc.Tiff
//.dib PhotoViewer.FileAssoc.Tiff
//.jpeg PhotoViewer.FileAssoc.Tiff
//.jpg PhotoViewer.FileAssoc.Tiff
//.jxr PhotoViewer.FileAssoc.Tiff
//.jfif PhotoViewer.FileAssoc.Tiff
//.wdp PhotoViewer.FileAssoc.Tiff
//.png PhotoViewer.FileAssoc.Tiff
//.ico PhotoViewer.FileAssoc.Tiff
//.gif PhotoViewer.FileAssoc.Tiff
//.tiff PhotoViewer.FileAssoc.Tiff
#define WINDOWS_PHOTO_VIEWER_PATH LR"(SOFTWARE\Microsoft\Windows Photo Viewer\Capabilities\FileAssociations)" 
#define WINDOWS_PHOTO_VIEWER_TYPE L"PhotoViewer.FileAssoc.Tiff"
#define WINDOWS_PHOTO_VIEWER_BMP L".bmp"
#define WINDOWS_PHOTO_VIEWER_DIB L".dib"
#define WINDOWS_PHOTO_VIEWER_JPEG L".jpeg"
#define WINDOWS_PHOTO_VIEWER_JPG L".jpg"
#define WINDOWS_PHOTO_VIEWER_JXR L".jxr"
#define WINDOWS_PHOTO_VIEWER_JFIF L".jfif"
#define WINDOWS_PHOTO_VIEWER_WDP L".wdp"
#define WINDOWS_PHOTO_VIEWER_PNG L".png"
#define WINDOWS_PHOTO_VIEWER_ICO L".ico"
#define WINDOWS_PHOTO_VIEWER_GIF L".gif"
#define WINDOWS_PHOTO_VIEWER_TIFF L".tiff"
#define WINDOWS_PHOTO_VIEWER_REGISTER L"PhotoViewer.FileAssoc.Tiff"

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

//windows 10 /11 shotcut arrow
//计算机\HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons\29[REG_SZ(%windir%\System32\shell32.dll,-50)]

#define DESKTOP_SHOTCUT_ARROW_REGPATH L"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Shell Icons"
#define DESKTOP_SHOTCUT_ARROW L"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Shell Icons\\29"
#define DESKTOP_SHOTCUT_ARROW_NAME L"29"
#define DESKTOP_SHOTCUT_ARROW_VALUE L"%windir%\System32\shell32.dll,-50"

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

//HKEY_CURRENT_USER
#define STARTUP_RUN_1 LR"(Software\Microsoft\Windows\CurrentVersion\Run)"
#define STARTUP_RUN_2 LR"(Software\Microsoft\Windows\CurrentVersion\Policies\Explorer\Run)"

//HKEY_LOCAL_MACHINE
#define STARTUP_RUN_3 LR"(Software\Microsoft\Windows\CurrentVersion\Run)"
#define STARTUP_RUN_4 LR"(SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer\Run)"

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
SILVERAROWANACORE_API BOOL CreateSubKey(HKEY hKey, LPCTSTR lpSubKey);
SILVERAROWANACORE_API BOOL ExistRegValue(HKEY hKey, LPCTSTR lpSubKey,LPCTSTR lpValueName);
SILVERAROWANACORE_API BOOL QueryDWORDValue(HKEY hKey, LPCTSTR lpSubKey, LPCTSTR lpValueName, DWORD* value);
SILVERAROWANACORE_API BOOL QuerySZValue(HKEY hKey, LPCTSTR lpSubKey, LPCTSTR lpValueName, TCHAR* szValue, DWORD* nSize);
SILVERAROWANACORE_API BOOL QueryByteValue(HKEY hKey, LPCTSTR lpSubKey, LPCTSTR lpValueName, BYTE* byteValue, DWORD* nSize);
SILVERAROWANACORE_API std::vector<std::wstring> GetAllSubKey(HKEY hKey, LPCTSTR lpSubKey);

//recent file
//HKEY_CLASS_ROOT\LocalSettings\Software\Microsoft\Windows\Shell\MuiCache
//HKEY_CURRENT_USER\SOFTWARE\Classes\LocalSettings\Software\Microsoft\Windows\Shell\MuiCache

//BIOS
//HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\BIOS

//Taskbar
//HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize

//Visual Studio
//HKEY_CLASSES_ROOT\Directory\background\shell\AnyCode
//HKEY_CLASSES_ROOT\Directory\shell\AnyCode
