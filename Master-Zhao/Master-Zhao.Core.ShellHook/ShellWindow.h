#pragma once

typedef struct _ShellWindow
{
	HWND hwnd;
	HDC hdc;
	SIZE size;
}ShellWindow,*PShellWindow;