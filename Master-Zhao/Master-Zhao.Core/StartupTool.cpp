#include"StartupTool.h"
#include "CoreUtil.h"
#include "DesktopTool.h"
#include "SystemTool.h"
#include"StringHelper.h"

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

    //run wow64_64
    HKEY wow64_64Key = NULL;
    RegOpenKeyEx(HKEY_LOCAL_MACHINE, RUN_REGPATH, 0, KEY_READ | KEY_WOW64_64KEY, &wow64_64Key);

    if (wow64_64Key)
    {
        auto list2 = InternalGetStartupItemList(wow64_64Key, HKEY_LOCAL_MACHINE, RUN_REGPATH, KEY_READ | KEY_WOW64_64KEY,TRUE);
        RegCloseKey(wow64_64Key);
        totalVector.insert(totalVector.end(), list2.begin(), list2.end());
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

    //current user run 
    HKEY run_user_Key = NULL;
    RegOpenKeyEx(HKEY_CURRENT_USER, RUN_USER_REGPATH, 0, KEY_READ, &run_user_Key);

    if (run_user_Key)
    {
        auto list4 = InternalGetStartupItemList(run_user_Key, HKEY_CURRENT_USER, RUN_USER_REGPATH, KEY_READ , TRUE);
        RegCloseKey(run_user_Key);
        totalVector.insert(totalVector.end(), list4.begin(), list4.end());
    }

    TCHAR szStartupPath[MAX_PATH]{};
    BOOL result = GetSpeicalFolder(CSIDL_STARTUP, szStartupPath);
    if (result)
    {
        auto list5 = InternalGetStartupItemListFromShell(szStartupPath,TRUE);
        totalVector.insert(totalVector.end(), list5.begin(), list5.end());
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

BOOL GetStartupDisabledItems(byte* buffer, int nSizeTarget, int* count)
{
    std::vector<STARTUPITEM> totalVector;

    //run local machine run
    auto lstRun = InternalGetStartupItemList(HKEY_LOCAL_MACHINE, RUN_REGPATH_DISABLE);
    if (lstRun.first == true)
        totalVector.insert(totalVector.end(), lstRun.second.begin(), lstRun.second.end());

    auto lstRun32 = InternalGetStartupItemList(HKEY_LOCAL_MACHINE, RUN_REGPATH_DISABLE_RUN32);
    if (lstRun32.first == true)
        totalVector.insert(totalVector.end(), lstRun32.second.begin(), lstRun32.second.end());

    auto lstRunStartup = InternalGetStartupItemList(HKEY_LOCAL_MACHINE, RUN_REGPATH_DISABLE_STARTUPFOLDER, STARTUPITEM_TYPE::ShellStartup);
    if (lstRunStartup.first == true)
        totalVector.insert(totalVector.end(), lstRunStartup.second.begin(), lstRunStartup.second.end());

    if (*count < totalVector.size())
        return FALSE;

    *count = totalVector.size();
    auto nSizeSource = *count * sizeof(tagSTARTUPITEM);

    if (count == 0)
        return FALSE;

    memcpy_s(buffer, nSizeTarget, totalVector.data(), nSizeSource);
    return TRUE;
}

std::pair<bool, std::vector<STARTUPITEM>> InternalGetStartupItemList(HKEY hKeyRoot, LPCTSTR szRegPath, STARTUPITEM_TYPE startupType)
{
    std::vector<STARTUPITEM> lstStartItem;
    HKEY wow64_32Key = NULL;
    RegOpenKeyEx(hKeyRoot, szRegPath, 0, KEY_READ, &wow64_32Key);
    bool reg32 = false;
    if (wow64_32Key)
    {
        auto lst32 = InternalGetStartupItemList(wow64_32Key, hKeyRoot, szRegPath, KEY_READ, TRUE);
        RegCloseKey(wow64_32Key);

        if (lst32.size() > 0)
            lstStartItem.insert(lstStartItem.end(), lst32.begin(), lst32.end());

        reg32 = true;
    }

    return std::make_pair(reg32, lstStartItem);
}

std::vector<STARTUPITEM> InternalGetStartupItemList(HKEY hKeyStartupKey, HKEY hKeyRoot, LPCTSTR szRegPath, DWORD samDesired,BOOL bEnabled, STARTUPITEM_TYPE startupType)
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

                //TODO pass parameter to get binary/sz value
                if (Contains(szRegPath, L"StartupApproved"))
                {
                    BYTE* buffer = new BYTE[1024];
                    DWORD nBufferSize = 1024;
                    if (QueryByteValue(hKeyStartupKey, NULL, achValue, buffer, &nBufferSize))
                    {
                        if (InternalGetIsEnableItem(buffer, nBufferSize))
                        {
                            StringCchCopy(item.szPath, MAX_VALUE_NAME, L"Enable");
                        }
                        else
                        {
                            StringCchCopy(item.szPath, MAX_VALUE_NAME, L"Disable");
                        }
                    }
                }
                else
                {
                    QuerySZValue(hKeyStartupKey, NULL, achValue, item.szPath, &dwPathSize);
                }
              

                item.szDescription[0] = '\0';
                auto szFileDescription = GetFileDescrption(item.szPath);

                if(szFileDescription)
                    StringCchCopy(item.szDescription, MAX_VALUE_NAME, szFileDescription);

                StringCchCopy(item.szRegPath, MAX_VALUE_NAME, szRegPath);
                item.samDesired = samDesired;
                item.bEnabled = bEnabled;
                item.type = startupType;

                lstStartup.push_back(item);
            }
        }
    }

    return lstStartup;
}

BOOL InternalGetStartupItemFromFile(PSTARTUPITEM item, LPTSTR szFile)
{
    TCHAR szRealPath[MAX_PATH]{};
    HRESULT hr =  GetShortcutPath(szFile, szRealPath, MAX_PATH);

    if (FAILED(hr))
        return FALSE;

    LPTSTR szDescription = GetFileDescrption(szRealPath);

    if(szDescription)
        StringCchCopy(item->szDescription, MAX_VALUE_NAME, szDescription);

    StringCchCopy(item->szPath, MAX_VALUE_NAME, szFile);
    item->type = STARTUPITEM_TYPE::ShellStartup;
    if (GetFileNameWithoutExtension(&szFile))
    {
        StringCchCopy(item->szName, MAX_PATH, szFile);
    }

    return TRUE;
}

std::vector<STARTUPITEM> InternalGetStartupItemListFromShell(LPTSTR szStartupPath,BOOL isEnable)
{
    std::vector<STARTUPITEM> list;
    std::wstring strSearchPattern = szStartupPath;
    strSearchPattern += L"\\*.lnk";

    WIN32_FIND_DATA find_data;
    auto hFind = FindFirstFile(strSearchPattern.data(), &find_data);

    if (INVALID_HANDLE_VALUE == hFind)
    {
        return list;
    }

    do
    {
        if (!(find_data.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY))
        {
            STARTUPITEM item{};
            item.bEnabled = isEnable;

            TCHAR szLnkPath[MAX_PATH]{};

            StringCchCat(szLnkPath, MAX_PATH, szStartupPath);
            StringCchCat(szLnkPath, MAX_PATH, L"\\");
            StringCchCat(szLnkPath, MAX_PATH, find_data.cFileName);
            if (InternalGetStartupItemFromFile(&item, szLnkPath))
            {
                list.push_back(item);
            }
        }

    } while ( FindNextFile(hFind,&find_data) != 0);

    FindClose(hFind);

    return list;
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

BOOL DisableShellStartupItem(LPTSTR szName, LPTSTR szPath)
{
    TCHAR szDisabledPath[MAX_PATH]{};
    GetSpeicalFolder(CSIDL_STARTUP, szDisabledPath);
    StringCchCat(szDisabledPath, MAX_PATH, L"\\Disabled\\");
    StringCchCat(szDisabledPath, MAX_PATH, szName);
    StringCchCat(szDisabledPath, MAX_PATH, L".lnk");
    return MoveFile(szPath, szDisabledPath);
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

BOOL EnableShellStartupItem(LPTSTR szName, LPTSTR szPath)
{
    TCHAR szEnabledPath[MAX_PATH]{};
    GetSpeicalFolder(CSIDL_STARTUP, szEnabledPath);
    StringCchCat(szEnabledPath, MAX_PATH, L"\\");
    StringCchCat(szEnabledPath, MAX_PATH, szName);
    StringCchCat(szEnabledPath, MAX_PATH, L".lnk");
    return MoveFile(szPath, szEnabledPath);
}

BOOL InternalGetIsEnableItem(BYTE* byteData, DWORD nSize)
{
    BYTE bytesEnable[] = { 06 ,00, 00, 00 ,00 ,00 };
    return memcmp(byteData, bytesEnable, 6) == 0;
}
