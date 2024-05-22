#include "PowerTool.h"

VOID ShowShutDownDialog()
{
    //temp
    SwitchToDesktop();
    Sleep(50);
    SendMultiAsciiInput(2, VK_MENU, VK_F4);
}

VOID Logoff()
{
    system("logoff");
    //ExitWindows();
}

VOID ShutdownComputer()
{
    system("shutdown -s -t 0");
}

VOID SwitchUser()
{
    WTSDisconnectSession(WTS_CURRENT_SERVER_HANDLE, WTS_CURRENT_SESSION, FALSE);
}

VOID LockComputer()
{
    //SendMultiAsciiInput(2, VK_LWIN, 'L');
    LockWorkStation();
}

VOID RestartComputer()
{
    system("shutdown -r -t 0");
    //ExitWindowsEx(EWX_REBOOT, SHTDN_REASON_MINOR_PROCESSOR);
}

VOID SleepComputer()
{
    SetSuspendState(0, 0, 0);
}

VOID ShowRunDialog()
{
    SendMultiAsciiInput(2, VK_LWIN, 'R');
}
