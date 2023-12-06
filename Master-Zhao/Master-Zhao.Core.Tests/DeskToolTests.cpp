#include "pch.h"
#include "CppUnitTest.h"
#include "../Master-Zhao.Core/DesktopTool.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace MasterZhaoCoreTests
{
	TEST_CLASS(MasterZhaoCoreTests)
	{
	public:
		
		TEST_METHOD(TestGetBackground)
		{
			TCHAR* buf = new TCHAR[MAX_PATH];
			auto result = GetBackground(buf);
			Assert::IsTrue(result);
		}

		TEST_METHOD(TestSetBackground)
		{
			LPWSTR lpImagePath = new TCHAR[MAX_PATH];
			GetCurrentDirectory(MAX_PATH, lpImagePath);
			auto len = lstrlen(lpImagePath);
			lstrcat(lpImagePath, L"\\flower.jpg");

			if (_waccess(lpImagePath, 0) == 0)
			{
				Assert::IsTrue(SetBackground(lpImagePath));
			}

			delete[] lpImagePath;
		}

		TEST_METHOD(TestSwitchToDesktop)
		{
			SwitchToDesktop();	
		}

		TEST_METHOD(TestEmbedWindow)
		{
			EmbedWindowToDesktop(L"MainWindow");
		}

		TEST_METHOD(TestCenterTaskBar)
		{
			CenterStartMenu(FALSE);
			CenterTaskListIcon(FALSE);
		}

		TEST_METHOD(TestReg)
		{
			//auto result = ExistSubKey(HKEY_CURRENT_USER, L"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\HideDesktopIcons\\NewStartPanel");

			//DWORD dd = 0x3;
			//SetDWORDValue(HKEY_CURRENT_USER, L"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\HideDesktopIcons\\NewStartPanel",L"test", dd);

			//TCHAR str[260] = L"HelloWorld";
			//SetSZValue(HKEY_CURRENT_USER, L"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\HideDesktopIcons\\NewStartPanel", L"test2", str);

			DWORD value = 0;
			QueryDWORDValue(HKEY_CURRENT_USER, L"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\HideDesktopIcons\\NewStartPanel", L"{59031a47-3f72-44a7-89c5-5595fe6b30ee}", &value);
		}

		TEST_METHOD(TestRefresh)
		{
			RefreshDesktop();
		}

		TEST_METHOD(TestCreateGodmode)
		{
			if (GetGodModeShortCutState())
			{
				MessageBox(NULL, L"god mode快捷方式已经存在", L"", MB_OK);
				return;
			}
			CreateGodModeShortCut();
		}

		TEST_METHOD(TestSetThumbSize)
		{
			SetTaskbarThumbnailSize(128, TRUE);
		}

		TEST_METHOD(TestShotCut)
		{
			//auto result = RemoveShortcutArrow();
			//result = RestoreShortcutArrow();

			auto state = GetWindowsPhotoViewerState();

			//RegisterWindowsPhotoViewerFormat();

			UnregisterWindowsPhotoViewerFormat();
		}

		TEST_METHOD(TestOpenRunDialog)
		{
			OpenRunDialog(L"test");
		}

		TEST_METHOD(TestCreateShortcut)
		{
			CreateLink(L"Applications", LR"(C:\Users\user\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\WindowsMediaPlayer.lnk)", L"Microsoft.Windows.MediaPlayer32", L"Windows Media Player");
		}
	};
}
