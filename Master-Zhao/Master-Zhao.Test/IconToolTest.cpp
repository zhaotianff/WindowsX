#include"IconToolTest.h"

void TestGetFileIcon()
{
	HICON icon = NULL;
	ExtractFirstIconFromFile(L"C:\\Windows\\notepad.exe", TRUE, icon);
}