#include "pch.h"
#include "CppUnitTest.h"
#include "../Master-Zhao.Core/AppTool.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace MasterZhaoCoreTests
{
	TEST_CLASS(MasterZhaoCoreTests)
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
