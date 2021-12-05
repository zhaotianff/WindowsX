#pragma once

#include"framework.h"
#include <Shldisp.h>

#define WM_SPAWN_WORKER 0x052C

typedef struct tagBASICWINDOWINFO
{
	TCHAR szWindowName[260];
	HWND hwnd;
	tagBASICWINDOWINFO* next;
}*PBASICWINDOWINFO;

SILVERAROWANACORE_API BOOL SetBackground(LPTSTR lpImagePath);
SILVERAROWANACORE_API BOOL GetBackground(LPTSTR lpImagePath);
SILVERAROWANACORE_API BOOL GetRecentBackground(LPTSTR lpRecentPath);
SILVERAROWANACORE_API VOID SwitchToDesktop();
SILVERAROWANACORE_API VOID SwitchToWindow(HWND hwnd);

BOOL CALLBACK EnumWindowProc(HWND hwnd, LPARAM lParam);
SILVERAROWANACORE_API BOOL EmbedWindowToDesktop(LPTSTR lpWindowName);
SILVERAROWANACORE_API BOOL RestoreEmbedHwnd();
