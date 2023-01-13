#include "PowerTool.h"

VOID ShowShutDownDialog()
{
    //temp
    SwitchToDesktop();
    Sleep(50);
    SendMultiAsciiInput(2, VK_MENU, VK_F4);
}
