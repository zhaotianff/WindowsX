#pragma once

#include<Windows.h>

// First Windows 11 build (24H2) that can use the "raised desktop" layout.
const DWORD kRaisedDesktopMinBuild = 26100;

struct DesktopWindows
{
    HWND progman = nullptr;
    HWND defView = nullptr;  // SHELLDLL_DefView: the desktop icons
    HWND workerW = nullptr;  // WorkerW that sits behind the icons
    bool raised = false;     // Windows 11 24H2+ layout: both are children of Progman
};

// GetVersionEx lies to applications without a matching manifest; RtlGetVersion does not.
DWORD GetWindowsBuildNumber();

BOOL CALLBACK FindClassicWorkerW(HWND topLevel, LPARAM lParam);

bool LocateRaised(DesktopWindows& desktop);

bool LocateClassic(DesktopWindows& desktop);

bool LocateDesktop(DesktopWindows& desktop, DWORD build);

void SpawnWorkerW(HWND progman, DWORD build);

bool IsLayered(HWND hwnd);

void MakeChildStyle(HWND hwnd);

bool Reparent(HWND hwnd, HWND parent);

void Place(HWND hwnd, HWND parent, HWND insertAfter, const RECT* screenRect);

// Windows 11 24H2+: layered child of Progman, between the icons and Explorer's WorkerW.
bool EmbedRaisedLayered(HWND hwnd, const DesktopWindows& desktop, const RECT* screenRect);

// Child of the WorkerW behind the icons. Used on Windows 10 / 11 before 24H2, and as the
// fallback on 24H2+ when the window cannot be layered.
bool EmbedInWorkerW(HWND hwnd, const DesktopWindows& desktop, const RECT* screenRect);


// Embeds hwnd behind the desktop icons (above the static wallpaper).
// screenRect: target area in virtual-screen coordinates (e.g. MONITORINFO::rcMonitor);
//             nullptr covers the whole desktop (all monitors).
// Call again after explorer.exe restarts (RegisterWindowMessage(L"TaskbarCreated")).
bool EmbedWindowToDesktop(HWND hwnd, const RECT* screenRect = nullptr);

// Closes a window embedded with EmbedWindowToDesktop and repaints the normal wallpaper.
// Safe to call with a window that is already gone; the desktop is refreshed either way.
bool CloseEmbeddedWindow(HWND hwnd);

