#include "InputTool.h"

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

VOID AutoCode(TCHAR code)
{
    keybd_event(code, 0x45, NULL, NULL);
    keybd_event(code, 0x45, KEYEVENTF_KEYUP, NULL);
}
