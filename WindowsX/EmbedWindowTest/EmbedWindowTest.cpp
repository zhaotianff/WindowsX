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

#pragma comment(lib, "user32.lib")
#pragma comment(lib, "gdiplus.lib")

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




// ------------------------------------------------------------------
// 查找当前可用的壁纸层 WorkerW
// 兼容两种旧版布局：
//   布局 A：Progman 下有子 WorkerW
//   布局 B：DefView 父窗口之后有顶层 WorkerW
// ------------------------------------------------------------------
void FindWallpaperWorkerW()
{
    g_hProgman = FindWindow(L"Progman", L"Program Manager");
    if (g_hProgman == nullptr)
        return;

    // 触发系统创建/重组 WorkerW
    SendMessageTimeout(g_hProgman, WM_SPAWN_WORKER, 0xD, 0x1,
        SMTO_NORMAL, 1000, nullptr);
    Sleep(100);

    // 找到当前承载桌面图标的顶层窗口（通常是 Progman 或某个 WorkerW）
    HWND hDefViewParent = nullptr;
    EnumWindows(EnumDefViewParentProc, NULL);
}

BOOL EmbedWindowToDesktop(HWND hWorkerW, HWND hEmbed)
{
    g_hEmbedHwnd = hEmbed;

    // 3) 去掉 WS_POPUP，改为 WS_CHILD（子窗口才能嵌入父窗口）
    LONG_PTR style = GetWindowLongPtr(hEmbed, GWL_STYLE);
    style &= ~WS_POPUP;
    style |= WS_CHILD;
    SetWindowLongPtr(hEmbed, GWL_STYLE, style);

    // 4) 清除顶层相关的扩展样式，避免成为独立任务栏窗口
    LONG_PTR exStyle = GetWindowLongPtr(hEmbed, GWL_EXSTYLE);
    exStyle &= ~(WS_EX_APPWINDOW | WS_EX_TOOLWINDOW);
    SetWindowLongPtr(hEmbed, GWL_EXSTYLE, exStyle);

    // 5) 嵌入到 WorkerW（壁纸层）
    SetParent(hEmbed, hWorkerW);

    // 6) 铺满主屏（SWP_FRAMECHANGED 使样式修改立即生效）
    SetWindowPos(hEmbed, nullptr,
        0, 0,
        GetSystemMetrics(SM_CXSCREEN),
        GetSystemMetrics(SM_CYSCREEN),
        SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED | SWP_SHOWWINDOW);

    // 7) 确保可见
    ShowWindow(hEmbed, SW_SHOW);

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
    /*if (hDetachHwnd == nullptr)
        return;

    SetParent(hDetachHwnd, nullptr);

    LONG_PTR style = GetWindowLongPtr(hDetachHwnd, GWL_STYLE);
    style &= ~WS_CHILD;
    style |= WS_POPUP;
    SetWindowLongPtr(hDetachHwnd, GWL_STYLE, style);

    SetWindowPos(hDetachHwnd, nullptr, 0, 0, 800, 600,
        SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED);*/

    SendMessage(hDetachHwnd, WM_CLOSE, NULL, NULL);
    SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, NULL, SPIF_UPDATEINIFILE);
}

//Not working
void DetachFromDesktop()
{
    DetachFromDesktop(g_hEmbedHwnd);
}

int main()
{
    //std::vector<std::wstring> arguements = {L"WindowsX.DynamicDesktop.exe",L"",LR"(C:\Users\xi\Videos\2026-03-10 16-26-42.mkv)"};

    
    //CreateProcess(NULL,&arguements[0],NULL,NULL,FALSE,)

    EmbedWindowToDesktop(L"MainWindow");

    DetachFromDesktop();

    //Not working
    //SendMessage(g_hEmbedHwnd, WM_CLOSE, NULL, NULL);
}
