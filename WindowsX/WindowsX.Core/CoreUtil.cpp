#include"CoreUtil.h"

BOOL GetFileName(LPTSTR* lpszPath)
{
	std::wstring path = *lpszPath;
	auto length = path.size();
	auto index = length;
	while (--index > 0)
	{
		auto c = path[index];
		if (c == CHAR_BACKSLASH)
		{
			path = path.substr(index + 1, length - index - 1);
			*lpszPath = _tcsdup(path.c_str());
			return TRUE;
		}
	}
	return FALSE;
}

BOOL GetFileNameWithoutExtension(LPTSTR* lpszPath)
{
	if (GetFileName(lpszPath))
	{
		std::wstring path = *lpszPath;
		auto index = path.find_last_of(CHAR_DOT);
		path = path.substr(0, index);
		*lpszPath = _tcsdup(path.c_str());
		return TRUE;
	}
	return FALSE;
}
