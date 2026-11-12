//#include <windows.h>
//#include "EmbedTool.h"
//
//#ifndef WS_EX_NOREDIRECTIONBITMAP
//#define WS_EX_NOREDIRECTIONBITMAP 0x00200000L
//#endif
//
//
//BOOL CALLBACK FindClassicWorkerW(HWND topLevel, LPARAM lParam)
//{
//	auto* desktop = reinterpret_cast<DesktopWindows*>(lParam);
//	HWND defView = FindWindowExW(topLevel, nullptr, L"SHELLDLL_DefView", nullptr);
//	if (!defView)
//		return TRUE;
//
//	desktop->defView = defView;
//	// The wallpaper WorkerW is the next top-level WorkerW after the icon host.
//	desktop->workerW = FindWindowExW(nullptr, topLevel, L"WorkerW", nullptr);
//	return FALSE;
//}
//
//bool LocateDesktop(DesktopWindows& desktop)
//{
//	desktop = DesktopWindows{};
//	desktop.progman = FindWindowW(L"Progman", nullptr);
//	if (!desktop.progman)
//		return false;
//
//	// Since Windows 11 24H2, Progman has no redirection bitmap and hosts both
//	// SHELLDLL_DefView and the wallpaper WorkerW as its own children.
//	desktop.raised = (GetWindowLongPtrW(desktop.progman, GWL_EXSTYLE) & WS_EX_NOREDIRECTIONBITMAP) != 0;
//	if (desktop.raised)
//	{
//		desktop.defView = FindWindowExW(desktop.progman, nullptr, L"SHELLDLL_DefView", nullptr);
//		desktop.workerW = FindWindowExW(desktop.progman, nullptr, L"WorkerW", nullptr);
//	}
//	else
//	{
//		EnumWindows(FindClassicWorkerW, reinterpret_cast<LPARAM>(&desktop));
//	}
//	return desktop.defView && desktop.workerW;
//}
//
//
//// Embeds hwnd behind the desktop icons (above the static wallpaper).
//// screenRect: target area in virtual-screen coordinates (e.g. MONITORINFO::rcMonitor);
////             nullptr covers the whole desktop (all monitors).
//// Call again after explorer.exe restarts (RegisterWindowMessage(L"TaskbarCreated")).
//bool EmbedWindow(HWND hwnd, const RECT* screenRect)
//{
//	if (!IsWindow(hwnd))
//		return false;
//
//	HWND progman = FindWindowW(L"Progman", nullptr);
//	if (!progman)
//		return false;
//
//	// Undocumented: asks Progman to create the WorkerW behind the icons.
//	DWORD_PTR result = 0;
//	SendMessageTimeoutW(progman, 0x052C, 0xD, 0x1, SMTO_NORMAL, 1000, &result);
//
//	DesktopWindows desktop;
//	bool found = false;
//	for (int i = 0; i < 20 && !(found = LocateDesktop(desktop)); ++i)
//		Sleep(50);
//	if (!found)
//		return false;
//
//	HWND parent = desktop.raised ? desktop.progman : desktop.workerW;
//
//	LONG_PTR style = GetWindowLongPtrW(hwnd, GWL_STYLE);
//	style &= ~static_cast<LONG_PTR>(WS_POPUP | WS_OVERLAPPEDWINDOW);
//	style |= WS_CHILD;
//	SetWindowLongPtrW(hwnd, GWL_STYLE, style);
//
//	LONG_PTR exStyle = GetWindowLongPtrW(hwnd, GWL_EXSTYLE);
//	exStyle &= ~static_cast<LONG_PTR>(WS_EX_APPWINDOW | WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE | WS_EX_DLGMODALFRAME);
//	if (desktop.raised)
//		exStyle |= WS_EX_LAYERED;  // Progman has no surface; the child must bring its own.
//	SetWindowLongPtrW(hwnd, GWL_EXSTYLE, exStyle);
//
//	SetParent(hwnd, parent);
//	if (GetAncestor(hwnd, GA_PARENT) != parent)
//		return false;
//
//	if (desktop.raised)
//		SetLayeredWindowAttributes(hwnd, 0, 255, LWA_ALPHA);
//
//	RECT rc;
//	if (screenRect)
//	{
//		rc = *screenRect;
//		MapWindowPoints(nullptr, parent, reinterpret_cast<POINT*>(&rc), 2);
//	}
//	else
//	{
//		GetClientRect(parent, &rc);
//	}
//
//	// Raised layout: directly below the icons. Classic layout: top of the WorkerW.
//	SetWindowPos(hwnd, desktop.raised ? desktop.defView : HWND_TOP,
//		rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top,
//		SWP_NOACTIVATE | SWP_FRAMECHANGED | SWP_SHOWWINDOW);
//
//	if (desktop.raised)
//	{
//		// Keep Explorer's own wallpaper below ours.
//		SetWindowPos(desktop.workerW, HWND_BOTTOM, 0, 0, 0, 0,
//			SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
//	}
//	return true;
//
//}
//
//bool CloseEmbeddedWindow(HWND hwnd)
//{
//	if (IsWindow(hwnd))
//	{
//		ShowWindow(hwnd, SW_HIDE);
//		if (GetWindowThreadProcessId(hwnd, nullptr) == GetCurrentThreadId())
//			DestroyWindow(hwnd);
//		else
//		{
//			DWORD_PTR result = 0;
//			SendMessageTimeoutW(hwnd, WM_CLOSE, 0, 0, SMTO_ABORTIFHUNG, 2000, &result);
//		}
//	}
//	return SystemParametersInfoW(SPI_SETDESKWALLPAPER, 0, nullptr, SPIF_UPDATEINIFILE) != FALSE;
//}