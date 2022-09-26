#pragma once

#include"framework.h"

enum class Enm_BackgroundPos : DWORD
{
	LeftTop = 0,
	RightTop = 1,
	LeftBottom = 2,
	RightBottom = 3,
	Center = 4,
	Uniform = 5,
	UniformToFill = 6
};

Enm_BackgroundPos enm_Pos = Enm_BackgroundPos::RightBottom;
DWORD dw_Alpha = 255;

SILVERAROWANACORE_API BOOL SetExplorerBackground(LPTSTR szPath, DWORD alpha, Enm_BackgroundPos pos);
SILVERAROWANACORE_API VOID SetExplorerBackgroundParam(DWORD alpha, Enm_BackgroundPos pos);
