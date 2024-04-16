#pragma once

#include"framework.h"
#include"include/MinHook.h"

#pragma comment(lib,"lib/libMinHook-x64-v141-mtd.lib")

enum class Enm_BackgroundPos : DWORD
{
	LeftTop = 0,
	RightTop = 1,
	LeftBottom = 2,
	RightBottom = 3,
	Center = 4,
	Uniform = 5,
	UniformToFill = 6
};

Enm_BackgroundPos enm_Pos = Enm_BackgroundPos::RightBottom;
DWORD dw_Alpha = 255;

typedef HWND(WINAPI* FuncCreateWindowExW)(DWORD, LPCWSTR, LPCWSTR, DWORD,
	int, int, int, int, HWND, HMENU, HINSTANCE, LPVOID);
FuncCreateWindowExW fpCreateWindowExW;

typedef BOOL(WINAPI* FuncDestroyWindow)(HWND);
FuncDestroyWindow fpDestroyWindow;

typedef HDC(WINAPI* FuncBeginPaint)(HWND, LPPAINTSTRUCT);
FuncBeginPaint fpBeginPaint;

typedef int(WINAPI* FuncFillRect)(HDC, const RECT*, HBRUSH);
FuncFillRect fpFillRect;

typedef HDC(WINAPI* FuncCreateCompatibleDC)(HDC);
FuncCreateCompatibleDC fpCreateCompatibleDC;



SILVERAROWANACORE_API BOOL SetExplorerBackground(LPTSTR szPath, DWORD alpha, Enm_BackgroundPos pos);
SILVERAROWANACORE_API VOID SetExplorerBackgroundParam(DWORD alpha, Enm_BackgroundPos pos);
