#include "AppTool.h"

BOOL GetAppPath(LPTSTR szBuffer, DWORD nSize)
{
    auto appList = GetAllSubKey(HKEY_LOCAL_MACHINE, APP_PATH);
    
    TCHAR szValue[260];
    DWORD dwSize = 260;
    std::wstring buffer;

    for(auto& app : appList)
    {
        std::wstring appStr = APP_PATH + app;
        szValue[0] = '\0';
        dwSize = 260;

        //TODO

        QuerySZValue(HKEY_LOCAL_MACHINE, appStr.data(), L"", szValue, &dwSize);

        if (lstrlen(szValue) == 0)
            continue;

        buffer = buffer + app + L"-" + szValue + L";";
    }

    if (buffer.length() == 0)
        return FALSE;

    StringCchCopy(szBuffer, nSize, buffer.data());

    return TRUE;
}

BOOL AddAppPath(LPTSTR szName, LPTSTR szPath)
{
    return 0;
}
