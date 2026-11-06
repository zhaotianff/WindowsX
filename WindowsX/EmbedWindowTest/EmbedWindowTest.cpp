// EmbedWindowTest.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//

#ifndef UNICODE
#define UNICODE
#endif
#define _WIN32_WINNT 0x0A00

#include <windows.h>
#include <objidl.h>
#include <gdiplus.h>
#include<iostream>
#include<vector>
#include<tchar.h>
#include<string>
#include<strsafe.h>
#include<pathcch.h>

#pragma comment(lib, "user32.lib")
#pragma comment(lib, "gdiplus.lib")
#pragma comment(lib,"Pathcch.lib")

constexpr UINT WM_SPAWN_WORKER = 0x052C;

static HWND g_hWorkerW = nullptr;   // 找到的壁纸层 WorkerW
static HWND g_hEmbedHwnd = nullptr; // 被嵌入的窗口
static HWND g_hSHELLDLL_DefView = nullptr;
static HWND g_hProgman = nullptr;

BOOL CALLBACK EnumDefViewParentProc(HWND hwnd, LPARAM lParam)
{
    if (FindWindowEx(hwnd, nullptr, L"SHELLDLL_DefView", nullptr) != nullptr)
    {

        // 情况 A：Progman 下有子 WorkerW（Win10 早期 / 部分补丁）
        g_hWorkerW = FindWindowEx(nullptr, hwnd, L"WorkerW", nullptr);
        if (g_hWorkerW != nullptr)
        {
            g_hSHELLDLL_DefView = hwnd;
            return FALSE;
        }

        // 情况 B：DefView 父窗口之后的顶层 WorkerW（Win10 后期 / Win11 旧版）
        g_hWorkerW = FindWindowEx(g_hProgman, nullptr, L"WorkerW", nullptr);

        if (g_hWorkerW != nullptr)
        {
            return FALSE;
        }

        return FALSE; 
    }
    return TRUE;
}


void FindWallpaperWorkerW()
{
    g_hProgman = FindWindow(L"Progman", L"Program Manager");
    if (g_hProgman == nullptr)
        return;

    //触发系统创建/重组 WorkerW
    SendMessageTimeout(g_hProgman, WM_SPAWN_WORKER, 0xD, 0x1,
        SMTO_NORMAL, 1000, nullptr);
    Sleep(1000);

    //找到当前承载桌面图标的顶层窗口（通常是 Progman 或某个 WorkerW）
    HWND hDefViewParent = nullptr;
    EnumWindows(EnumDefViewParentProc, NULL);
}

BOOL EmbedWindowToDesktop(HWND hWorkerW, HWND hEmbed)
{
    g_hEmbedHwnd = hEmbed;

    //去掉 WS_POPUP，改为 WS_CHILD（子窗口才能嵌入父窗口）
   /* LONG_PTR style = GetWindowLongPtr(hEmbed, GWL_STYLE);
    style &= ~WS_POPUP;
    style |= WS_CHILD;
    SetWindowLongPtr(hEmbed, GWL_STYLE, style);*/

    //清除顶层相关的扩展样式，避免成为独立任务栏窗口
    /*LONG_PTR exStyle = GetWindowLongPtr(hEmbed, GWL_EXSTYLE);
    exStyle &= ~(WS_EX_APPWINDOW | WS_EX_TOOLWINDOW);
    SetWindowLongPtr(hEmbed, GWL_EXSTYLE, exStyle);*/

    SetParent(hEmbed, hWorkerW);

   /* SetWindowPos(hEmbed, nullptr,
        0, 0,
        GetSystemMetrics(SM_CXSCREEN),
        GetSystemMetrics(SM_CYSCREEN),
        SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED | SWP_SHOWWINDOW);*/

    ShowWindow(hEmbed, SW_MAXIMIZE);

    return TRUE;
}


BOOL EmbedWindowToDesktop(LPCTSTR lpWindowName)
{
    FindWallpaperWorkerW();
    if (g_hWorkerW == nullptr)
        return FALSE;

    HWND hEmbed = FindWindow(nullptr, lpWindowName);
    if (hEmbed == nullptr)
        return FALSE;
    
    return EmbedWindowToDesktop(g_hWorkerW, hEmbed);
}

void DetachFromDesktop(HWND hDetachHwnd)
{
    SendMessage(hDetachHwnd, WM_CLOSE, NULL, NULL);
    SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, NULL, SPIF_UPDATEINIFILE);
}

//Not working
void DetachFromDesktop()
{
    DetachFromDesktop(g_hEmbedHwnd);
}

BOOL GetAppDirectoryW(LPWSTR szExeDir, DWORD cchExeDir)
{
    if (szExeDir == nullptr || cchExeDir == 0)
        return FALSE;

    ZeroMemory(szExeDir, cchExeDir * sizeof(WCHAR));

    DWORD dwRet = GetModuleFileNameW(NULL, szExeDir, cchExeDir);
    if (dwRet == 0 || dwRet >= cchExeDir)
    {
        return FALSE;
    }

    // 去掉exe文件名，保留目录
    HRESULT hr = PathCchRemoveFileSpec(szExeDir, cchExeDir);
    
    return SUCCEEDED(hr);
}

int main()
{
    STARTUPINFO sif{ sizeof(sif) };    
    PROCESS_INFORMATION psi{};
    TCHAR szPath[MAX_PATH]{};

    if (GetAppDirectoryW(szPath, _countof(szPath)) == FALSE)
    {
        return 0;
    }
    
    HRESULT hr = StringCchCat(szPath, MAX_PATH, TEXT("\\res\\dynamic wallpaper\\default\\default.mp4"));

    if (FAILED(hr))
        return 0;

    const TCHAR* szArgTemplate = TEXT("\"WindowsX.DynamicDesktop.exe\" \"\" \"%s\"");
    TCHAR szArguments[1024]{}; 

    StringCchPrintf(szArguments, _countof(szArguments), szArgTemplate, szPath);
    CreateProcess(NULL, szArguments, NULL, NULL, FALSE, 0, NULL, NULL, &sif, &psi);


    if (psi.hProcess)
        CloseHandle(psi.hProcess);

    if (psi.hThread)
        CloseHandle(psi.hThread);

    EmbedWindowToDesktop(L"MainWindow");

    Sleep(10000);

    DetachFromDesktop();
}
