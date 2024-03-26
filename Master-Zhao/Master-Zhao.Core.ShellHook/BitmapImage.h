#pragma once

#include"framework.h"

class BitmapImage
{
public:
	BitmapImage(LPCTSTR szPath);
	~BitmapImage();

	HDC hdc;
	HBITMAP hBitmap;
	SIZE size;
	Gdiplus::Bitmap* bitmap;
};
