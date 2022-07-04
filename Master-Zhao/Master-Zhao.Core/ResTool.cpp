#include "ResTool.h"

BOOL ExtractResourceToFile(int nResId, LPCTSTR lpszType, LPCTSTR lpszFile)
{
    HRSRC hrSrc = FindResource(NULL, MAKEINTRESOURCE(nResId), lpszType);
    HGLOBAL hRes = LoadResource(NULL,hrSrc);
    //LockResource does not actually lock memory; it is just used to obtain a pointer to the memory containing the resource data.
    LPVOID lpData = LockResource(hRes);

    HANDLE hFile = CreateFile(lpszFile, FILE_GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, 0, NULL);

    if (INVALID_HANDLE_VALUE == hFile)
        return FALSE;

    DWORD dwResSize = SizeofResource(NULL, hrSrc);
    DWORD dwWriteSize = 0;

    BOOL bWrite = WriteFile(hFile, lpData, dwResSize, &dwWriteSize, NULL);
    CloseHandle(hFile);
    return bWrite && dwResSize == dwWriteSize;
}
