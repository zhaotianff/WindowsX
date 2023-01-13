#include "pch.h"
#include "CppUnitTest.h"
#include "../Master-Zhao.Core/PowerTool.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace MasterZhaoCoreTests
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
