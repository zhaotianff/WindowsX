#include "DesktopTool.h"

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
	TCHAR buf[MAX_PATH]{};
	TCHAR path[MAX_PATH]{};
	std::vector<std::wstring> list;
	auto result = RegOpenKeyEx(HKEY_CURRENT_USER, L"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Wallpapers", 0, KEY_ALL_ACCESS, &hKey);
	if (ERROR_SUCCESS == result)
	{
		for (int i = 0;i < 5;i++)
		{
			length = MAX_PATH;
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

VOID SwitchToDesktop()
{
	CoInitialize(NULL);
	IShellDispatch4* pShellDisp = NULL;
	HRESULT sc = CoCreateInstance(CLSID_Shell, NULL, CLSCTX_SERVER, IID_IDispatch, (LPVOID*)&pShellDisp);
	sc = pShellDisp->ToggleDesktop();
	sc = pShellDisp->ToggleDesktop();
	pShellDisp->Release();
}
