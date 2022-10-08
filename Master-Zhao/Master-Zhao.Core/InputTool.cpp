#include "InputTool.h"
#include <time.h>

SILVERAROWANACORE_API int GetRawInput(LPARAM lParam)
{
    RAWINPUT rawInputData{};
    UINT uiSize = sizeof(rawInputData);
    GetRawInputData((HRAWINPUT)lParam, RID_INPUT, &rawInputData, &uiSize, sizeof(RAWINPUTHEADER));
    if (RIM_TYPEKEYBOARD == rawInputData.header.dwType)
    {
        if (WM_KEYDOWN == rawInputData.data.keyboard.Message)
        {
            return rawInputData.data.keyboard.VKey;
        }
    }
    return -1;
}

BOOL IsKeyPressed(int vKey)
{
    auto state = GetAsyncKeyState(vKey);

    return (USHORT)state >> 15 == 1;
}

BOOL SendUnicodeInput(TCHAR c)
{    
    INPUT inputs[2]{};
    inputs[0].type = INPUT_KEYBOARD;
    inputs[0].ki.wScan = c;
    inputs[0].ki.wVk = 0;
    inputs[0].ki.dwFlags = KEYEVENTF_UNICODE; 

    inputs[1].type = INPUT_KEYBOARD;
    inputs[1].ki.wScan = c;
    inputs[1].ki.wVk = 0;
    inputs[1].ki.dwFlags = KEYEVENTF_KEYUP | KEYEVENTF_UNICODE;

    return SendInput(2, inputs, sizeof(INPUT));  
}

BOOL SendAsciiInput(TCHAR c)
{
    INPUT inputs[2]{};
    inputs[0].type = INPUT_KEYBOARD;
    inputs[0].ki.wVk = c;

    inputs[1].type = INPUT_KEYBOARD;
    inputs[1].ki.wVk = c;
    inputs[1].ki.dwFlags = KEYEVENTF_KEYUP;

    return SendInput(2, inputs, sizeof(INPUT));
}

BOOL SendAsciiInputUp(TCHAR c)
{
    INPUT inputs[1]{};
    inputs[0].type = INPUT_KEYBOARD;
    inputs[0].ki.wVk = c;
    inputs[0].ki.dwFlags = KEYEVENTF_KEYUP;
    return SendInput(1, inputs, sizeof(INPUT));
}

VOID AutoCode(LPTSTR code)
{
    int nLen = lstrlen(code);

    for (int i = 0; i < nLen; i++)
    {
        srand(time(NULL));
        Sleep(rand() % 100);

        TCHAR c = code[i];

        if (c == L'\n')
        {
            SendAsciiInput(VK_RETURN);
        }
        else
        {
            SendUnicodeInput(code[i]);
        }  
    }
}

