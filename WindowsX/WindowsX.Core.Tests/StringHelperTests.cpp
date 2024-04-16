#include "pch.h"
#include "CppUnitTest.h"
#include "../WindowsX.Core/StringHelper.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace WindowsXCoreTests
{
	TEST_CLASS(WindowsXCoreTests)
	{
	public:
		TEST_METHOD(TestContains)
		{
			auto result = Contains(L"HelloWorld", L"oW");
			Assert::IsTrue(result);
		}
	};
}
