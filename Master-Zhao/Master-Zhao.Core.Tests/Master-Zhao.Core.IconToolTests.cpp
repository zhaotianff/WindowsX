#include "pch.h"
#include "CppUnitTest.h"
#include "../Master-Zhao.Core/IconTool.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace MasterZhaoCoreTests
{
	TEST_CLASS(MasterZhaoCoreTests)
	{
	public:

		TEST_METHOD(TestGetFileIcon)
		{
			HICON icon = NULL;
			ExtractFirstIconFromFile(L"C:\\Windows\\notepad.exe", TRUE, icon);
			Assert::IsNotNull(icon);
		}
	};
}
		