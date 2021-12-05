#include "DesktopTool.h"

PBASICWINDOWINFO pwiGlobalInfo;
int nWindowCount;
HWND hWorkerW;
HWND hEmbedWindow;

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

BOOL EmbedWindowToDesktop(LPTSTR lpWindowName)
{
	HWND hwnd = FindWindow(NULL, lpWindowName);

	if (hwnd == NULL)
		return FALSE;

	pwiGlobalInfo = new tagBASICWINDOWINFO;
	nWindowCount = 0;
	hEmbedWindow = hwnd;

	HWND hProgman = FindWindow(L"Progman", L"Program Manager");
	if (hProgman == NULL)
		return FALSE;

	SendMessage(hProgman, WM_SPAWN_WORKER, 0, 0);
	EnumWindows(EnumWindowProc, 0);

	for (int i = 0; i < nWindowCount; i++)
	{
		if (lstrcmpW(pwiGlobalInfo->szWindowName, L"WorkerW") == 0)
		{
			HWND hSHellDefView = FindWindowEx(pwiGlobalInfo->hwnd, 0, L"SHELLDLL_DefView", NULL);
			if (hSHellDefView == NULL)
			{
				hWorkerW = pwiGlobalInfo->hwnd;
				ShowWindow(hWorkerW, SW_HIDE);
				return TRUE;
			}
			else
			{
				PBASICWINDOWINFO pwiNext = pwiGlobalInfo->next;
				delete pwiGlobalInfo;
				pwiGlobalInfo = NULL;
				pwiGlobalInfo = pwiNext;

				if (pwiGlobalInfo->szWindowName == L"Progman")
				{
					HWND hLocalWorkW = FindWindowEx(pwiGlobalInfo->hwnd, 0, L"WorkerW", NULL);

					if (IsWindowVisible(hLocalWorkW) == FALSE)
					{
						return TRUE;
					}
					else
					{
						hWorkerW = hLocalWorkW;
						ShowWindow(hLocalWorkW, SW_HIDE);
					}
				}
				else
				{
					ShowWindow(pwiGlobalInfo->hwnd, SW_HIDE);
				}
			}
		}
		PBASICWINDOWINFO pwiNext = pwiGlobalInfo->next;
		delete pwiGlobalInfo;
		pwiGlobalInfo = NULL;
		pwiGlobalInfo = pwiNext;
	}
	
	return FALSE;
}

BOOL RestoreEmbedHwnd()
{
	if (pwiGlobalInfo)
		delete pwiGlobalInfo;

	if (hEmbedWindow)
	{
		SendMessage(hEmbedWindow, WM_CLOSE, 0, 0);
	}

	if (hWorkerW)
	{
		ShowWindow(hWorkerW, SW_SHOW);
	}
	return TRUE;
}

BOOL CALLBACK EnumWindowProc(HWND hwnd, LPARAM lParam)
{
	PBASICWINDOWINFO windowInfo = new tagBASICWINDOWINFO;
	memset(pwiGlobalInfo->szWindowName, 0, sizeof(pwiGlobalInfo->szWindowName));
	GetClassName(hwnd, pwiGlobalInfo->szWindowName, sizeof(pwiGlobalInfo->szWindowName) / sizeof(TCHAR));
	windowInfo->hwnd = hwnd;
	nWindowCount++;
	windowInfo->next = pwiGlobalInfo;
	pwiGlobalInfo = windowInfo;
	return TRUE;
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
