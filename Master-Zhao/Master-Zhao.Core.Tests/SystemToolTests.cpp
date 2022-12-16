#include "pch.h"
#include "CppUnitTest.h"
#include "../Master-Zhao.Core/SystemTool.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace MasterZhaoCoreTests
{
	TEST_CLASS(MasterZhaoCoreTests)
	{
	public:

		TEST_METHOD(TestGetFileDescription)
		{
			LPTSTR szBuffer = GetFileDescrption(LR"(C:\windows\notepad.exe)");
			Assert::IsNotNull(szBuffer);
		}
	};
}