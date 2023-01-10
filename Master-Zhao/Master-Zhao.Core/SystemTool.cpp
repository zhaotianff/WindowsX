#include"SystemTool.h"
#include"DesktopTool.h"
#include"InputTool.h"
#include<VersionHelpers.h>
#include<functional>
#include<Winternl.h>
#include<powrprof.h>
#include<wtsapi32.h>

#pragma comment(lib,"PowrProf.lib")
#pragma comment(lib,"wtsapi32.lib")

extern HMODULE g_hDllModule;

#pragma data_seg("mydata")
HHOOK g_hHookKb = NULL;
HHOOK g_hHookMs = NULL;
#pragma data_seg()
#pragma comment(linker, "/SECTION:mydata,RWS")

HWND hStartMenuWindow = NULL;
BOOL bStartMenuDisplay = FALSE;
TCHAR szStartMenu[MAX_PATH];
DWORD cKeyCode = 0;

SYSTEMTIME GetUserLoginTime()
{
	SYSTEMTIME stSys{};

	HINSTANCE hInstance = LoadLibrary(L"ntdll.dll");
	if (hInstance == NULL)
		return stSys;

	funNtQuerySystemInformation NtQuerySystemInformation = (funNtQuerySystemInformation)GetProcAddress(hInstance,"NtQuerySystemInformation");

	if (NtQuerySystemInformation)
	{
		SYSTEM_TIME_INFORMATION tInfo{};
		auto ret = NtQuerySystemInformation(SystemTimeOfDayInformation, &tInfo, sizeof(tInfo), 0);
		FILETIME stFile = *(FILETIME*)&tInfo.liKeBootTime;

		FileTimeToLocalFileTime(&stFile, &stFile);
		FileTimeToSystemTime(&stFile, &stSys);

		FreeLibrary(hInstance);
	}
	
	return stSys;
}

void GetVersionNumbers(DWORD* pdwMajor, DWORD* pdwMinor, DWORD* pdwBuildNumber)
{
	HINSTANCE hInstance = LoadLibrary(L"ntdll.dll");
	if (hInstance == NULL)
		return;

	auto RtlGetNtVersionNumbers = (funRtlGetNtVersionNumbers)GetProcAddress(hInstance, "RtlGetNtVersionNumbers");
	if (RtlGetNtVersionNumbers)
	{
		RtlGetNtVersionNumbers(pdwMajor, pdwMinor, pdwBuildNumber);
		*pdwBuildNumber &= 0xffff;
	}
}

BOOL IsWindows10()
{
	//first windows 10 version =>Version 1507 (RTM) (OS build 10240)
	DWORD dwMajor, dwMinor, dwBuildNumber;
	GetVersionNumbers(&dwMajor, &dwMinor, &dwBuildNumber);
	return dwMajor == 10 && dwBuildNumber >= 10240 && dwBuildNumber < 22000;
}

BOOL IsWindows10OrHigher()
{
	DWORD dwMajor, dwMinor, dwBuildNumber;
	GetVersionNumbers(&dwMajor, &dwMinor, &dwBuildNumber);
	return dwMajor == 10 && dwBuildNumber >= 10240;
}

BOOL IsWindows11()
{
	//no windows 11 SDK
	

	//Windows 10 2004 =>	10.0.19041
	//Windows 11 =>         10.0.22000
	DWORD dwMajor, dwMinor, dwBuildNumber;
	GetVersionNumbers(&dwMajor, &dwMinor, &dwBuildNumber);
	return dwMajor == 10 && dwBuildNumber >= 22000;
}

BOOL RegisterFastRunHotKey(HWND hwnd)
{
	RAWINPUTDEVICE rawInputDevice{};
	rawInputDevice.usUsagePage = 0x01;
	rawInputDevice.usUsage = 0x06;
	rawInputDevice.dwFlags = RIDEV_INPUTSINK;
	rawInputDevice.hwndTarget = hwnd;

	BOOL bRet = RegisterRawInputDevices(&rawInputDevice, 1, sizeof(rawInputDevice));

	return bRet;
}

BOOL UnRegisterFastRunHotKey()
{
	RAWINPUTDEVICE rawInputDevice{};
	rawInputDevice.usUsagePage = 0x01;
	rawInputDevice.usUsage = 0x06;
	rawInputDevice.dwFlags = RIDEV_REMOVE;
	rawInputDevice.hwndTarget = NULL;

	BOOL bRet = RegisterRawInputDevices(&rawInputDevice, 1, sizeof(rawInputDevice));

	return bRet;
}

int GetCPUKernelCount()
{
	SYSTEM_INFO sinfo{};
	GetSystemInfo(&sinfo);
	return sinfo.dwNumberOfProcessors;
}

BOOL AdjustPrivilege()
{
	HANDLE hToken = NULL;
	LUID uID; //Describes a local identifier for an adapter.
	TOKEN_PRIVILEGES privValues;

	BOOL bOpen = OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken);
	if (!bOpen)
		return FALSE;

	BOOL bLooked = LookupPrivilegeValue(NULL, SE_DEBUG_NAME ,&uID);

	if (!bLooked)
	{
		CloseHandle(hToken);
		hToken = NULL;
	}

	privValues.PrivilegeCount = 1;
	privValues.Privileges[0].Luid = uID;
	privValues.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
	//The AdjustTokenPrivileges function enables or disables privileges in the specified access token. 
	//Enabling or disabling privileges in an access token requires TOKEN_ADJUST_PRIVILEGES access.

	BOOL bAdjusted = FALSE;
	if(hToken)
		bAdjusted = AdjustTokenPrivileges(hToken, FALSE, &privValues, sizeof(privValues), NULL, NULL);

	if (!bAdjusted)
		return FALSE;

	if(hToken)
		CloseHandle(hToken);
	hToken = NULL;
	return TRUE;
}

BOOL ForceDeleteFile(LPTSTR lpszFilePah)
{
	BOOL bResult = FALSE;
	return bResult;

	HINSTANCE hInstance = LoadLibrary(L"ntdll.dll");
	if (hInstance == NULL)
		return bResult;

	funNtQuerySystemInformation NtQuerySystemInformation = (funNtQuerySystemInformation)GetProcAddress(hInstance, "NtQuerySystemInformation");

	if (NtQuerySystemInformation)
	{
		int nCount = 0;
		SYSTEM_PROCESS_INFORMATION* pSystemProcess = NULL;
		DWORD dwBufferSize = 0xFFFFF;
		pSystemProcess = (SYSTEM_PROCESS_INFORMATION*)malloc(dwBufferSize);
		DWORD dwRet = NtQuerySystemInformation(SystemProcessInformation, pSystemProcess, dwBufferSize, &dwBufferSize);

		if (dwRet != 0)
		{
			return bResult;
		}

		while (pSystemProcess->NextEntryOffset)
		{
			if (0 != pSystemProcess->UniqueProcessId)
			{
				//pSystemProcess->ImageName
				//pSystemProcess->UniqueProcessId
				//add to list
			}

			pSystemProcess = (SYSTEM_PROCESS_INFORMATION*)(((PUCHAR)pSystemProcess) + pSystemProcess->NextEntryOffset);
		}

		bResult = true;

		if (pSystemProcess)
			free(pSystemProcess);
	}

	return bResult;
}

VOID Shutdown()
{
	//InitiateSystemShutdownEx(NULL, NULL, 0, FALSE, FALSE, SHTDN_REASON_MINOR_PROCESSOR);
	system("shutdown -s -t 0");
}

VOID SwitchUser()
{
	WTSDisconnectSession(WTS_CURRENT_SERVER_HANDLE, WTS_CURRENT_SESSION, FALSE);
}

VOID Logoff()
{
	ExitWindows();
}

VOID Lock()
{
	LockWorkStation();
}

VOID Restart()
{
	ExitWindowsEx(EWX_REBOOT, SHTDN_REASON_MINOR_PROCESSOR);
}

VOID Sleep()
{
	SetSuspendState(FALSE, FALSE, TRUE);
}

LRESULT WINAPI KbLLProc(int code, WPARAM wParam, LPARAM lParam)
{
	PKBDLLHOOKSTRUCT pKb = NULL;
	BOOL bWinKeyStroke = FALSE;

	if (code == HC_ACTION)
	{
		pKb = (PKBDLLHOOKSTRUCT)lParam;

		switch (wParam)
		{
		case WM_KEYDOWN:
			if (pKb->vkCode != VK_LWIN && pKb->vkCode != VK_RWIN)
			{
				cKeyCode = pKb->vkCode;
			}
			break;
		case WM_KEYUP:
		{	
			bWinKeyStroke = (pKb->vkCode == VK_LWIN) || (pKb->vkCode == VK_RWIN) ||
				((pKb->vkCode == VK_ESCAPE) && ((GetKeyState(VK_CONTROL) & 0x8000) != 0));
			break;
			}
			default:
				break;
		}
	}

	if (bWinKeyStroke && hStartMenuWindow)
	{
		if (cKeyCode == 0)
		{
			bStartMenuDisplay = !bStartMenuDisplay;
			ShowWindow(hStartMenuWindow, bStartMenuDisplay ? SW_SHOW : SW_HIDE);
		}
		else
		{
			cKeyCode = 0;
		}
	}

	//hide start menu directly 
	bWinKeyStroke = FALSE;

	return bWinKeyStroke ? TRUE : CallNextHookEx(g_hHookKb, code, wParam, lParam);
}

LRESULT WINAPI MsLLProc(int code, WPARAM wParam, LPARAM lParam)
{
	BOOL bStartMenuClick = FALSE;
	PMSLLHOOKSTRUCT p = NULL;

	if (code == HC_ACTION)
	{
		p = (PMSLLHOOKSTRUCT)lParam;
		switch (wParam)
		{
		case  WM_LBUTTONDOWN:
		{
			HWND hwnd = WindowFromPoint(p->pt);
			GetClassName(hwnd, szStartMenu, MAX_PATH);
			if (lstrcmpW(szStartMenu, L"Start") == 0)
				bStartMenuClick = TRUE;
		}
		break;
		default:
			break;
		}
	}

	if (bStartMenuClick && hStartMenuWindow)
	{
		bStartMenuDisplay = !bStartMenuDisplay;
		ShowWindow(hStartMenuWindow, bStartMenuDisplay ? SW_SHOW : SW_HIDE);
	}

	//hide start menu directly 
	bStartMenuClick = FALSE;

	return (bStartMenuClick ? TRUE : CallNextHookEx(g_hHookMs, code, wParam, lParam));
}

BOOL HookStart(HWND hwnd)
{
	hStartMenuWindow = hwnd;

	HWND hStart = NULL;
	HWND hStartBtn = NULL;
	FindStartMenu(hStart, hStartBtn);

	if (hStart)
	{
		ShowWindow(hStart, SW_HIDE);
	}

	g_hHookKb = SetWindowsHookEx(WH_KEYBOARD_LL, KbLLProc, g_hDllModule, 0);

	//RetroBar Taskbar
	HWND hRetroBar = FindWindow(NULL, L"RetroBar Taskbar");

	if (NULL == hRetroBar)
	{
		g_hHookMs = SetWindowsHookEx(WH_MOUSE_LL, MsLLProc, g_hDllModule, 0);

		return (g_hHookKb && g_hHookMs && hStart);
	}

	return NULL != g_hHookKb;
}

BOOL UnHookStart()
{
	BOOL bResult = FALSE;
	hStartMenuWindow = NULL;

	if (g_hHookKb)
	{
		bResult = UnhookWindowsHookEx(g_hHookKb);
		keybd_event(VK_LWIN, 0x45, KEYEVENTF_KEYUP, NULL);
	}

	if(g_hHookMs)
		bResult &= UnhookWindowsHookEx(g_hHookMs);

	HWND hStart = NULL;
	HWND hStartBtn = NULL;
	FindStartMenu(hStart, hStartBtn);

	if (hStart)
	{
		ShowWindow(hStart, SW_SHOW);
	}

	bResult &= NULL != hStart;

	return bResult;
}

BOOL ShowCustomStart()
{
	bStartMenuDisplay = TRUE;
	if (hStartMenuWindow)
	{
		return ShowWindow(hStartMenuWindow,SW_SHOW);
	}

	return FALSE;
}

BOOL HideCustomStart()
{
	bStartMenuDisplay = FALSE;
	if (hStartMenuWindow)
	{
		return ShowWindow(hStartMenuWindow, SW_HIDE);
	}

	return FALSE;
}

VOID CloseCustomStart()
{
	if (hStartMenuWindow)
	{
		SendMessage(hStartMenuWindow, WM_CLOSE, NULL, NULL);
	}
}

BOOL RegisterBossKeyHotKey(HWND hwnd, UINT modifier,UINT vkCode,UINT nHotKeyId)
{
	return RegisterHotKey(hwnd, nHotKeyId, modifier, vkCode);
}

BOOL UnRegisterBossKeyHotKey(HWND hwnd,UINT nHotKeyId)
{
	return UnregisterHotKey(hwnd, nHotKeyId);
}

BOOL GetUserProfilePicturePath(LPTSTR buf,DWORD nSize)
{
	HMODULE hModule = LoadLibrary(L"shell32.dll");

	if (hModule)
	{
		funSHGetUserPicturePath  SHGetUserPicturePath = (funSHGetUserPicturePath)GetProcAddress(hModule, MAKEINTRESOURCEA(261));

		if (SHGetUserPicturePath)
		{
			TCHAR szPicturebuf[MAX_PATH];
			DWORD nUserName = 128;
			TCHAR szUserName[128];
			GetUserName(szUserName, &nUserName);
			HRESULT hr = SHGetUserPicturePath(szUserName, 0x80000000, szPicturebuf, MAX_PATH);
			StringCchCopy(buf, nSize, szPicturebuf);
			FreeLibrary(hModule);
			return SUCCEEDED(hr);
		}
	}
	return FALSE;
}

LPTSTR GetFileDescrption(LPTSTR pszFilePath)
{
	CoInitialize(nullptr);   

	//PKEY_Software_ProductName
	LPTSTR pszDescription = GetShellPropertyStringFromPath(pszFilePath, PKEY_FileDescription);

	CoUninitialize();
	return pszDescription;
}

HRESULT GetShortcutPath(LPTSTR szLnkPath, LPTSTR szAbsoluatePath, DWORD nBufferSize)
{
	IShellLink* psl;

	HRESULT hr = CoCreateInstance(CLSID_ShellLink, NULL, CLSCTX_INPROC_SERVER, IID_IShellLink, (LPVOID*)&psl);
	if (SUCCEEDED(hr))
	{
		IPersistFile* ppf;

		hr = psl->QueryInterface(IID_IPersistFile, (void**)&ppf);

		if (SUCCEEDED(hr))
		{
			hr = ppf->Load(szLnkPath, STGM_READ);

			if (SUCCEEDED(hr))
			{
				hr = psl->GetPath(szAbsoluatePath, MAX_PATH, NULL, SLGP_SHORTPATH);
			}
			ppf->Release();
		}
		psl->Release();
	}
	return hr;
}

BOOL GetSpeicalFolder(DWORD csidl, LPTSTR szBuffer)
{
	LPITEMIDLIST pIdList;
	HRESULT hr = SHGetSpecialFolderLocation(NULL, csidl, &pIdList);
	if (FAILED(hr))
		return FALSE;
	return SHGetPathFromIDList(pIdList, szBuffer);
}


LPTSTR GetShellPropertyStringFromPath(LPCWSTR pszPath, PROPERTYKEY const& key)
{
	CComPtr<IShellItem2> pItem;

	TCHAR szFullPath[MAX_PATH]{};
	MakeFullPath(pszPath, szFullPath, MAX_PATH);

	HRESULT hr = SHCreateItemFromParsingName(szFullPath, nullptr, IID_PPV_ARGS(&pItem));

	if (FAILED(hr))
		return NULL;

	CComHeapPtr<WCHAR> pValue;
	hr = pItem->GetString(key, &pValue);

	if (FAILED(hr))
		return NULL;

	return pValue.m_pData;
}

VOID MakeFullPath(LPCWSTR pszPath,LPTSTR szFullPath,DWORD nFullPathSize)
{
	StringCchCopy(szFullPath, nFullPathSize, pszPath);

	std::wstring path = pszPath;
	if (path.find('%') != std::wstring::npos)
	{
		auto nStart = path.find_first_of('%') + 1;
		auto nEnd = path.find_last_of('%');
		std::wstring strEnv = path.substr(nStart, nEnd - nStart);

		TCHAR envBuffer[MAX_PATH]{};

		if (GetEnvironmentVariable(strEnv.data(), envBuffer, MAX_PATH))
		{
			path = path.replace(nStart - 1, nEnd - nStart + 2, envBuffer);
			StringCchCopy(szFullPath,path.size() + 1, path.data());
		}
	}
}



