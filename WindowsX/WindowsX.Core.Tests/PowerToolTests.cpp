#include "pch.h"
#include "CppUnitTest.h"
#include "../WindowsX.Core/PowerTool.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace WindowsXCoreTests
{
	TEST_CLASS(PowerToolTests)
	{
	public:

		TEST_METHOD(TestShowShutDownDialog)
		{
			ShowShutDownDialog();
		}
	};
}
