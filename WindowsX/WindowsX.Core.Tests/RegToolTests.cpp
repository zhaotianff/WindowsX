#include "pch.h"
#include "CppUnitTest.h"
#include "../WindowsX.Core/RegTool.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace WindowsXCoreTests
{
	TEST_CLASS(WindowsXCoreTests)
	{
	public:

		TEST_METHOD(TestSetSZValue)
		{
			BOOL bResult = SetSZValue(HKEY_LOCAL_MACHINE, LR"(SOFTWARE\Microsoft\Windows\CurrentVersion\Run\Disabled)", L"test",L"ttttttt");
			Assert::IsTrue(bResult);
		}
	};
}
