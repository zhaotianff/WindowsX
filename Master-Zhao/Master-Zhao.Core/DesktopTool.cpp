#include "DesktopTool.h"

struct tagBASICWINDOWINFO
{
	HWND hwnd;
	TCHAR szName[260];
};

HWND hEmbedHwnd;
std::vector<tagBASICWINDOWINFO> lstWindows;

BOOL SetBackground(LPTSTR lpImagePath)
{
	return SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, (PVOID)lpImagePath, SPIF_UPDATEINIFILE);
}

//调用前分配空间
BOOL GetBackground(LPTSTR lpImagePath)
{
	return SystemParametersInfo(SPI_GETDESKWALLPAPER, MAX_PATH, (PVOID)lpImagePath, 0);
}

//计算机\HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers
BOOL GetRecentBackground(LPTSTR lpRecentPath)
{
	HKEY hKey;
	DWORD length;
	TCHAR buf[1024]{};
	TCHAR path[MAX_PATH]{};
	std::vector<std::wstring> list;
	auto result = RegOpenKeyEx(HKEY_CURRENT_USER, L"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Wallpapers", 0, KEY_ALL_ACCESS, &hKey);
	if (ERROR_SUCCESS == result)
	{
		for (int i = 0;i < 5;i++)
		{
			length = 1024;
			wsprintf(path, L"BackgroundHistoryPath%d", i);
			result = RegQueryValueEx(hKey, path, 0, NULL, (BYTE*)buf, &length);
			if (ERROR_SUCCESS == result)
			{
				lstrcat(lpRecentPath, buf);
				lstrcat(lpRecentPath, L";");
			}
		}

		RegCloseKey(hKey);
	}

	return TRUE;
}


BOOL EmbedWindowToDesktop(LPCTSTR lpWindowName)
{
	Sleep(1000);
	HWND hProgman = FindWindow(L"Progman", L"Program Manager");
	if (hProgman == NULL)
	{
		return FALSE;
	}

	SendMessage(hProgman, WM_SPAWN_WORKER, NULL, NULL);

	lstWindows.clear();
	EnumWindows(EnumWindowProc, 0);

	for (size_t i = lstWindows.size() -1;i >1 ; i--) 
	{
		tagBASICWINDOWINFO bwiCurrent = lstWindows[i];
		tagBASICWINDOWINFO bwiNext = lstWindows[i - 1];
		if (wcsncmp(lstWindows[i].szName, L"WorkerW", lstrlen(lstWindows[i].szName)) == 0)
		{
			HWND hShellDefView = FindWindowEx(bwiCurrent.hwnd, NULL, L"SHELLDLL_DefView", NULL);
			if (hShellDefView == NULL) 
			{
				SendMessage(bwiCurrent.hwnd, WM_CLOSE, NULL, NULL);
				break;
			}
			else 
			{	
				if (bwiNext.szName == L"Progman") 
				{
					HWND hWorkerW = FindWindowEx(bwiNext.hwnd, NULL, L"WorkerW", NULL);	
					if (hWorkerW == NULL) 
					{	
						break;
					}
					else 
					{	
						SendMessage(hWorkerW, WM_CLOSE, NULL, NULL);
					}
				}
				else 
				{
					SendMessage(bwiNext.hwnd, WM_CLOSE, NULL, NULL);
				}
			}
		}	
	}

	hEmbedHwnd = FindWindow(NULL, lpWindowName);

	if (hEmbedHwnd == NULL)
	{
		return FALSE;
	}

	SetParent(hEmbedHwnd, hProgman);
	ShowWindow(hEmbedHwnd, SW_MAXIMIZE);
	return true;
}

BOOL CloseEmbedWindow()
{
	if (hEmbedHwnd)
	{
		SendMessage(hEmbedHwnd, WM_CLOSE, NULL, NULL);
		return TRUE;
	}

	return FALSE;
}

VOID SwitchToDesktop()
{
	CoInitialize(NULL);
	IShellDispatch4* pShellDisp = NULL;
	HRESULT sc = CoCreateInstance(CLSID_Shell, NULL, CLSCTX_SERVER, IID_IDispatch, (LPVOID*)&pShellDisp);
	sc = pShellDisp->ToggleDesktop();
	sc = pShellDisp->ToggleDesktop();
	pShellDisp->Release();
}

VOID SwitchToWindow(HWND hwnd)
{
	ShowWindow(hwnd, SW_SHOW);
	SetForegroundWindow(hwnd);
}

BOOL CALLBACK EnumWindowProc(HWND hwnd, LPARAM lParam)
{
	tagBASICWINDOWINFO bwi{};
	memset(bwi.szName, 0, sizeof(bwi.szName));
	GetClassName(hwnd, bwi.szName, sizeof(bwi.szName) / sizeof(TCHAR));
	bwi.hwnd = hwnd;
	lstWindows.push_back(bwi);
	return TRUE;
}

HBITMAP GetFileThumbnail(PCWSTR path)
{
	HRESULT hr = CoInitialize(nullptr);

	IShellItem* item = nullptr;
	hr = SHCreateItemFromParsingName(path, nullptr, IID_PPV_ARGS(&item));

	IThumbnailCache* cache = nullptr;
	hr = CoCreateInstance(
		CLSID_LocalThumbnailCache,
		nullptr,
		CLSCTX_INPROC,
		IID_PPV_ARGS(&cache));

	WTS_CACHEFLAGS flags = WTS_LOWQUALITY;
	ISharedBitmap* shared_bitmap;
	hr = cache->GetThumbnail(
		item,
		12*16,
		WTS_EXTRACT,
		&shared_bitmap,
		nullptr,
		nullptr);

	HBITMAP hbitmap = NULL;
	hr = shared_bitmap->GetSharedBitmap(&hbitmap);
	CoUninitialize();

	return hbitmap;
}

BOOL CenterStartMenu(BOOL enable)
{
	//TODO get taskbar height
	HWND hDesktop = GetDesktopWindow();
	RECT rectDesktop{}, rectShellTrayWnd{}, rectNotify{}, rectStart{};
	GetWindowRect(hDesktop, &rectDesktop);
	HWND hShell_TrayWnd = FindWindow(L"Shell_TrayWnd", NULL);
	GetWindowRect(hShell_TrayWnd, &rectShellTrayWnd);
	POINT point{ 0, rectDesktop.bottom - rectShellTrayWnd.bottom + 1};
	HWND hTrayNotifyWnd = FindWindowEx(hShell_TrayWnd, NULL, L"TrayNotifyWnd", NULL);
	GetClientRect(hTrayNotifyWnd, &rectNotify);
	HWND hStart = FindWindowEx(hShell_TrayWnd, NULL, L"Start", NULL);
	GetClientRect(hStart, &rectStart);
	keybd_event(VK_LWIN, 0x45, NULL, NULL);
	keybd_event(VK_LWIN, 0x45, KEYEVENTF_KEYUP, NULL);

	Sleep(100);
	HWND hwnd = WindowFromPoint(point);

	//center 
	int x = 0;

	if (hwnd)
	{
		if (enable)
		{
			x = (rectDesktop.right - rectNotify.right - rectStart.right) / 2 - rectStart.right;
		}
		else
		{
			x = 0;
		}

		SetWindowPos(hStart, NULL, x, 0, 0, 0, SWP_NOSIZE | SWP_ASYNCWINDOWPOS | SWP_NOACTIVATE | SWP_NOZORDER | SWP_SHOWWINDOW);	
		SetWindowPos(hwnd, NULL, x, 0, 0, 0, SWP_NOSIZE | SWP_ASYNCWINDOWPOS | SWP_NOACTIVATE | SWP_NOZORDER | SWP_SHOWWINDOW);
		Sleep(10);	
	}

	Sleep(100);
	keybd_event(VK_LWIN, 0x45, NULL, NULL);
	keybd_event(VK_LWIN, 0x45, KEYEVENTF_KEYUP, NULL);
	return TRUE;
}

BOOL CenterTaskListIcon(BOOL enable)
{
	HWND hShell_TrayWnd = FindWindow(L"Shell_TrayWnd", NULL);
	if (hShell_TrayWnd)
	{
		HWND hRegBar32 = FindWindowEx(hShell_TrayWnd, NULL, L"ReBarWindow32", NULL);
		if (hRegBar32)
		{
			HWND hMsTask = FindWindowEx(hRegBar32, NULL, L"MSTaskSwWClass", NULL);
			if (hMsTask)
			{
				HWND hStart = FindWindowEx(hShell_TrayWnd, NULL, L"Start", NULL);

				RECT rectStart{},rectTaskSw;
				GetWindowRect(hStart, &rectStart);
				GetWindowRect(hMsTask, &rectTaskSw);
				int x = 0;

				if (enable)
				{
					x = (rectTaskSw.right - rectTaskSw.left) / 2 - (rectStart.right - rectStart.left);
				}
				else
				{
					x = 0;
				}
				SetWindowPos(hMsTask, NULL, x, 0, 0, 0, SWP_NOSIZE | SWP_ASYNCWINDOWPOS | SWP_NOACTIVATE | SWP_NOZORDER | SWP_SHOWWINDOW);
			}
		}
	}

	return TRUE;
}


BOOL BlurTaskbar(ACCENT_POLICY accent_policy)
{
	HMODULE hModule = LoadLibraryA("user32.dll");
	BOOL bResult = FALSE;

	if (hModule)
	{
		PFN_SET_WINDOW_COMPOSITION_ATTRIBUTE SetWindowCompositionAttribute = (PFN_SET_WINDOW_COMPOSITION_ATTRIBUTE)GetProcAddress(hModule, "SetWindowCompositionAttribute");
		//ACCENT_POLICY policy = {ACCENT_ENABLE_BLURBEHIND,0,RGB(0,0,0),0};
		WINDOWCOMPOSITIONATTRIBDATA data = { WCA_ACCENT_POLICY,&accent_policy,sizeof(accent_policy) };

		HWND hwnd = FindWindow(L"Shell_TrayWnd", NULL);

		if (hwnd)
		{
			bResult = SetWindowCompositionAttribute(hwnd, &data);
		}
		FreeLibrary(hModule);
	}
	return bResult;

}

VOID SetDesktopIcon(DESKTOPICONS icon, BOOL bEnable)
{

}

BOOL GetDesktopIconState(DESKTOPICONS icon)
{
	switch (icon)
	{
		case ICON_COMPUTER:
			break;
		case ICON_USER:
			break;
		case ICON_RECYCLE:
			break;
		case ICON_CONTROL_PANEL:
			break;
		case ICON_NETWORK:
			break;
		default:
			break;
	}
	return TRUE;
}

VOID RefreshDesktop()
{
	//https://docs.microsoft.com/en-us/windows/win32/api/shlobj_core/nf-shlobj_core-shchangenotify
	//Notifies the system of an event that an application has performed. 
	//An application should use this function if it performs an action that may affect the Shell.
	SHChangeNotify(0x8000000, 0x1000, NULL,NULL);
}
