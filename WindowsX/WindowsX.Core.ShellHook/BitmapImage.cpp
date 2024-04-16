#include "pch.h"
#include "BitmapImage.h"

BitmapImage::BitmapImage(LPCTSTR szPath)
{
	hdc = NULL;
	hBitmap = NULL;
	memset(&size, 0, sizeof(size));
	bitmap = NULL;
		
	bitmap = new Gdiplus::Bitmap(szPath);

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
