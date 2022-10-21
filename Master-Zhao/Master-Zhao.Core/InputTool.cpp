#include "InputTool.h"
#include <time.h>
#include<stdarg.h>

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

BOOL SendMultiAsciiInput(int nArgs, ...)
{
    std::vector<INPUT> inputs;

    va_list ap;
    va_start(ap, nArgs);
    for (int i = 0; i < nArgs; i++)
    {
        TCHAR c = va_arg(ap, TCHAR);

        INPUT inputDown{};

        inputDown.type = INPUT_KEYBOARD;
        inputDown.ki.wVk = c;
        inputs.push_back(inputDown);
    }
    va_end(ap);

    for (int i = 0; i < nArgs; i++)
    {
        INPUT inputUp{};
        inputUp.type = INPUT_KEYBOARD;
        inputUp.ki.wVk = inputs[i].ki.wVk;
        inputUp.ki.dwFlags = KEYEVENTF_KEYUP;
        inputs.push_back(inputUp);
    }

    return SendInput(nArgs * 2, inputs.data(), sizeof(INPUT));
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

VOID SendSearch()
{
    SendMultiAsciiInput(2, VK_LWIN, 'Q');
}

