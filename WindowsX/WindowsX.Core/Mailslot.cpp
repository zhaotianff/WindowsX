#include "Mailslot.h"

SILVERAROWANACORE_API BOOL WriteToMailslot(LPCTSTR lpszSlotName, LPCTSTR lpszMessage)
{
    HANDLE hFile = CreateFile(lpszSlotName, GENERIC_WRITE,
        FILE_SHARE_READ, NULL, OPEN_EXISTING,
        FILE_ATTRIBUTE_NORMAL, NULL);

    if (hFile == INVALID_HANDLE_VALUE)
    {
        return FALSE;
    }

    DWORD cbWritten = 0;
    BOOL bResult = WriteFile(hFile, lpszMessage, (DWORD)(lstrlen(lpszMessage) * sizeof(TCHAR)),
        &cbWritten, NULL);

    if (!bResult)
    {
        CloseHandle(hFile);
        return FALSE;
    }
    else
    {
        CloseHandle(hFile);
        return TRUE;
    }
}
