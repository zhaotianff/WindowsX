#pragma once

#define WIN32_LEAN_AND_MEAN             // 从 Windows 头文件中排除极少使用的内容
// Windows 头文件
#include <windows.h>
#include<comdef.h>
#include<gdiplus.h>
#include<shlwapi.h>
#include<strsafe.h>
#include<vector>
#include<unordered_map>
#include<dwmapi.h>

#pragma comment(lib, "GdiPlus.lib")

#include "include/MinHook.h"
#include"ShellWindow.h"
#include"BitmapImage.h"

#pragma comment(lib,"Shlwapi.lib")
#pragma comment(lib,"Dwmapi.lib")
