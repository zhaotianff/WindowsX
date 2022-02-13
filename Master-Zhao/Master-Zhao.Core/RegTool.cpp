#include "RegTool.h"

BOOL SetDWORDValue(HKEY hKey, LPCTSTR lpSubKey, LPCTSTR lpValueName, DWORD value)
{
	HKEY hSubKey = NULL;
	auto lResult = RegOpenKeyEx(hKey, lpSubKey, 0, KEY_ALL_ACCESS, &hSubKey);

	if (lResult != ERROR_SUCCESS)
	{
		return FALSE;
	}

	lResult = RegSetValueEx(hSubKey, lpValueName, 0, REG_DWORD, (const BYTE*)&value, sizeof(value));

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

BOOL SetSZValue(HKEY hKey, LPCTSTR lpSubKey, LPCTSTR lpValueName, TCHAR* value)
{
	HKEY hSubKey = NULL;
	auto lResult = RegOpenKeyEx(hKey, lpSubKey, 0, KEY_ALL_ACCESS, &hSubKey);

	if (lResult != ERROR_SUCCESS)
	{
		return FALSE;
	}

	lResult = RegSetValueEx(hSubKey, lpValueName, 0, REG_SZ, (const BYTE*)value, lstrlen(value) * sizeof(TCHAR));

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

BOOL ExistSubKey(HKEY hKey, LPCTSTR lpSubKey)
{
	HKEY hSubKey = NULL;
	RegOpenKeyEx(hKey, lpSubKey,0, KEY_READ, &hSubKey);
	BOOL result = hSubKey == NULL ? FALSE : TRUE;
	if (hSubKey)
		RegCloseKey(hSubKey);
	return result;
}

BOOL QueryDWORDValue(HKEY hKey, LPCTSTR lpSubKey, LPCTSTR lpValueName, DWORD* value)
{
	DWORD length = sizeof(value);
	auto result = RegOpenKeyEx(hKey, lpSubKey, 0, KEY_READ, &hKey);
	if (ERROR_SUCCESS == result)
	{		
		result = RegQueryValueEx(hKey, lpValueName, 0, NULL, (BYTE*)value, &length);
		if (ERROR_SUCCESS == result)
		{
			RegCloseKey(hKey);
			return TRUE;
		}	
	}
	return FALSE;
}
