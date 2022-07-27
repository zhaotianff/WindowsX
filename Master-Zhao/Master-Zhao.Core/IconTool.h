#pragma once

#include "framework.h"

SILVERAROWANACORE_API BOOL ExtractIconFromFile(LPTSTR lpszExtPath, int nIconIndex, byte* buffer, int length);
SILVERAROWANACORE_API BOOL ExtractFirstIconFromFile(LPCTSTR lpszExtPath, BOOL isLargeIcon,HICON& icon);