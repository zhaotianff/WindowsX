#include"StartupTool.h"
#include "CoreUtil.h"
#include "DesktopTool.h"
#include "SystemTool.h"

BOOL IsExistStartupRun(LPTSTR lpszPath, LPTSTR* lpszLnkPath)
{
	//TODO check all registry

	if (lpszPath == NULL)
		return FALSE;

	if (GetFileNameWithoutExtension(&lpszPath) == FALSE)
		return FALSE;

	LPITEMIDLIST pIdList;
	HRESULT hr = SHGetSpecialFolderLocation(NULL, CSIDL_STARTUP, &pIdList);
    if (FAILED(hr))
        return FALSE;
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
    std::vector<STARTUPITEM> totalVector;

    //run wow64_32
    HKEY wow64_32Key = NULL;
    RegOpenKeyEx(HKEY_LOCAL_MACHINE, RUN_REGPATH, 0, KEY_READ | KEY_WOW64_32KEY, &wow64_32Key);
    if (wow64_32Key)
    {
        auto list = InternalGetStartupItemList(wow64_32Key, HKEY_LOCAL_MACHINE, RUN_REGPATH, KEY_READ | KEY_WOW64_32KEY,TRUE);
        RegCloseKey(wow64_32Key);
        totalVector.insert(totalVector.end(), list.begin(),list.end());
    }

    HKEY wow64_32Key_disable = NULL;
    RegOpenKeyEx(HKEY_LOCAL_MACHINE, RUN_REGPATH_DISABLE, 0, KEY_READ | KEY_WOW64_32KEY, &wow64_32Key_disable);
    if (wow64_32Key_disable)
    {
        auto list_disable = InternalGetStartupItemList(wow64_32Key_disable, HKEY_LOCAL_MACHINE, RUN_REGPATH_DISABLE, KEY_READ | KEY_WOW64_32KEY, FALSE);
        RegCloseKey(wow64_32Key_disable);
        totalVector.insert(totalVector.end(), list_disable.begin(), list_disable.end());
    }

    //run wow64_64
    HKEY wow64_64Key = NULL;
    RegOpenKeyEx(HKEY_LOCAL_MACHINE, RUN_REGPATH, 0, KEY_READ | KEY_WOW64_64KEY, &wow64_64Key);

    if (wow64_64Key)
    {
        auto list2 = InternalGetStartupItemList(wow64_64Key, HKEY_LOCAL_MACHINE, RUN_REGPATH, KEY_READ | KEY_WOW64_64KEY,TRUE);
        RegCloseKey(wow64_64Key);
        totalVector.insert(totalVector.end(), list2.begin(), list2.end());
    }

    HKEY wow64_64Key_disable = NULL;
    RegOpenKeyEx(HKEY_LOCAL_MACHINE, RUN_REGPATH_DISABLE, 0, KEY_READ | KEY_WOW64_64KEY, &wow64_64Key_disable);

    if (wow64_64Key_disable)
    {
        auto list2_disable = InternalGetStartupItemList(wow64_64Key_disable, HKEY_LOCAL_MACHINE, RUN_REGPATH_DISABLE, KEY_READ | KEY_WOW64_64KEY, FALSE);
        RegCloseKey(wow64_64Key_disable);
        totalVector.insert(totalVector.end(), list2_disable.begin(), list2_disable.end());
    }

    //run once wow64_32
    HKEY runonce64_32_Key = NULL;
    RegOpenKeyEx(HKEY_LOCAL_MACHINE, RUN_ONCE_REGPATH, 0, KEY_READ | KEY_WOW64_32KEY, &runonce64_32_Key);

    if (runonce64_32_Key)
    {
        auto list3 = InternalGetStartupItemList(runonce64_32_Key, HKEY_LOCAL_MACHINE, RUN_ONCE_REGPATH, KEY_READ | KEY_WOW64_32KEY,TRUE);
        RegCloseKey(runonce64_32_Key);
        totalVector.insert(totalVector.end(), list3.begin(), list3.end());
    }

    HKEY runonce64_32_Key_disable = NULL;
    RegOpenKeyEx(HKEY_LOCAL_MACHINE, RUN_ONCE_REGPATH_DISABLE, 0, KEY_READ | KEY_WOW64_32KEY, &runonce64_32_Key_disable);

    if (runonce64_32_Key_disable)
    {
        auto list3_disable = InternalGetStartupItemList(runonce64_32_Key_disable, HKEY_LOCAL_MACHINE, RUN_ONCE_REGPATH_DISABLE, KEY_READ | KEY_WOW64_32KEY, FALSE);
        RegCloseKey(runonce64_32_Key_disable);
        totalVector.insert(totalVector.end(), list3_disable.begin(), list3_disable.end());
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

std::vector<STARTUPITEM> InternalGetStartupItemList(HKEY hKeyStartupKey, HKEY hKeyRoot, LPCTSTR szRegPath, DWORD samDesired,BOOL bEnabled)
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
                item.hKey = hKeyRoot;
                StringCchCopy(item.szName, MAX_VALUE_NAME, achValue);
                DWORD dwPathSize = MAX_PATH;
                item.szPath[0] = '\0';
                QuerySZValue(hKeyStartupKey, NULL, achValue, item.szPath, &dwPathSize);

                item.szDescription[0] = '\0';
                auto szFileDescription = GetFileDescrption(item.szPath);

                if(szFileDescription)
                    StringCchCopy(item.szDescription, MAX_VALUE_NAME, szFileDescription);

                StringCchCopy(item.szRegPath, MAX_VALUE_NAME, szRegPath);
                item.samDesired = samDesired;
                item.bEnabled = bEnabled;

                lstStartup.push_back(item);
            }
        }
    }

    return lstStartup;
}

BOOL DisableStartupItem(HKEY hKey, LPTSTR szRegPath, DWORD samDesired, LPTSTR szName, LPTSTR szPath)
{
    std::wstring strDisabledPath = szRegPath;
    strDisabledPath += L"\\Disabled";
    HKEY hSubKey = NULL;
    auto lResult = RegOpenKeyEx(hKey, strDisabledPath.data(), 0, samDesired, &hSubKey);
    RegCloseKey(hSubKey);

    if (lResult == ERROR_FILE_NOT_FOUND)
    {
        lResult = RegCreateKey(hKey, strDisabledPath.data(), &hSubKey);
    }

    if (lResult != ERROR_SUCCESS)
        return FALSE;

    //TODO backup register

    BOOL bCreate = SetSZValue(hKey, strDisabledPath.data(), szName, szPath);
    BOOL bRemove = RemovRegValue(hKey, szRegPath, szName);

    return bCreate && bRemove;
}

BOOL EnableStartupItem(HKEY hKey, LPTSTR szRegPath, DWORD samDesired, LPTSTR szName, LPTSTR szPath)
{
    std::wstring strEnabledPath = szRegPath;
    strEnabledPath = strEnabledPath.substr(0, strEnabledPath.length() - lstrlen(L"\\Disabled"));
    HKEY hSubKey = NULL;
    auto lResult = RegOpenKeyEx(hKey, strEnabledPath.data(), 0, samDesired, &hSubKey);
    RegCloseKey(hSubKey);

    if (lResult == ERROR_FILE_NOT_FOUND)
    {
        return FALSE;
    }

    BOOL bCreate = SetSZValue(hKey, strEnabledPath.data(), szName, szPath);
    BOOL bRemove = RemovRegValue(hKey, szRegPath, szName);

    return bCreate && bRemove;
}
