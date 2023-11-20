#include "pch.h"
#include "CppUnitTest.h"
#include "../Master-Zhao.Core/StringHelper.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace MasterZhaoCoreTests
{
	TEST_CLASS(MasterZhaoCoreTests)
	{
	public:
		TEST_METHOD(TestContains)
		{
			auto result = Contains(L"HelloWorld", L"oW");
			Assert::IsTrue(result);
		}
	};
}
