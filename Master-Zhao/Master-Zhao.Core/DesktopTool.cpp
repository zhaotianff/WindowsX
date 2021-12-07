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
	MessageBox(NULL, lpWindowName, L"", MB_OK);
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
