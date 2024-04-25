// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.h"
#include "shellhook.h"
#include"logger.h"

#define SHELLHOOK_MAILSLOT_NAME L"\\\\.\\mailslot\\shellhook_slot"

extern HMODULE g_hShellHookDllModule;
ULONG_PTR g_lGdiPlusToken;
std::unordered_map<DWORD, ShellWindow> g_mapShellWindow;
BitmapImage* img = NULL;
BOOL g_bMonitor = TRUE;
shellhook::logger* logger = nullptr;

const HINSTANCE hModule = LoadLibrary(TEXT("user32.dll"));

struct ACCENTPOLICY
{
    int nAccentState;
    int nFlags;
    int nColor;
    int nAnimationId;
};
struct WINCOMPATTRDATA
{
    int nAttribute;
    PVOID pData;
    ULONG ulDataSize;
};


typedef BOOL(WINAPI* pSetWindowCompositionAttribute)(HWND, WINCOMPATTRDATA*);
const pSetWindowCompositionAttribute SetWindowCompositionAttribute = (pSetWindowCompositionAttribute)GetProcAddress(hModule, "SetWindowCompositionAttribute");

void SetWindowBlur(HWND hWnd)
{

    if (hModule)
    {
        if (SetWindowCompositionAttribute)
        {
            ACCENTPOLICY policy = { 3, 0, 0, 0 }; // ACCENT_ENABLE_BLURBEHIND=3, ACCENT_INVALID=4...
            WINCOMPATTRDATA data = { 19, &policy, sizeof(ACCENTPOLICY) }; // WCA_ACCENT_POLICY=19
            SetWindowCompositionAttribute(hWnd, &data);
        }

    }
}

typedef HWND(WINAPI* Func_CreateWindowExW)(
    DWORD dwExStyle,
    LPCWSTR lpClassName,
    LPCWSTR lpWindowName,
    DWORD dwStyle,
    int X,
    int Y,
    int nWidth,
    int nHeight,
    HWND hWndParent,
    HMENU hMenu,
    HINSTANCE hInstance,
    LPVOID lpParam);
typedef BOOL(WINAPI* Func_DestroyWindow)(HWND hwnd);
typedef HDC(WINAPI* Func_BeginPaint)(HWND hWnd, LPPAINTSTRUCT lpPaint);
typedef int(WINAPI* Func_FillRect)(HDC hDC, RECT* lprc, HBRUSH hbr);
typedef HDC(WINAPI* Func_CreateCompatibleDC)(HDC hdc);


Func_CreateWindowExW InitCreateWindowExW;
Func_DestroyWindow InitDestroyWindow;
Func_BeginPaint InitBeginPaint;
Func_FillRect InitFillRect;
Func_CreateCompatibleDC InitCreateCompatibleDC;

HWND MyCreateWindowExW(
    DWORD dwExStyle,
    LPCWSTR lpClassName,
    LPCWSTR lpWindowName,
    DWORD dwStyle,
    int X,
    int Y,
    int nWidth,
    int nHeight,
    HWND hWndParent,
    HMENU hMenu,
    HINSTANCE hInstance,
    LPVOID lpParam);

BOOL MyDestroyWindow(HWND hwnd);
HDC MyBeginPaint(HWND hWnd, LPPAINTSTRUCT lpPaint);
int MyFillRect(HDC hDC, RECT* lprc, HBRUSH hbr);
HDC MyCreateCompatibleDC(HDC hdc);
VOID StartHook();

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        if (NULL == g_hShellHookDllModule)
        {
            g_hShellHookDllModule = hModule;
            DisableThreadLibraryCalls(hModule);

            TCHAR szModulePath[MAX_PATH];
            GetModuleFileName(NULL, szModulePath, MAX_PATH);
            LPCTSTR szModuleName = PathFindFileName(szModulePath);

            if (lstrcmp(szModuleName, L"explorer.exe") == 0)
            {
                //MessageBox(NULL, L"模块注入成功", L"标题", MB_OK);
                StartHook();
            }
        }
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:

        if (nullptr != img)
        {
            delete img;
        }

        if (nullptr != logger)
        {
            delete logger;
        }

        g_mapShellWindow.clear();
        g_bMonitor = FALSE;
        break;
    }
    return TRUE;
}

#include "pch.h"
#include "shellhook.h"

HWND MyCreateWindowExW(DWORD dwExStyle, LPCWSTR lpClassName, LPCWSTR lpWindowName, DWORD dwStyle, int X, int Y, int nWidth, int nHeight, HWND hWndParent, HMENU hMenu, HINSTANCE hInstance, LPVOID lpParam)
{
     HWND hwnd = InitCreateWindowExW(dwExStyle, lpClassName, lpWindowName,
         dwStyle, X, Y, nWidth, nHeight, hWndParent,
         hMenu, hInstance, lpParam);

     TCHAR szClassName[260]{};
     TCHAR szParentClassName[260]{};
     GetClassName(hwnd, szClassName, 260);
     GetClassName(hWndParent, szParentClassName, 260);


     if (lstrcmp(szClassName, L"DirectUIHWND") == 0
         && lstrcmp(szParentClassName, L"SHELLDLL_DefView") == 0)
     {
         HWND hWndGranp = GetParent(hWndParent);
         TCHAR szGranpClassName[260]{};
         GetClassName(hWndGranp, szGranpClassName, 260);

         if (lstrcmp(szGranpClassName, L"ShellTabWindowClass") == 0)
         {
             ShellWindow shellwindow;
             shellwindow.hwnd = hwnd;
             g_mapShellWindow[GetCurrentThreadId()] = shellwindow;
         }
     }

     return hwnd;
}

BOOL MyDestroyWindow(HWND hwnd)
{
    std::unordered_map<DWORD, ShellWindow>::iterator iter = g_mapShellWindow.find(GetCurrentThreadId());

    if (iter != g_mapShellWindow.end() && hwnd == iter->second.hwnd)
    {
        g_mapShellWindow.erase(iter);
    }

    return InitDestroyWindow(hwnd);
}

HDC MyBeginPaint(HWND hWnd, LPPAINTSTRUCT lpPaint)
{
    HDC hdc =  InitBeginPaint(hWnd, lpPaint);

    auto iter = g_mapShellWindow.find(GetCurrentThreadId());

    if (iter != g_mapShellWindow.end() && hWnd == iter->second.hwnd)
    {
        iter->second.hdc = hdc;
    }

    return hdc;
}

int MyFillRect(HDC hDC, RECT* lprc, HBRUSH hbr)
{
    int ret =  InitFillRect(hDC, lprc, hbr);

    auto iter = g_mapShellWindow.find(GetCurrentThreadId());

    if (iter != g_mapShellWindow.end() && iter->second.hdc == hDC)
    {
        ShellWindow shellWindow = iter->second;
        RECT pRc{};
        GetWindowRect(shellWindow.hwnd, &pRc);
        SIZE wndSize = { pRc.right - pRc.left,pRc.bottom - pRc.top };

        if (shellWindow.size.cx != wndSize.cx ||
            shellWindow.size.cy != wndSize.cy)
        {
            InvalidateRect(shellWindow.hwnd, 0, TRUE);
        }

        SaveDC(hDC);
        IntersectClipRect(hDC, lprc->left, lprc->top, lprc->right, lprc->bottom);

        POINT pos;
        SIZE dstSize = { img->size.cx,img->size.cy };
        pos.x = 0;
        pos.y = 0;

        BLENDFUNCTION bf = { AC_SRC_OVER, 0, img->opactiy, AC_SRC_ALPHA };
        AlphaBlend(hDC, pos.x, pos.y, dstSize.cx, dstSize.cy, img->hdc, 0, 0, img->size.cx, img->size.cy, bf);

        RestoreDC(hDC, -1);

        iter->second.size = wndSize;
    }
    return ret;
}

HDC MyCreateCompatibleDC(HDC hDc)
{
    HDC retHdc =  InitCreateCompatibleDC(hDc);

    auto iter = g_mapShellWindow.find(GetCurrentThreadId());
    if (iter != g_mapShellWindow.end() && iter->second.hdc == hDc)
    {
        iter->second.hdc = retHdc;
    }
    return retHdc;
}

VOID StartHook()
{
    Gdiplus::GdiplusStartupInput gdiStartupInput;
    auto ret = Gdiplus::GdiplusStartup(&g_lGdiPlusToken, &gdiStartupInput, NULL);

    if (Gdiplus::Status::Ok != ret)
        return;

    if (MH_Initialize() != MH_OK)
        return;

    logger = new shellhook::logger();
    logger->SetEnable(true);
    logger->SetLogFilePath("D:\\shellhook.log");
    logger->WriteLog("start hook......");

    img = new BitmapImage(L"res\\explorerbg.jpg");

    MH_CreateHookEx(&CreateWindowExW, &MyCreateWindowExW, &InitCreateWindowExW);
    MH_CreateHookEx(&DestroyWindow, &MyDestroyWindow, &InitDestroyWindow);
    MH_CreateHookEx(&BeginPaint, &MyBeginPaint, &InitBeginPaint);
    MH_CreateHookEx(&FillRect, &MyFillRect, &InitFillRect);
    MH_CreateHookEx(&CreateCompatibleDC, &MyCreateCompatibleDC, &InitCreateCompatibleDC);

    MH_EnableHook(MH_ALL_HOOKS);
}


