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
