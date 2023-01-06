#include "pch.h"
#include "CppUnitTest.h"
#include "../Master-Zhao.Core/RegTool.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace MasterZhaoCoreTests
{
	TEST_CLASS(MasterZhaoCoreTests)
	{
	public:

		TEST_METHOD(TestSetSZValue)
		{
			BOOL bResult = SetSZValue(HKEY_LOCAL_MACHINE, LR"(SOFTWARE\Microsoft\Windows\CurrentVersion\Run\Disabled)", L"test",L"ttttttt");
			Assert::IsTrue(bResult);
		}
	};
}
