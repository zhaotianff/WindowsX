#pragma once

#include "framework.h"

SILVERAROWANACORE_API BOOL ExtractIconFromFile(LPTSTR lpszExtPath, int nIconIndex, byte* buffer, int length);
SILVERAROWANACORE_API BOOL ExtractFirstIconFromFile(LPCTSTR lpszExtPath, BOOL isLargeIcon,HICON& icon);
SILVERAROWANACORE_API BOOL ExtractStockIcon(SHSTOCKICONID shIconID, HICON& icon);
SILVERAROWANACORE_API BOOL GetFileExtensionAssocIcon(LPCTSTR lpszExtension, HICON& icon);
SILVERAROWANACORE_API BOOL GetShell32Icon(int index, HICON& icon);