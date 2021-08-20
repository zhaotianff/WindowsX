#include "RegTool.h"

BOOL SetValue(HKEY hKey,LPCTSTR lpSubKey,LPCTSTR lpValueName, BYTE* value)
{
	HKEY hSubKey = NULL;
	auto lResult = RegOpenKeyEx(hKey, lpSubKey, 0, KEY_ALL_ACCESS, &hSubKey);

	if(lResult != ERROR_SUCCESS)
	{
		return FALSE;		
	}

	lResult = RegSetValueEx(hSubKey, lpValueName, NULL, REG_DWORD, value, sizeof(value));

	if (lResult != ERROR_SUCCESS)
	{
		return FALSE;
	}

	if (hSubKey)
	{
		lResult = RegCloseKey(hSubKey);
		if (lResult != ERROR_SUCCESS)
			return FALSE;
	}

	return TRUE;
}
