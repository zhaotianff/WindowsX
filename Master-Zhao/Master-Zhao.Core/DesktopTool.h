#pragma once

#include"framework.h"
#include <Shldisp.h>
#include<vector>
#include<shobjidl_core.h>
#include<thumbcache.h>

#define WM_SPAWN_WORKER 0x052C

enum ACCENT_STATE :INT
{
	ACCENT_DISABLED = 0,
	ACCENT_ENABLE_GRADIENT = 1,
	ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
	ACCENT_ENABLE_BLURBEHIND =3,
	ACCENT_ENABLE_ACRYLICBLURBEHIND = 4
};

struct ACCENT_POLICY
{
	ACCENT_STATE AcentState;
	UINT AccentFlags;
	COLORREF GradientColor;
	LONG AnimationId;
};

enum WINDOWCOMPOSITIONATTRIB : INT
{
	WCA_ACCENT_POLICY = 0x13
};

struct WINDOWCOMPOSITIONATTRIBDATA
{
	WINDOWCOMPOSITIONATTRIB Attrib;
	LPVOID pvData;
	UINT cbData;
};

typedef BOOL(WINAPI* PFN_SET_WINDOW_COMPOSITION_ATTRIBUTE)(HWND, const WINDOWCOMPOSITIONATTRIBDATA*);

SILVERAROWANACORE_API BOOL SetBackground(LPTSTR lpImagePath);
SILVERAROWANACORE_API BOOL GetBackground(LPTSTR lpImagePath);
SILVERAROWANACORE_API BOOL GetRecentBackground(LPTSTR lpRecentPath);
SILVERAROWANACORE_API VOID SwitchToDesktop();
SILVERAROWANACORE_API VOID SwitchToWindow(HWND hwnd);
SILVERAROWANACORE_API BOOL CloseEmbedWindow();
BOOL CALLBACK EnumWindowProc(HWND hwnd, LPARAM lParam);
SILVERAROWANACORE_API HBITMAP GetFileThumbnail(PCWSTR path);
SILVERAROWANACORE_API BOOL CenterStartMenu();
SILVERAROWANACORE_API BOOL CenterTaskListIcon();
SILVERAROWANACORE_API BOOL BlurTaskbar(ACCENT_POLICY);

SILVERAROWANACORE_API BOOL EmbedWindowToDesktop(LPCTSTR lpWindowName);
