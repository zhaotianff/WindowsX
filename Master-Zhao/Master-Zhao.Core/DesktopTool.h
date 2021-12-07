#pragma once

#include"framework.h"
#include <Shldisp.h>
#include<vector>

#define WM_SPAWN_WORKER 0x052C

SILVERAROWANACORE_API BOOL SetBackground(LPTSTR lpImagePath);
SILVERAROWANACORE_API BOOL GetBackground(LPTSTR lpImagePath);
SILVERAROWANACORE_API BOOL GetRecentBackground(LPTSTR lpRecentPath);
SILVERAROWANACORE_API VOID SwitchToDesktop();
SILVERAROWANACORE_API VOID SwitchToWindow(HWND hwnd);
SILVERAROWANACORE_API BOOL CloseEmbedWindow();
BOOL CALLBACK EnumWindowProc(HWND hwnd, LPARAM lParam);

SILVERAROWANACORE_API BOOL EmbedWindowToDesktop(LPCTSTR lpWindowName);
