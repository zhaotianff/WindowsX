#include "EmbedTool2.h"

#ifndef WS_EX_NOREDIRECTIONBITMAP
#define WS_EX_NOREDIRECTIONBITMAP 0x00200000L
#endif


    // GetVersionEx lies to applications without a matching manifest; RtlGetVersion does not.
    DWORD GetWindowsBuildNumber()
    {
        HMODULE ntdll = GetModuleHandleW(L"ntdll.dll");
        FARPROC proc = ntdll ? GetProcAddress(ntdll, "RtlGetVersion") : nullptr;
        if (!proc)
            return 0;

        typedef LONG(WINAPI* RtlGetVersionFn)(OSVERSIONINFOW*);
        RtlGetVersionFn rtlGetVersion = reinterpret_cast<RtlGetVersionFn>(reinterpret_cast<void*>(proc));

        OSVERSIONINFOW info = {};
        info.dwOSVersionInfoSize = sizeof(info);
        return rtlGetVersion(&info) == 0 ? info.dwBuildNumber : 0;
    }

    BOOL CALLBACK FindClassicWorkerW(HWND topLevel, LPARAM lParam)
    {
        auto* desktop = reinterpret_cast<DesktopWindows*>(lParam);
        HWND defView = FindWindowExW(topLevel, nullptr, L"SHELLDLL_DefView", nullptr);
        if (!defView)
            return TRUE;

        desktop->defView = defView;
        // The wallpaper WorkerW is the next top-level WorkerW after the icon host.
        desktop->workerW = FindWindowExW(nullptr, topLevel, L"WorkerW", nullptr);
        return FALSE;
    }

    bool LocateRaised(DesktopWindows& desktop)
    {
        desktop.defView = FindWindowExW(desktop.progman, nullptr, L"SHELLDLL_DefView", nullptr);
        desktop.workerW = FindWindowExW(desktop.progman, nullptr, L"WorkerW", nullptr);
        desktop.raised = desktop.defView && desktop.workerW;
        return desktop.raised;
    }

    bool LocateClassic(DesktopWindows& desktop)
    {
        desktop.defView = nullptr;
        desktop.workerW = nullptr;
        desktop.raised = false;
        EnumWindows(FindClassicWorkerW, reinterpret_cast<LPARAM>(&desktop));
        return desktop.defView && desktop.workerW;
    }

    bool LocateDesktop(DesktopWindows& desktop, DWORD build)
    {
        desktop = DesktopWindows{};
        desktop.progman = FindWindowW(L"Progman", nullptr);
        if (!desktop.progman)
            return false;

        // The raised layout was rolled out by update within 24H2, so the build number
        // only tells which layout to expect; the window tree decides.
        bool noRedirection = (GetWindowLongPtrW(desktop.progman, GWL_EXSTYLE) & WS_EX_NOREDIRECTIONBITMAP) != 0;
        bool expectRaised = build >= kRaisedDesktopMinBuild || noRedirection;

        if (expectRaised)
            return LocateRaised(desktop) || LocateClassic(desktop);
        return LocateClassic(desktop) || LocateRaised(desktop);
    }

    void SpawnWorkerW(HWND progman, DWORD build)
    {
        // Undocumented: asks Progman to create the WorkerW behind the icons.
        DWORD_PTR result = 0;
        SendMessageTimeoutW(progman, 0x052C, 0xD, 0x0, SMTO_NORMAL, 1000, &result);
        SendMessageTimeoutW(progman, 0x052C, 0xD, 0x1, SMTO_NORMAL, 1000, &result);
        if (build < kRaisedDesktopMinBuild)
            SendMessageTimeoutW(progman, 0x052C, 0x0, 0x0, SMTO_NORMAL, 1000, &result);
    }

    bool IsLayered(HWND hwnd)
    {
        return (GetWindowLongPtrW(hwnd, GWL_EXSTYLE) & WS_EX_LAYERED) != 0;
    }

    void MakeChildStyle(HWND hwnd)
    {
        LONG_PTR exStyle = GetWindowLongPtrW(hwnd, GWL_EXSTYLE);
        exStyle &= ~static_cast<LONG_PTR>(WS_EX_APPWINDOW | WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE | WS_EX_DLGMODALFRAME);
        SetWindowLongPtrW(hwnd, GWL_EXSTYLE, exStyle);

        LONG_PTR style = GetWindowLongPtrW(hwnd, GWL_STYLE);
        style &= ~static_cast<LONG_PTR>(WS_POPUP | WS_OVERLAPPEDWINDOW);
        style |= WS_CHILD;
        SetWindowLongPtrW(hwnd, GWL_STYLE, style);
    }

    bool Reparent(HWND hwnd, HWND parent)
    {
        SetParent(hwnd, parent);
        return GetAncestor(hwnd, GA_PARENT) == parent;
    }

    void Place(HWND hwnd, HWND parent, HWND insertAfter, const RECT* screenRect)
    {
        RECT rc;
        if (screenRect)
        {
            rc = *screenRect;
            MapWindowPoints(nullptr, parent, reinterpret_cast<POINT*>(&rc), 2);
        }
        else
        {
            GetClientRect(parent, &rc);
        }
        SetWindowPos(hwnd, insertAfter, rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top,
            SWP_NOACTIVATE | SWP_FRAMECHANGED | SWP_SHOWWINDOW);
    }

    // Windows 11 24H2+: layered child of Progman, between the icons and Explorer's WorkerW.
    bool EmbedRaisedLayered(HWND hwnd, const DesktopWindows& desktop, const RECT* screenRect)
    {
        // Must be applied while the window is still top-level: a child window only accepts
        // WS_EX_LAYERED when the application manifest declares Windows 8+ support.
        SetWindowLongPtrW(hwnd, GWL_EXSTYLE, GetWindowLongPtrW(hwnd, GWL_EXSTYLE) | WS_EX_LAYERED);
        if (!IsLayered(hwnd) || !SetLayeredWindowAttributes(hwnd, 0, 255, LWA_ALPHA))
            return false;

        MakeChildStyle(hwnd);
        if (!Reparent(hwnd, desktop.progman) || !IsLayered(hwnd))
            return false;

        Place(hwnd, desktop.progman, desktop.defView, screenRect);
        // Keep Explorer's own wallpaper below ours.
        SetWindowPos(desktop.workerW, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        return true;
    }

    // Child of the WorkerW behind the icons. Used on Windows 10 / 11 before 24H2, and as the
    // fallback on 24H2+ when the window cannot be layered.
    bool EmbedInWorkerW(HWND hwnd, const DesktopWindows& desktop, const RECT* screenRect)
    {
        SetWindowLongPtrW(hwnd, GWL_EXSTYLE, GetWindowLongPtrW(hwnd, GWL_EXSTYLE) & ~static_cast<LONG_PTR>(WS_EX_LAYERED));
        MakeChildStyle(hwnd);
        if (!Reparent(hwnd, desktop.workerW))
            return false;

        ShowWindow(desktop.workerW, SW_SHOWNA);
        Place(hwnd, desktop.workerW, HWND_TOP, screenRect);
        return true;
    }


// Embeds hwnd behind the desktop icons (above the static wallpaper).
// screenRect: target area in virtual-screen coordinates (e.g. MONITORINFO::rcMonitor);
//             nullptr covers the whole desktop (all monitors).
// Call again after explorer.exe restarts (RegisterWindowMessage(L"TaskbarCreated")).
bool EmbedWindowToDesktop(HWND hwnd, const RECT* screenRect)
{
    if (!IsWindow(hwnd))
        return false;

    HWND progman = FindWindowW(L"Progman", nullptr);
    if (!progman)
        return false;

    const DWORD build = GetWindowsBuildNumber();

    DesktopWindows desktop;
    bool found = LocateDesktop(desktop, build);
    if (!found || !desktop.raised)
    {
        SpawnWorkerW(progman, build);
        for (int i = 0; i < 40 && !(found = LocateDesktop(desktop, build)); ++i)
            Sleep(50);
    }
    if (!found)
        return false;

    if (desktop.raised && EmbedRaisedLayered(hwnd, desktop, screenRect))
        return true;
    return EmbedInWorkerW(hwnd, desktop, screenRect);
}

// Closes a window embedded with EmbedWindowToDesktop and repaints the normal wallpaper.
// Safe to call with a window that is already gone; the desktop is refreshed either way.
bool CloseEmbeddedWindow(HWND hwnd)
{
    if (IsWindow(hwnd))
    {
        ShowWindow(hwnd, SW_HIDE);
        if (GetWindowThreadProcessId(hwnd, nullptr) == GetCurrentThreadId())
        {
            DestroyWindow(hwnd);
        }
        else
        {
            // DestroyWindow only works on the owning thread.
            DWORD_PTR result = 0;
            SendMessageTimeoutW(hwnd, WM_CLOSE, 0, 0, SMTO_ABORTIFHUNG, 2000, &result);
        }
    }

    // Makes Explorer redraw its wallpaper, clearing the last frame we left behind.
    return SystemParametersInfoW(SPI_SETDESKWALLPAPER, 0, nullptr, SPIF_UPDATEINIFILE) != FALSE;
}
