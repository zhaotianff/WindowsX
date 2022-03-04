#include"StartupTool.h"

BOOL CreateStartupRun(LPTSTR lpszPath)
{
	LPITEMIDLIST pIdList;
	SHGetSpecialFolderLocation(NULL, CSIDL_STARTUP, &pIdList);
	TCHAR szStartupPath[MAX_PATH]{};
	SHGetPathFromIDList(pIdList, szStartupPath);
	LPTSTR szSearchPattern = lstrcat(szStartupPath, L"\\*.lnk");
	WIN32_FIND_DATA data;
	HANDLE hFind = FindFirstFile(szSearchPattern, &data);

	if (INVALID_HANDLE_VALUE == hFind)
		return FALSE;

	return FALSE;
}

BOOL IsExistStartupRun(LPTSTR lpszExeName)
{
	return FALSE;
}