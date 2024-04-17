#pragma once

#include"framework.h"

class BitmapImage
{
public:
	BitmapImage(LPCTSTR szPath);
	~BitmapImage();
	std::wstring GetAbsoluteImagePath(LPCTSTR szPath);

	HDC hdc;
	HBITMAP hBitmap;
	SIZE size;
	Gdiplus::Bitmap* bitmap;
};
