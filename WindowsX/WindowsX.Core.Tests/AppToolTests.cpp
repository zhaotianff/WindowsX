#include "pch.h"
#include "CppUnitTest.h"
#include "../WindowsX.Core/AppTool.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace WindowsXCoreTests
{
	TEST_CLASS(WindowsXCoreTests)
	{
	public:

		TEST_METHOD(TestGetAppList)
		{
			TCHAR szBuffer[1024];
			auto result = GetAppPath(szBuffer, 1024);

			Assert::IsTrue(result);
		}
	};
}
