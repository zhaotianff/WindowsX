#include "pch.h"
#include "CppUnitTest.h"
#include "../Master-Zhao.Core/StartupTool.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace MasterZhaoCoreTests
{
	TEST_CLASS(MasterZhaoCoreTests)
	{
	public:

		TEST_METHOD(TestGetStartupItemList)
		{
			int count = 10;
			int size = sizeof(tagSTARTUPITEM) * count;
			byte* buffer = new byte[size];

			GetStartupItems(buffer, size, &count);

			delete[] buffer;
		}
	};
}