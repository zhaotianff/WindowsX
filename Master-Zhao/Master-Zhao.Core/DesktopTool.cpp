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

BOOL SetBackgroundPosition(DESKTOP_WALLPAPER_POSITION pos)
{
	CoInitialize(NULL);

	IDesktopWallpaper* pDesktopWallpaper = NULL;
	HRESULT sc = CoCreateInstance(CLSID_DesktopWallpaper, NULL, CLSCTX_ALL, IID_IDesktopWallpaper, (LPVOID*)&pDesktopWallpaper);

	if (SUCCEEDED(sc))
	{
		sc = pDesktopWallpaper->SetPosition(DESKTOP_WALLPAPER_POSITION::DWPOS_TILE);
		pDesktopWallpaper->Release();
	}

	CoUninitialize();
	return SUCCEEDED(sc);
}

//调用前分配空间
BOOL GetBackground(LPTSTR lpImagePath)
{
	return SystemParametersInfo(SPI_GETDESKWALLPAPER, MAX_PATH, (PVOID)lpImagePath, 0);
}

BOOL GetBackgroundPosition(DESKTOP_WALLPAPER_POSITION* pos)
{
	CoInitialize(NULL);

	IDesktopWallpaper* pDesktopWallpaper = NULL;
	HRESULT sc = CoCreateInstance(CLSID_DesktopWallpaper, NULL, CLSCTX_ALL, IID_IDesktopWallpaper, (LPVOID*)&pDesktopWallpaper);

	if (SUCCEEDED(sc))
	{
		sc = pDesktopWallpaper->GetPosition(pos);
		pDesktopWallpaper->Release();
	}

	CoUninitialize();
	return SUCCEEDED(sc);
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

VOID RestartExplorer()
{
	DWORD dwPID;
	HWND hShellTray = ::FindWindow(TEXT("Shell_TrayWnd"), NULL);
	GetWindowThreadProcessId(hShellTray, &dwPID);
	HANDLE hExplorer;
	hExplorer = OpenProcess(PROCESS_TERMINATE, false, dwPID);
	//restart explorer
	TerminateProcess(hExplorer, 2);
	CloseHandle(hExplorer);
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
	const wchar_t* szSubKeyPath = L"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\HideDesktopIcons\\NewStartPanel";
	DWORD dwEnable = bEnable == TRUE ? 0 : 1;
	switch (icon)
	{
		case DESKTOPICONS::ICON_COMPUTER:
			SetDWORDValue(HKEY_CURRENT_USER, szSubKeyPath, L"{20D04FE0-3AEA-1069-A2D8-08002B30309D}", dwEnable);
			break;
		case DESKTOPICONS::ICON_USER:
			SetDWORDValue(HKEY_CURRENT_USER, szSubKeyPath, L"{59031a47-3f72-44a7-89c5-5595fe6b30ee}", dwEnable);
			break;
		case DESKTOPICONS::ICON_RECYCLE:
			SetDWORDValue(HKEY_CURRENT_USER, szSubKeyPath, L"{645FF040-5081-101B-9F08-00AA002F954E}", dwEnable);
			break;
		case DESKTOPICONS::ICON_CONTROL_PANEL:
			SetDWORDValue(HKEY_CURRENT_USER, szSubKeyPath, L"{5399E694-6CE5-4D6C-8FCE-1D8870FDCBA0}", dwEnable);
			break;
		case DESKTOPICONS::ICON_NETWORK:
			SetDWORDValue(HKEY_CURRENT_USER, szSubKeyPath, L"{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}", dwEnable);
			break;
		default:
			break;
	}
}

BOOL GetDesktopIconState(DESKTOPICONS icon)
{
	const wchar_t* szSubKeyPath = L"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\HideDesktopIcons\\NewStartPanel";
	DWORD dwState = 0;
	switch (icon)
	{
		case DESKTOPICONS::ICON_COMPUTER:
			QueryDWORDValue(HKEY_CURRENT_USER, szSubKeyPath, L"{20D04FE0-3AEA-1069-A2D8-08002B30309D}", &dwState);
			break;
		case DESKTOPICONS::ICON_USER:
			QueryDWORDValue(HKEY_CURRENT_USER, szSubKeyPath, L"{59031a47-3f72-44a7-89c5-5595fe6b30ee}", &dwState);
			break;
		case DESKTOPICONS::ICON_RECYCLE:
			QueryDWORDValue(HKEY_CURRENT_USER, szSubKeyPath, L"{645FF040-5081-101B-9F08-00AA002F954E}", &dwState);
			break;
		case DESKTOPICONS::ICON_CONTROL_PANEL:
			QueryDWORDValue(HKEY_CURRENT_USER, szSubKeyPath, L"{5399E694-6CE5-4D6C-8FCE-1D8870FDCBA0}", &dwState);
			break;
		case DESKTOPICONS::ICON_NETWORK:
			QueryDWORDValue(HKEY_CURRENT_USER, szSubKeyPath, L"{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}", &dwState);
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

//from msdn 
//https://docs.microsoft.com/en-us/windows/win32/shell/links?redirectedfrom=MSDN#creating-a-shortcut-and-a-folder-shortcut-to-a-file
HRESULT CreateLink(LPCWSTR lpszPathObj, LPCTSTR lpszPathLink, LPCTSTR lpszArgs,LPCWSTR lpszDesc)
{
	HRESULT hres;
	IShellLink* psl;

	// Get a pointer to the IShellLink interface. It is assumed that CoInitialize
	// has already been called.
	hres = CoCreateInstance(CLSID_ShellLink, NULL, CLSCTX_INPROC_SERVER, IID_IShellLink, (LPVOID*)&psl);
	if (SUCCEEDED(hres))
	{
		IPersistFile* ppf;

		// Set the path to the shortcut target
		// set arguments
		// set description
		psl->SetPath(lpszPathObj);
		psl->SetArguments(lpszArgs);
		psl->SetDescription(lpszDesc);

		// Query IShellLink for the IPersistFile interface, used for saving the 
		// shortcut in persistent storage. 
		hres = psl->QueryInterface(IID_IPersistFile, (LPVOID*)&ppf);

		if (SUCCEEDED(hres))
		{
			// Save the link by calling IPersistFile::Save. 
			hres = ppf->Save(lpszPathLink, TRUE);
			ppf->Release();
		}
		psl->Release();
	}
	return hres;
}

BOOL GetGodModeShortCutState()
{
	WIN32_FIND_DATA ffd;
	HANDLE hFind = INVALID_HANDLE_VALUE;

	TCHAR szDesktopPath[MAX_PATH]{};
	SHGetFolderPath(NULL, CSIDL_DESKTOP, NULL, SHGFP_TYPE_CURRENT, szDesktopPath);
	StringCchCat(szDesktopPath, MAX_PATH, L"\\god mode.lnk");

	hFind = FindFirstFile(szDesktopPath, &ffd);

	if (INVALID_HANDLE_VALUE == hFind)
	{
		FindClose(hFind);
		return FALSE;
	}

	FindClose(hFind);
	return TRUE;
}

BOOL CreateGodModeShortCut()
{
	TCHAR szExplorer[MAX_PATH]{};
	TCHAR szGodmodLnk[MAX_PATH]{};
	SHGetFolderPath(NULL, CSIDL_WINDOWS, NULL, SHGFP_TYPE_CURRENT, szExplorer);
	StringCchCat(szExplorer, MAX_PATH, L"\\explorer.exe");
	SHGetFolderPath(NULL, CSIDL_DESKTOP, NULL, SHGFP_TYPE_CURRENT, szGodmodLnk);
	StringCchCat(szGodmodLnk, MAX_PATH, L"\\god mode.lnk");
	auto hr = CreateLink(szExplorer, szGodmodLnk, L"shell:::{ED7BA470-8E54-465E-825C-99712043E01C}", L"god mode");
	return SUCCEEDED(hr);
}

BOOL RemoveGodModeShortCut()
{
	TCHAR szGodmodLnk[MAX_PATH]{};
	SHGetFolderPath(NULL, CSIDL_DESKTOP, NULL, SHGFP_TYPE_CURRENT, szGodmodLnk);
	StringCchCat(szGodmodLnk, MAX_PATH, L"\\god mode.lnk");
	return DeleteFile(szGodmodLnk);
}

BOOL RemoveShortcutArrow()
{
	auto bResult = RemovRegValue(HKEY_CLASSES_ROOT, L"lnkfile", L"IsShortcut");
	if (bResult)
		RefreshDesktop();
	return bResult;
}

BOOL RestoreShortcutArrow()
{
	auto bResult =  SetSZValue(HKEY_CLASSES_ROOT, L"lnkfile", L"IsShortcut", NULL);
	if (bResult)
		RefreshDesktop();
	return bResult;
}

VOID RegisterWindowsPhotoViewerFormat()
{
	LPTSTR szTiff = _tcsdup(L"PhotoViewer.FileAssoc.Tiff");
	SetSZValue(HKEY_LOCAL_MACHINE, LR"(Software\Microsoft\Windows PhotoViewer\Capabilities\FileAssociations)",L".bmp", szTiff);
	SetSZValue(HKEY_LOCAL_MACHINE, LR"(Software\Microsoft\Windows PhotoViewer\Capabilities\FileAssociations)", L".jpeg", szTiff);
	SetSZValue(HKEY_LOCAL_MACHINE, LR"(Software\Microsoft\Windows PhotoViewer\Capabilities\FileAssociations)", L".jpg", szTiff);
	SetSZValue(HKEY_LOCAL_MACHINE, LR"(Software\Microsoft\Windows PhotoViewer\Capabilities\FileAssociations)", L".png", szTiff);
	SetSZValue(HKEY_LOCAL_MACHINE, LR"(Software\Microsoft\Windows PhotoViewer\Capabilities\FileAssociations)", L".ico", szTiff);
	SetSZValue(HKEY_LOCAL_MACHINE, LR"(Software\Microsoft\Windows PhotoViewer\Capabilities\FileAssociations)", L".gif", szTiff);
	SetSZValue(HKEY_LOCAL_MACHINE, LR"(Software\Microsoft\Windows PhotoViewer\Capabilities\FileAssociations)", L".tiff", szTiff);
	free(szTiff);
}

VOID UnregisterWindowsPhotoViewerFormat()
{
	LPTSTR szTiff = _tcsdup(L"PhotoViewer.FileAssoc.Tiff");
	RemovRegValue(HKEY_LOCAL_MACHINE, LR"(Software\Microsoft\Windows PhotoViewer\Capabilities\FileAssociations)", L".bmp");
	RemovRegValue(HKEY_LOCAL_MACHINE, LR"(Software\Microsoft\Windows PhotoViewer\Capabilities\FileAssociations)", L".bmp");
	RemovRegValue(HKEY_LOCAL_MACHINE, LR"(Software\Microsoft\Windows PhotoViewer\Capabilities\FileAssociations)", L".bmp");
	RemovRegValue(HKEY_LOCAL_MACHINE, LR"(Software\Microsoft\Windows PhotoViewer\Capabilities\FileAssociations)", L".bmp");
	RemovRegValue(HKEY_LOCAL_MACHINE, LR"(Software\Microsoft\Windows PhotoViewer\Capabilities\FileAssociations)", L".bmp");
	RemovRegValue(HKEY_LOCAL_MACHINE, LR"(Software\Microsoft\Windows PhotoViewer\Capabilities\FileAssociations)", L".bmp");
	RemovRegValue(HKEY_LOCAL_MACHINE, LR"(Software\Microsoft\Windows PhotoViewer\Capabilities\FileAssociations)", L".bmp");
	RemovRegValue(HKEY_LOCAL_MACHINE, LR"(Software\Microsoft\Windows PhotoViewer\Capabilities\FileAssociations)", L".bmp");
	RemovRegValue(HKEY_LOCAL_MACHINE, LR"(Software\Microsoft\Windows PhotoViewer\Capabilities\FileAssociations)", L".bmp");
	RemovRegValue(HKEY_LOCAL_MACHINE, LR"(Software\Microsoft\Windows PhotoViewer\Capabilities\FileAssociations)", L".bmp");
	free(szTiff);
}

BOOL PaintVersionInfo(BOOL enable)
{
	//HKEY_CURRENT_USER\Control Panel\Desktop\PaintDesktopVersion(0x1)
	DWORD dwValue = 1;

	if (enable == FALSE)
	{
		dwValue = 0;
	}

	return SetDWORDValue(HKEY_CURRENT_USER, LR"(Control Panel\Desktop)", L"PaintDesktopVersion", dwValue);
}

BOOL SetTaskbarThumbnailSize(DWORD dwSize,BOOL bRestartExplorer)
{
	BOOL bResult = FALSE;
	if (dwSize == RESET_TASKBARTHUMB)
	{
		bResult = RemovRegValue(HKEY_CURRENT_USER, TASKBAR_THUMB_SIZE_REGPATH, TASKBAR_THUMB_SIZE);
	}
	else
	{
		bResult = SetDWORDValue(HKEY_CURRENT_USER, TASKBAR_THUMB_SIZE_REGPATH, TASKBAR_THUMB_SIZE, dwSize);
	}

	if (bRestartExplorer)
	{
		RestartExplorer();
	}
	return bResult;
	
}

