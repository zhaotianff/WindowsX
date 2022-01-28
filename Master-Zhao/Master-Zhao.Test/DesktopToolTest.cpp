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
	MessageBox(NULL, result == true ? L"Is windows 10 = true." : L"Is windows 10 = false.", L"",MB_OK);
	result = IsWindows10OrHigher();
	MessageBox(NULL, result == true ? L"Is windows 10 or greater = true." : L"Is windows 10 or greater = false.", L"", MB_OK);
}
