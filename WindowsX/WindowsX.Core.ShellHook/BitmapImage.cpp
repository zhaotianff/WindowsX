#include "pch.h"
#include "BitmapImage.h"

HMODULE g_hShellHookDllModule;

BitmapImage::BitmapImage(LPCTSTR szPath)
{
	hdc = NULL;
	hBitmap = NULL;
	memset(&size, 0, sizeof(size));
	bitmap = NULL;
	
	std::wstring strAbsolutePath = GetAbsoluteImagePath(szPath);
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

	//MessageBox(NULL, str.data(), L"", MB_OK);
	return str;
}
