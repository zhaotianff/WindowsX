// header.h: 标准系统包含文件的包含文件，
// 或特定于项目的包含文件
//

#pragma once

#ifdef SILVERAROWANACORE_EXPORTS
#define SILVERAROWANACORE_API __declspec(dllexport)
#else
#define SILVERAROWANACORE_API __declspec(dllimport)
#endif

#define WIN32_LEAN_AND_MEAN             // 从 Windows 头文件中排除极少使用的内容
// Windows 头文件
#include <windows.h>


