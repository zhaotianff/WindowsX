#include "DesktopToolTest.h"

void TestGetBackground()
{
	TCHAR* buf = new TCHAR[MAX_PATH];
	auto result = GetBackground(buf);
	if (result)
	{
		setlocale(LC_ALL, "");
		std::wcout << buf << std::endl;
	}
	std::cin.get();
}

void TestSetBackground()
{
	LPWSTR lpImagePath = new TCHAR[MAX_PATH];
	GetCurrentDirectory(MAX_PATH, lpImagePath);
	auto len = lstrlen(lpImagePath);
	lstrcat(lpImagePath, L"\\flower.jpg");			

	if (_waccess(lpImagePath, 0) == 0)
	{
		if (SetBackground(lpImagePath))
		{
			std::cout << "set background success";
		}
		else
		{
			std::cout << "set background failed";
		}
	}

	delete[] lpImagePath;
	std::cin.get();
}

void TestSwitchToDesktop()
{
	SwitchToDesktop();
}

void TestEmbedWindow()
{
	EmbedWindowToDesktop(L"MainWindow");
}

void TestCenterTaskBar()
{
	CenterStartMenu(FALSE);
	CenterTaskListIcon(FALSE);
}

void TestOsVersion()
{
 	auto result = IsWindows10();
	MessageBox(NULL, result == TRUE ? L"Is windows 10 = true." : L"Is windows 10 = false.", L"",MB_OK);
	result = IsWindows10OrHigher();
	MessageBox(NULL, result == TRUE ? L"Is windows 10 or greater = true." : L"Is windows 10 or greater = false.", L"", MB_OK);
}

void TestReg()
{
	//auto result = ExistSubKey(HKEY_CURRENT_USER, L"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\HideDesktopIcons\\NewStartPanel");

	//DWORD dd = 0x3;
	//SetDWORDValue(HKEY_CURRENT_USER, L"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\HideDesktopIcons\\NewStartPanel",L"test", dd);

	//TCHAR str[260] = L"HelloWorld";
	//SetSZValue(HKEY_CURRENT_USER, L"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\HideDesktopIcons\\NewStartPanel", L"test2", str);

	DWORD value = 0;
	QueryDWORDValue(HKEY_CURRENT_USER, L"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\HideDesktopIcons\\NewStartPanel", L"{59031a47-3f72-44a7-89c5-5595fe6b30ee}", &value);
}

void TestRefresh()
{
	RefreshDesktop();
}

void TestCreateGodmode()
{
	if (GetGodModeShortCutState())
	{
		MessageBox(NULL, L"god mode快捷方式已经存在",L"",MB_OK);
		return;
	}
	CreateGodModeShortCut();
}
