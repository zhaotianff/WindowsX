// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.h"
#include "shellhook.h"

HMODULE g_hDllModule = NULL;
ULONG_PTR g_lGdiPlusToken;
std::map<DWORD, ShellWindow> g_lstShellWindow;

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

void EnableWindowBlur(HWND hwnd)
{
    DWM_BLURBEHIND bh{};
    bh.dwFlags = DWM_BB_ENABLE;
    bh.fEnable = TRUE;
    bh.hRgnBlur = NULL;
    bh.fTransitionOnMaximized = TRUE;

    DwmEnableBlurBehindWindow(hwnd, &bh);


    //SetWindowLong(hwnd, GWL_EXSTYLE, GetWindowLong(hwnd, GWL_EXSTYLE) - WS_EX_DLGMODALFRAME);
    //SetWindowLong(hwnd, GWL_EXSTYLE, GetWindowLong(hwnd, GWL_EXSTYLE) | WS_EX_LAYERED);

    SetLayeredWindowAttributes(hwnd, RGB(128,128,128), 200, LWA_ALPHA);
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
        if (NULL == g_hDllModule)
        {
            g_hDllModule = hModule;
            DisableThreadLibraryCalls(hModule);

            TCHAR szModulePath[MAX_PATH];
            GetModuleFileName(NULL, szModulePath, MAX_PATH);
            LPCTSTR szModuleName = PathFindFileName(szModulePath);

            if (lstrcmp(szModuleName, L"explorer.exe") == 0)
            {
                MessageBox(NULL, L"模块注入成功", L"标题", MB_OK);
                StartHook();
            }
        }
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
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

     TCHAR buf[MAX_PATH]{};
     GetClassName(hwnd, buf, 260);

     if (lstrcmp(buf, L"CabinetWClass") == 0)
     {
         //SetWindowLong(hwnd, GWL_STYLE, 0x16CF0000);
         //SetWindowLong(hwnd, GWL_EXSTYLE, 0x00040100);
         //Sleep(50);
         EnableWindowBlur(hwnd);
         SetWindowBlur(hwnd);
         //MessageBox(NULL, lpWindowName, L"", MB_OK);
     }

     if (lstrcmp(buf, L"NamespaceTreeControl") == 0)
     {
         TCHAR parbuf[MAX_PATH]{};
         GetClassName(GetParent(hwnd), parbuf, 260);

         if (lstrcmp(parbuf, L"CtrlNotifySink") == 0)
         {
             EnableWindowBlur(hwnd);
             SetWindowBlur(hwnd);
             //MessageBox(NULL, parbuf, L"", MB_OK);
         }
     }

     return hwnd;
}

BOOL MyDestroyWindow(HWND hwnd)
{
    return InitDestroyWindow(hwnd);
}

HDC MyBeginPaint(HWND hWnd, LPPAINTSTRUCT lpPaint)
{
    return InitBeginPaint(hWnd, lpPaint);
}

int MyFillRect(HDC hDC, RECT* lprc, HBRUSH hbr)
{
    return InitFillRect(hDC, lprc, hbr);
}

HDC MyCreateCompatibleDC(HDC hdc)
{
    return InitCreateCompatibleDC(hdc);
}

VOID StartHook()
{
    Gdiplus::GdiplusStartupInput gdiStartupInput;
    auto ret = Gdiplus::GdiplusStartup(&g_lGdiPlusToken, &gdiStartupInput, NULL);

    if (Gdiplus::Status::Ok != ret)
        return;

    if (MH_Initialize() != MH_OK)
        return;

    MH_CreateHookEx(&CreateWindowExW, &MyCreateWindowExW, &InitCreateWindowExW);
    MH_CreateHookEx(&DestroyWindow, &MyDestroyWindow, &InitDestroyWindow);
    MH_CreateHookEx(&BeginPaint, &MyBeginPaint, &InitBeginPaint);
    MH_CreateHookEx(&FillRect, &MyFillRect, &InitFillRect);
    MH_CreateHookEx(&CreateCompatibleDC, &MyCreateCompatibleDC, &InitCreateCompatibleDC);

    MH_EnableHook(MH_ALL_HOOKS);
}


