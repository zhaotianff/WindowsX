#include "pch.h"
#include "CppUnitTest.h"
#include "../WindowsX.Core/IconTool.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace WindowsXCoreTests
{
	TEST_CLASS(WindowsXCoreTests)
	{
	public:

		TEST_METHOD(TestGetFileIcon)
		{
			HICON icon = NULL;
			ExtractFirstIconFromFile(L"C:\\Windows\\notepad.exe", TRUE, icon);
			Assert::IsNotNull(icon);
		}

		TEST_METHOD(TestGetFileExtensionAssocIcon)
		{
			HICON icon = NULL;
			GetFileExtensionAssocIcon(L".pdf", icon);
			Assert::IsNotNull(icon);
		}
	};
}
		