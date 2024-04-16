#pragma once

#include"framework.h"
#include"InputTool.h"
#include "DesktopTool.h"
#include<PowrProf.h>
#include<wtsapi32.h>

#pragma comment(lib,"wtsapi32.lib")
#pragma comment(lib,"PowrProf.lib")

SILVERAROWANACORE_API VOID ShowShutDownDialog();
SILVERAROWANACORE_API VOID Logoff();
SILVERAROWANACORE_API VOID ShutdownComputer();
SILVERAROWANACORE_API VOID SwitchUser();
SILVERAROWANACORE_API VOID LockComputer();
SILVERAROWANACORE_API VOID RestartComputer();
SILVERAROWANACORE_API VOID SleepComputer();
SILVERAROWANACORE_API VOID ShowRunDialog();