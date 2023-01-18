#include "DesktopTool.h"
#include"InputTool.h"
#include"SystemTool.h"
#include <dwmapi.h>

struct tagBASICWINDOWINFO
{
	HWND hwnd;
	TCHAR szName[260];
};

HWND hEmbedHwnd = NULL;
std::vector<tagBASICWINDOWINFO> lstWindows;
HWND hStartMenu = NULL;
HWND hStartMenuBtn = NULL;

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
	//DWORD dwPID;
	//HWND hShellTray = ::FindWindow(TEXT("Shell_TrayWnd"), NULL);
	//GetWindowThreadProcessId(hShellTray, &dwPID);
	//HANDLE hExplorer;
	//hExplorer = OpenProcess(PROCESS_TERMINATE, false, dwPID);
	////restart explorer
	//TerminateProcess(hExplorer, 2);
	//CloseHandle(hExplorer);
	system("taskkill /f /im explorer.exe & start explorer.exe");

	hStartMenu = NULL;
	hStartMenuBtn = NULL;
}

VOID SelectFile(LPCTSTR lpszFile)
{
	TCHAR szFullPath[MAX_PATH]{};
	MakeFullPath(lpszFile, szFullPath, MAX_PATH);
	TCHAR szCmd[260]{};
	STARTUPINFO si{};
	PROCESS_INFORMATION pi{};
	si.cb = sizeof(si);
	wsprintf(szCmd, L"explorer.exe /e,/select,%s", szFullPath);
	auto nRet = CreateProcess(NULL, szCmd, NULL, NULL, FALSE, 0, NULL, NULL, &si, &pi);

	if (!nRet)
	{
		if (pi.hThread)
		{
			CloseHandle(pi.hThread);
		}

		if (pi.hProcess)
		{
			CloseHandle(pi.hProcess);
		}
	}
}

VOID OpenFileProperty(LPCTSTR lpszFile)
{
	TCHAR szFullPath[MAX_PATH]{};
	MakeFullPath(lpszFile, szFullPath, MAX_PATH);
	SHELLEXECUTEINFO shellInfo{};
	shellInfo.cbSize = sizeof(shellInfo);
	shellInfo.lpVerb = L"properties";
	shellInfo.lpFile = szFullPath;
	shellInfo.fMask = SEE_MASK_INVOKEIDLIST;
	ShellExecuteEx(&shellInfo);
}

VOID OpenWindowsHelp()
{
	SHELLEXECUTEINFO shellInfo{};
	shellInfo.cbSize = sizeof(shellInfo);
	shellInfo.lpVerb = L"open";
	shellInfo.lpFile = L"https://support.microsoft.com/home/contact";
	shellInfo.fMask = SEE_MASK_INVOKEIDLIST;
	ShellExecuteEx(&shellInfo);
}

VOID OpenRunDialog(LPTSTR command)
{
	SendMultiAsciiInput(2, VK_LWIN, 'R');
	Sleep(50);
	//TODO locale 
	HWND hWndRun = FindWindow(NULL, L"运行");

	if (hWndRun)
	{
		HWND hComBox = FindWindowEx(hWndRun, NULL, L"ComboBox", NULL);

		if (hComBox)
		{
			HWND hEdit = FindWindowEx(hComBox, NULL, L"Edit", NULL);

			TCHAR buf[MAX_PATH]{};
			GetWindowText(hEdit, buf, MAX_PATH);
			if (hEdit)
			{
				SetWindowText(hEdit, command);
				GetWindowText(hEdit, buf, MAX_PATH);
				//TODO
			}
		}
	}
}

BOOL CloseEmbedWindow()
{
	if (hEmbedHwnd)
	{
		SendMessage(hEmbedHwnd, WM_CLOSE, NULL, NULL);
		ResetWallpaper();
		return TRUE;
	}

	return FALSE;
}

VOID ResetWallpaper()
{
	CoInitialize(NULL);

	IDesktopWallpaper* pDesktopWallpaper = NULL;
	HRESULT sc = CoCreateInstance(CLSID_DesktopWallpaper, NULL, CLSCTX_ALL, IID_IDesktopWallpaper, (LPVOID*)&pDesktopWallpaper);

	if (SUCCEEDED(sc))
	{
		LPTSTR szWallpaper = NULL;
		sc = pDesktopWallpaper->GetWallpaper(NULL, &szWallpaper);
		if(szWallpaper)
			pDesktopWallpaper->SetWallpaper(NULL, szWallpaper);
		pDesktopWallpaper->Release();
	}

	CoUninitialize();
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
	WINDOWPLACEMENT winPlackment{};
	winPlackment.length = sizeof(WINDOWPLACEMENT);
	GetWindowPlacement(hwnd, &winPlackment);

	if (winPlackment.showCmd == SW_SHOWMINIMIZED)
	{
		winPlackment.showCmd |= SW_SHOWNORMAL;
		SetWindowPlacement(hwnd,&winPlackment);
	}

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

BOOL FindStartMenu(HWND& startMenuHwnd, HWND& startMenuBtnHwnd)
{
	RECT rectDesktop{}, rectShellTrayWnd{}, rectNotify{}, rectStart{};
	HWND hShell_TrayWnd = FindWindow(L"Shell_TrayWnd", NULL);
	GetWindowRect(hShell_TrayWnd, &rectShellTrayWnd);
	POINT point{ 0, rectShellTrayWnd.top - 10 };

	if (NULL == hStartMenuBtn)
	{
		hStartMenuBtn = FindWindowEx(hShell_TrayWnd, NULL, L"Start", NULL);
	}

	if (NULL == hStartMenu)
	{
		SendAsciiInput(VK_LWIN);
		Sleep(300);
		hStartMenu = WindowFromPoint(point);

		TCHAR szStartClass[260]{};
		GetClassName(hStartMenu, szStartClass, 260);

		if (lstrcmp(szStartClass, L"Windows.UI.Core.CoreWindow") != 0)
		{
			Sleep(100);
			SendAsciiInput(VK_LWIN);
			return FALSE;
		}

		Sleep(100);
		SendAsciiInput(VK_LWIN);
	}

	startMenuHwnd = hStartMenu;
	startMenuBtnHwnd = hStartMenuBtn;

	return startMenuHwnd && startMenuBtnHwnd;
}

BOOL CenterStartMenu(BOOL enable)
{
	//TODO get taskbar height
	HWND hDesktop = GetDesktopWindow();
	RECT rectDesktop{}, rectShellTrayWnd{}, rectNotify{}, rectStart{};
	GetWindowRect(hDesktop, &rectDesktop);
	HWND hShell_TrayWnd = FindWindow(L"Shell_TrayWnd", NULL);
	GetWindowRect(hShell_TrayWnd, &rectShellTrayWnd);

	int pX = 0;
	if (enable)
	{
		pX = 0;
	}
	else
	{
		pX = (rectDesktop.right - rectNotify.right - rectStart.right) / 2 - rectStart.right;
	}
	POINT point{ pX, rectShellTrayWnd.top - 10 };
	HWND hTrayNotifyWnd = FindWindowEx(hShell_TrayWnd, NULL, L"TrayNotifyWnd", NULL);
	GetClientRect(hTrayNotifyWnd, &rectNotify);
	HWND hStart = FindWindowEx(hShell_TrayWnd, NULL, L"Start", NULL);
	GetClientRect(hStart, &rectStart);
	keybd_event(VK_LWIN, 0x45, NULL, NULL);
	keybd_event(VK_LWIN, 0x45, KEYEVENTF_KEYUP, NULL);

	Sleep(100);
	HWND hwnd = WindowFromPoint(point);

	TCHAR szStartClass[260]{};
	GetClassName(hwnd, szStartClass, 260);

	if (lstrcmp(szStartClass, L"Windows.UI.Core.CoreWindow") != 0)
	{
		Sleep(100);
		keybd_event(VK_LWIN, 0x45, NULL, NULL);
		keybd_event(VK_LWIN, 0x45, KEYEVENTF_KEYUP, NULL);
		return FALSE;
	}

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

	RefreshDesktop();
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
	return dwState == 0;
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
BOOL CreateLink(LPCWSTR lpszPathObj, LPCTSTR lpszPathLink, LPCTSTR lpszArgs,LPCWSTR lpszDesc)
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
	return SUCCEEDED(hres);
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
	return hr;
}

BOOL RemoveGodModeShortCut()
{
	TCHAR szGodmodLnk[MAX_PATH]{};
	SHGetFolderPath(NULL, CSIDL_DESKTOP, NULL, SHGFP_TYPE_CURRENT, szGodmodLnk);
	StringCchCat(szGodmodLnk, MAX_PATH, L"\\god mode.lnk");
	return DeleteFile(szGodmodLnk);
}

BOOL GetShortcutArrowState()
{
	return !ExistRegValue(HKEY_CLASSES_ROOT, L"lnkfile", L"IsShortcut");
}

BOOL RemoveShortcutArrow()
{
	auto bResult = RemovRegValue(HKEY_CLASSES_ROOT, L"lnkfile", L"IsShortcut");
	if (bResult)
		RestartExplorer();
	return bResult;
}

BOOL RestoreShortcutArrow()
{
	auto bResult =  SetSZValue(HKEY_CLASSES_ROOT, L"lnkfile", L"IsShortcut", NULL);
	if (bResult)
		RestartExplorer();
	return bResult;
}

BOOL GetWindowsPhotoViewerState()
{
	return ExistRegValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_JPG);
}

VOID RegisterWindowsPhotoViewerFormat()
{
	SetSZValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_BMP, WINDOWS_PHOTO_VIEWER_REGISTER);
	SetSZValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_DIB, WINDOWS_PHOTO_VIEWER_REGISTER);
	SetSZValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_JPEG, WINDOWS_PHOTO_VIEWER_REGISTER);
	SetSZValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_JPG, WINDOWS_PHOTO_VIEWER_REGISTER);
	SetSZValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_JXR, WINDOWS_PHOTO_VIEWER_REGISTER);
	SetSZValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_JFIF, WINDOWS_PHOTO_VIEWER_REGISTER);
	SetSZValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_WDP, WINDOWS_PHOTO_VIEWER_REGISTER);
	SetSZValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_PNG, WINDOWS_PHOTO_VIEWER_REGISTER);
	SetSZValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_ICO, WINDOWS_PHOTO_VIEWER_REGISTER);
	SetSZValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_GIF, WINDOWS_PHOTO_VIEWER_REGISTER);
}

VOID UnregisterWindowsPhotoViewerFormat()
{
	RemovRegValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_BMP);
	RemovRegValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_DIB);
	RemovRegValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_JPEG);
	RemovRegValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_JPG);
	RemovRegValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_JXR);
	RemovRegValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_JFIF);
	RemovRegValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_WDP);
	RemovRegValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_PNG);
	RemovRegValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_ICO);
	RemovRegValue(HKEY_LOCAL_MACHINE, WINDOWS_PHOTO_VIEWER_PATH, WINDOWS_PHOTO_VIEWER_GIF);
}

BOOL GetPaintVersionState()
{
	DWORD dwValue = 0;
	QueryDWORDValue(HKEY_CURRENT_USER, LR"(Control Panel\Desktop)", L"PaintDesktopVersion", &dwValue);
	return dwValue == 1;
}

BOOL PaintVersionInfo(BOOL enable)
{
	//HKEY_CURRENT_USER\Control Panel\Desktop\PaintDesktopVersion(0x1)
	DWORD dwValue = enable == TRUE ? 1 : 0;
	auto bResult =  SetDWORDValue(HKEY_CURRENT_USER, LR"(Control Panel\Desktop)", L"PaintDesktopVersion", dwValue);

	if (bResult)
		RefreshDesktop();

	return bResult;
}

BOOL GetTaskbarThumbnailSize(DWORD* dwSize)
{
	return QueryDWORDValue(HKEY_CURRENT_USER, TASKBAND_REGPATH, TASKBAR_THUMB_SIZE, dwSize);
}

BOOL SetTaskbarThumbnailSize(DWORD dwSize,BOOL bRestartExplorer)
{
	BOOL bResult = FALSE;
	if (dwSize == RESET_TASKBARTHUMB)
	{
		bResult = RemovRegValue(HKEY_CURRENT_USER, TASKBAND_REGPATH, TASKBAR_THUMB_SIZE);
	}
	else
	{
		bResult = SetDWORDValue(HKEY_CURRENT_USER, TASKBAND_REGPATH, TASKBAR_THUMB_SIZE, dwSize);
	}

	if (bRestartExplorer)
	{
		RestartExplorer();
	}
	return bResult;
	
}

VOID ActivateTaskBar()
{
	keybd_event(VK_LWIN, 0x45, NULL, NULL);
	keybd_event(VK_LWIN, 0x45, KEYEVENTF_KEYUP, NULL);
	Sleep(70);
	keybd_event(VK_LWIN, 0x45, NULL, NULL);
	keybd_event(VK_LWIN, 0x45, KEYEVENTF_KEYUP, NULL);
}

////from https://github.com/cairoshell/ManagedShell  
//src/ManagedShell.WindowsTasks/ApplicationWindow.cs
BOOL GetShowInTaskbar(HWND hwnd,int extendedWindowStyles)
{
	// EnumWindows and ShellHook return UWP app windows that are 'cloaked', which should not be visible in the taskbar.
	if (IsWindows8OrGreater())
	{
		UINT cloaked;
		DwmGetWindowAttribute(hwnd, DWMWA_CLOAKED, &cloaked, sizeof(UINT));

		if (cloaked > 0)
		{
			return FALSE;
		}

		// UWP shell windows that are not cloaked should be hidden from the taskbar, too.
		LPTSTR cName = new TCHAR[256];
		GetClassName(hwnd, cName, 256);
		if (StrCmp(L"ApplicationFrameWindow",cName ) == 0 || StrCmp(L"Windows.UI.Core.CoreWindow",cName) == 0)
		{
			if ((extendedWindowStyles & (int)WS_EX_WINDOWEDGE) == 0)
			{
				delete[] cName;
				return false;
			}
		}
		else if (!IsWindows10OrGreater && (StrCmp(L"ImmersiveBackgroundWindow", cName) == 0 || StrCmp(L"SearchPane",cName) == 0 || StrCmp(L"NativeHWNDHost",cName) == 0 || StrCmp(L"Shell_CharmWindow",cName) == 0 || StrCmp(L"ImmersiveLauncher",cName)))
		{
			delete[] cName;
			return FALSE;
		}
	}

	return TRUE;
}


//from https://github.com/cairoshell/ManagedShell  
//src/ManagedShell.WindowsTasks/ApplicationWindow.cs
BOOL CanAddToTaskBar(HWND hwnd)
{
	if (NULL == hwnd)
		return FALSE;

	int extendedWindowStyles = GetWindowLong(hwnd, GWL_EXSTYLE);
	bool isWindow = IsWindow(hwnd);
	bool isVisible = IsWindowVisible(hwnd);
	bool isToolWindow = (extendedWindowStyles & WS_EX_TOOLWINDOW) != 0;
	bool isAppWindow = (extendedWindowStyles & WS_EX_APPWINDOW) != 0;
	bool isNoActivate = (extendedWindowStyles & WS_EX_NOACTIVATE) != 0;
	HWND ownerWin = GetWindow(hwnd, GW_OWNER);

	return isWindow && isVisible && (ownerWin == NULL || isAppWindow) && (!isNoActivate || isAppWindow) && !isToolWindow && GetShowInTaskbar(hwnd, extendedWindowStyles);
}

LPTSTR GetProcessNameFomrHwnd(HWND hWnd)
{
	TCHAR* buf = new TCHAR[512];

	DWORD dwLen = 512;
	DWORD procId;
	GetWindowThreadProcessId(hWnd, &procId);
	if (procId != 0)
	{
		HANDLE hProc = OpenProcess(0x00001000, false, (int)procId);
		QueryFullProcessImageName(hProc, 0, buf, &dwLen);

		//uwp
		TCHAR* lowerBuf = new TCHAR[260];
		lstrcpy(lowerBuf, buf);
	    lowerBuf = CharLower(lowerBuf);
		if (NULL != StrStr(lowerBuf, L"applicationframehost.exe"))
		{
			delete[] lowerBuf;
			lowerBuf = NULL;
			return NULL;
		}

		//fix this
		/*if (NULL != StrStr(lowerBuf, L"windowsapps"))
		{
			delete[] lowerBuf;
			lowerBuf = NULL;
			return NULL;
		}*/

		/*if (NULL != StrStr(lowerBuf, L"systemapps"))
		{
			delete[] lowerBuf;
			lowerBuf = NULL;
			return NULL;
		}*/

		if (NULL != StrStr(lowerBuf, L"explorer.exe"))
		{
			delete[] lowerBuf;
			lowerBuf = NULL;
			return NULL;
		}
	}

	TCHAR* szText = new TCHAR[128];
	GetWindowText(hWnd, szText, 128);
	buf = lstrcat(buf, L";");
	buf = lstrcat(buf, szText);

	delete[] szText;
	szText = NULL;

	return buf;
}

