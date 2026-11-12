//#pragma once
//
///*
// * Windows 10 / 11 before 24H2: the window becomes a child of the WorkerW behind the icons.
// * Windows 11 24H2+: the window becomes a layered child of Progman, placed just below the icons.
// * screenRect: pass a monitor's rcMonitor for one screen; nullptr covers all monitors.
// * Explorer restart: call it again when you receive the TaskbarCreated message.
// */
//
//#include<Windows.h>
//
//
//struct DesktopWindows
//{
//	HWND progman = nullptr;
//	HWND defView = nullptr;  // SHELLDLL_DefView: the desktop icons
//	HWND workerW = nullptr;  // WorkerW that sits behind the icons
//	bool raised = false;     // Windows 11 24H2+ layout
//};
//
//BOOL CALLBACK FindClassicWorkerW(HWND topLevel, LPARAM lParam);
//
//bool LocateDesktop(DesktopWindows& desktop);
//
//// Embeds hwnd behind the desktop icons (above the static wallpaper).
//// screenRect: target area in virtual-screen coordinates (e.g. MONITORINFO::rcMonitor);
////             nullptr covers the whole desktop (all monitors).
//// Call again after explorer.exe restarts (RegisterWindowMessage(L"TaskbarCreated")).
//bool EmbedWindow(HWND hwnd, const RECT* screenRect = nullptr);
//
//
///// <summary>destroys the window directly on its own thread; for a window owned by another thread or process it sends WM_CLOSE instead.</summary>
///// <param name="hwnd"></param> 
///// <returns></returns>
//bool CloseEmbeddedWindow(HWND hwnd);