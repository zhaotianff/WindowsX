#include"StartupTool.h"
#include "CoreUtil.h"
#include "DesktopTool.h"

BOOL IsExistStartupRun(LPTSTR lpszPath, LPTSTR* lpszLnkPath)
{
	//TODO check all registry

	if (lpszPath == NULL)
		return FALSE;

	if (GetFileNameWithoutExtension(&lpszPath) == FALSE)
		return FALSE;

	LPITEMIDLIST pIdList;
	SHGetSpecialFolderLocation(NULL, CSIDL_STARTUP, &pIdList);
	TCHAR szStartupPath[MAX_PATH]{};
	SHGetPathFromIDList(pIdList, szStartupPath);
	TCHAR szLnkPath[MAX_PATH]{};
	wsprintf(szLnkPath, L"%s\\%s.lnk", szStartupPath, lpszPath);
	*lpszLnkPath = szLnkPath;
	return PathFileExists(szLnkPath);
}

BOOL CreateStartupRun(LPTSTR lpszPath)
{
	LPTSTR szLnkPath = NULL;
	if (IsExistStartupRun(lpszPath, &szLnkPath) == TRUE)
		return TRUE;

	return CreateLink(lpszPath, szLnkPath, NULL, NULL);
}

BOOL RemoveStartupRun(LPTSTR lpszPath)
{
	//TODO check all registry
	LPTSTR szLnkPath = NULL;
	if (IsExistStartupRun(lpszPath, &szLnkPath))
		return DeleteFile(szLnkPath);
	return FALSE;
}

BOOL GetStartupItems(tagStartupItem** items, int count)
{
	return FALSE;
}
