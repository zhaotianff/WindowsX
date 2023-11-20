#include "StringHelper.h"

BOOL Contains(LPCTSTR szRaw, LPCTSTR szKey)
{
    std::wstring str = szRaw;
    return str.find(szKey) != std::wstring::npos;
}
