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

BOOL GetStartupItems(byte* buffer,int nSizeTarget, int* count)
{
    HKEY wow64_32Key = NULL;
    RegOpenKeyEx(HKEY_LOCAL_MACHINE, RUN_REGPATH, 0, KEY_READ | KEY_WOW64_32KEY, &wow64_32Key);

    std::vector<STARTUPITEM> totalVector;

    if (wow64_32Key)
    {
        auto list = InternalGetStartupItemList(wow64_32Key);
        RegCloseKey(wow64_32Key);
        totalVector.insert(totalVector.begin(), list.begin(),list.end());
    }

    HKEY wow64_64Key = NULL;
    RegOpenKeyEx(HKEY_LOCAL_MACHINE, RUN_REGPATH, 0, KEY_READ | KEY_WOW64_64KEY, &wow64_64Key);

    if (wow64_64Key)
    {
        auto list2 = InternalGetStartupItemList(wow64_64Key);
        RegCloseKey(wow64_64Key);
        totalVector.insert(totalVector.begin(), list2.begin(), list2.end());
    }

    if (*count < totalVector.size())
        return FALSE;

    *count = totalVector.size();
    auto nSizeSource = *count * sizeof(tagSTARTUPITEM);

    if (count == 0)
        return FALSE;

    memcpy_s(buffer, nSizeTarget,totalVector.data(),nSizeSource);

    //test code
    /* for (int i = 0; i < 5; i++)
     {
         tagSTARTUPITEM* item = (tagSTARTUPITEM*)buffer;
    
         buffer += sizeof(tagSTARTUPITEM);
     }*/

    return TRUE;
}

std::vector<STARTUPITEM> InternalGetStartupItemList(HKEY hKeyStartupKey)
{
    std::vector<STARTUPITEM> lstStartup;
    TCHAR  achValue[MAX_VALUE_NAME];
    DWORD cchValue = MAX_VALUE_NAME;
    TCHAR    achKey[MAX_KEY_LENGTH];   
    DWORD    cbName;                 
    TCHAR    achClass[MAX_PATH] = TEXT("");   
    DWORD    cchClassName = MAX_PATH;  
    DWORD    cSubKeys = 0;             
    DWORD    cbMaxSubKey;              
    DWORD    cchMaxClass;             
    DWORD    cValues;             
    DWORD    cchMaxValue;          
    DWORD    cbMaxValueData;      
    DWORD    cbSecurityDescriptor; 
    FILETIME ftLastWriteTime;      
    DWORD i, retCode;

    retCode = RegQueryInfoKey(hKeyStartupKey, achClass, &cchClassName, NULL, &cSubKeys, &cbMaxSubKey, &cchMaxClass, &cValues, &cchMaxValue, &cbMaxValueData, &cbSecurityDescriptor, &ftLastWriteTime);

    if (cValues)
    {
        for (i = 0, retCode = ERROR_SUCCESS; i < cValues; i++)
        {
            cchValue = MAX_VALUE_NAME;
            achValue[0] = '\0';
            retCode = RegEnumValue(hKeyStartupKey, i, achValue, &cchValue, NULL, NULL, NULL, NULL);

            if (retCode == ERROR_SUCCESS)
            {
                STARTUPITEM item;
                
                StringCchCopy(item.szName, MAX_VALUE_NAME, achValue);

                DWORD dwPathSize = MAX_PATH;
                item.szPath[0] = '\0';
                QuerySZValue(hKeyStartupKey, NULL, achValue, item.szPath, &dwPathSize);

                lstStartup.push_back(item);
            }
        }
    }

    return lstStartup;
}
