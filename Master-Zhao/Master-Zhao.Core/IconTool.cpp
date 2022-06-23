#include "IconTool.h"

void SaveIcon(HICON hIconToSave, LPCTSTR sIconFileName)
{
    //if (hIconToSave == NULL || sIconFileName == NULL)
    //    return;
    ////warning: this code snippet is not bullet proof.
    ////do error check by yourself [masterz]
    //PICTDESC picdesc;
    //picdesc.cbSizeofstruct = sizeof(PICTDESC);
    //picdesc.picType = PICTYPE_ICON;
    //picdesc.icon.hicon = hIconToSave;
    //IPicture* pPicture = NULL;
    //OleCreatePictureIndirect(&picdesc, IID_IPicture, TRUE, (VOID**)&pPicture);
    //LPSTREAM pStream;
    //CreateStreamOnHGlobal(NULL, TRUE, &pStream);
    //LONG size;
    //HRESULT hr = pPicture->SaveAsFile(pStream, TRUE, &size);
    //TCHAR pathbuf[1024];
    //lstrcpy(pathbuf, sIconFileName);
    //CFile iconfile;
    //iconfile.Open(pathbuf, CFile::modeCreate | CFile::modeWrite);
    //LARGE_INTEGER li;
    //li.HighPart = 0;
    //li.LowPart = 0;
    //ULARGE_INTEGER ulnewpos;
    //pStream->Seek(li, STREAM_SEEK_SET, &ulnewpos);
    //ULONG uReadCount = 1;
    //while (uReadCount > 0)
    //{
    //    pStream->Read(pathbuf, sizeof(pathbuf), &uReadCount);
    //    if (uReadCount > 0)
    //        iconfile.Write(pathbuf, uReadCount);
    //}
    //pStream->Release();
    //iconfile.Close();
}

BOOL ExtractIcon(LPTSTR lpszExtPath, int nIconIndex, byte* buffer, int length)
{
    //https://docs.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-extracticonexa
    
    //LPTSTR lpszExtPath = _tcsdup(L"D:\\QQ\\Bin\\QQScLauncher.exe");
    HICON icon = (HICON)LoadImage(NULL, lpszExtPath, IMAGE_ICON, GetSystemMetrics(SM_CXSMICON), GetSystemMetrics(SM_CYSMICON), LR_LOADFROMFILE | LR_LOADMAP3DCOLORS);
    if (!icon)
    {
        ExtractIconEx(lpszExtPath, 0, &icon, NULL, 1);
    }

    return FALSE;  
}
