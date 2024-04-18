#include "pch.h"
#include "BitmapImage.h"

HMODULE g_hShellHookDllModule;

BitmapImage::BitmapImage(LPCTSTR szPath)
{
	hdc = NULL;
	hBitmap = NULL;
	memset(&size, 0, sizeof(size));
	bitmap = NULL;
	opactiy = 255;
	stretchMode = 0;

	std::wstring strAbsolutePath = GetAbsoluteImagePath(szPath);

	FILE* fp = NULL;
	_wfopen_s(&fp, strAbsolutePath.data(), L"rb");
	if (fp)
	{
		fseek(fp, 2, 0);
		BYTE* readBuffer = new BYTE[5];
		fread(readBuffer, 1, 5, fp);

		if (readBuffer[0] == 0xFF && readBuffer[2] == 0x02)
		{
			opactiy = readBuffer[3];
			stretchMode = (int)readBuffer[4];
		}
		delete[] readBuffer;
	}

	bitmap = new Gdiplus::Bitmap(strAbsolutePath.data());

	if (bitmap)
	{
		hdc = CreateCompatibleDC(NULL);
		size.cx = (LONG)bitmap->GetWidth();
		size.cy = (LONG)bitmap->GetHeight();
		bitmap->GetHBITMAP(NULL, &hBitmap);
		SelectObject(hdc, hBitmap);

		delete bitmap;
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

std::wstring BitmapImage::GetAbsoluteImagePath(LPCTSTR szPath)
{
	TCHAR szAbsoluteFilePath[260]{};
	GetModuleFileName(g_hShellHookDllModule, szAbsoluteFilePath, 260);
	std::wstring str = szAbsoluteFilePath;
	size_t index = str.find_last_of('\\');
	str = str.substr(0, index + 1);
	str += szPath;
	return str;
}
