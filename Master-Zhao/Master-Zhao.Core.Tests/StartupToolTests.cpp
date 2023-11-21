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

		TEST_METHOD(TestGetStartupDisableItemList)
		{
			int count = 10;
			int size = sizeof(tagSTARTUPITEM) * count;
			byte* buffer = new byte[size]{};

			auto result = GetStartupDisabledItems(buffer, size, &count);
			Assert::IsTrue(result);
			delete[] buffer;
			buffer = nullptr;
		}

		TEST_METHOD(TestDisableStartupItem)
		{
			int count = 10;
			int size = sizeof(tagSTARTUPITEM) * count;
			byte* buffer = new byte[size];

			GetStartupItems(buffer, size, &count);

			byte* offsetBuffer = buffer + sizeof(tagSTARTUPITEM) * 5;

			tagSTARTUPITEM* item = (tagSTARTUPITEM*)offsetBuffer;

			//registry
			auto result = DisableStartupItem(item->hKey, item->szRegPath, item->samDesired, item->szName,STARTUPITEM_TYPE::Registry);
			//shellstartup
			//auto result = DisableStartupItem(item->hKey, item->szPath, item->samDesired, item->szName, STARTUPITEM_TYPE::ShellStartup);

			delete[] buffer;

			Assert::IsTrue(result);
		}

		TEST_METHOD(TestEnableStartupItem)
		{
			int count = 10;
			int size = sizeof(tagSTARTUPITEM) * count;
			byte* buffer = new byte[size];

			GetStartupItems(buffer, size, &count);

			byte* offsetBuffer = buffer + sizeof(tagSTARTUPITEM) * 5;

			tagSTARTUPITEM* item = (tagSTARTUPITEM*)offsetBuffer;

			auto result = EnableStartupItem(item->hKey, item->szRegPath, item->samDesired, item->szName, item->type);

			delete[] buffer;

			Assert::IsTrue(result);
		}

		TEST_METHOD(TestDisableShellStartupItem)
		{
			int count = 10;
			int size = sizeof(tagSTARTUPITEM) * count;
			byte* buffer = new byte[size];

			GetStartupItems(buffer, size, &count);

			auto tempBuffer = buffer;

			tempBuffer += sizeof(tagSTARTUPITEM) * 2;

			tagSTARTUPITEM* item = (tagSTARTUPITEM*)tempBuffer;

			auto result = DisableShellStartupItem(item->szName, item->szPath);

			delete[] buffer;

			Assert::IsTrue(result);
		}

		TEST_METHOD(TestEnableShellStartupItem)
		{
			int count = 10;
			int size = sizeof(tagSTARTUPITEM) * count;
			byte* buffer = new byte[size];

			GetStartupItems(buffer, size, &count);

			auto tempBuffer = buffer;

			tempBuffer += sizeof(tagSTARTUPITEM) * 2;

			tagSTARTUPITEM* item = (tagSTARTUPITEM*)tempBuffer;

		    auto result = EnableShellStartupItem(item->szName, item->szPath);

			delete[] buffer;

			Assert::IsTrue(result);
		}


	};
}