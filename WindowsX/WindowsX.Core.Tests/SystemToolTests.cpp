#include "pch.h"
#include "CppUnitTest.h"
#include "../WindowsX.Core/SystemTool.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace WindowsXCoreTests
{
	TEST_CLASS(WindowsXCoreTests)
	{
	public:

		TEST_METHOD(TestGetFileDescription)
		{
			LPTSTR szBuffer = GetFileDescrption(LR"(%windir%\notepad.exe)");
			Assert::IsNotNull(szBuffer);
		}

		TEST_METHOD(TestGetShortcutPath)
		{
			TCHAR buf[MAX_PATH]{};
			HRESULT hr = GetShortcutPath(LR"(C:\Users\xi\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\Ganshorn.LFX.lnk)", buf, MAX_PATH);
		}

		TEST_METHOD(TestIsProcessContainModule)
		{
			
			BOOL bRet = IsProcessContainModule(L"explorer.exe",L"WINDOWS.UI.dll");

			Assert::IsTrue(bRet);
		}
	};
}