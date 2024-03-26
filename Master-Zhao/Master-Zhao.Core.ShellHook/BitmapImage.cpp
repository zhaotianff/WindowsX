#include "pch.h"
#include "BitmapImage.h"

BitmapImage::BitmapImage(LPCTSTR szPath)
{
	hdc = NULL;
	hBitmap = NULL;
	memset(&size, 0, sizeof(size));
	bitmap = NULL;

	HANDLE hFile =  CreateFile(szPath, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
	if (hFile)
	{
		DWORD dwSize = 0;
		DWORD dwRead = 0;
		BOOL bResult = TRUE;
		GetFileSize(hFile, &dwSize);
		BYTE* buffer = new BYTE[dwSize];
		bResult = ReadFile(hFile, buffer, dwSize, &dwRead, NULL);

		if (bResult)
		{
			IStream* stream = SHCreateMemStream(buffer, dwSize);
			bitmap = Gdiplus::Bitmap::FromStream(stream);

			if (bitmap)
			{
				hdc = CreateCompatibleDC(NULL);
				size.cx = (LONG)bitmap->GetWidth();
				size.cy = (LONG)bitmap->GetHeight();
				bitmap->GetHBITMAP(NULL, &hBitmap);
				SelectObject(hdc, hBitmap);

				stream->Release();
			}
		}

		delete[] buffer;
		CloseHandle(hFile);
	}
	
}

BitmapImage::~BitmapImage()
{
	if (bitmap)
		delete bitmap;

	if (hdc)
		DeleteDC(hdc);

	if (hBitmap)
		DeleteObject(hBitmap);
}
