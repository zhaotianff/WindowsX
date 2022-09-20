# Master-Zhao
Master-Zhao is a toolset software for Windows. Including beautification, system management, gadgets, paddling and other functions.  

Why write such a software?  
Willing to go through hardships and never forget the original intention.  
 remember the joy of outputting "Hello World" on the screen for the first time.  
Right here waiting to recall, just at that time already disconsolate.

<p align="center">
<a href="https://github.com/zhaotianff/Master-Zhao" target="_blank">
<img align="center" alt="Master-Zhao" src="logo.png" />
</a>
</p>
<p align="center">
<a href="https://github.com/zhaotianff/Master-Zhao/stargazers" target="_blank">
 <img alt="GitHub stars" src="https://img.shields.io/github/stars/zhaotianff/Master-Zhao.svg" />
</a>
<a href="https://github.com/zhaotianff/Master-Zhao/releases" target="_blank">
 <img alt="All releases" src="https://img.shields.io/github/downloads/zhaotianff/Master-Zhao/total.svg" />
</a>
<a href="https://github.com/zhaotianff/Master-Zhao/network/members" target="_blank">
 <img alt="Github forks" src="https://img.shields.io/github/forks/zhaotianff/Master-Zhao.svg" />
</a>
<a href="https://github.com/zhaotianff/Master-Zhao/issues" target="_blank">
 <img alt="All issues" src="https://img.shields.io/github/issues/zhaotianff/Master-Zhao.svg" />
</a>
</p>
<h1 align="center">Master-Zhao :hammer_and_wrench: </h1>

# Development environment
* ~~.NET5~~ .NET Core 3.1
* Visual C++ Toolset 142
* Visual Studio 2019 16.10.4

# Build
Set the project to 64 bit
<p align="center">
    <img align="center" alt="error dll format" src="Screenshots/error_dll_format.png" />
</p>
<p align="center">
    <img align="center" alt="build x64" src="Screenshots/build_x64.png" />
</p>

# Run directly
If it just runs without compiling. After installing .NET Core 3.1 Runtime and Visual C++ 2019 x64 Redistributable Package, execute Master.Zhao.Shell.exe.
* [.NET Core 3.1 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-3.1.27-windows-x64-installer)
* [Visual C++ 2019 Redistributable Package (x64)](https://aka.ms/vs/16/release/VC_redist.x64.exe)

# List of completed features  
## Beautify
  * Static wallpaper setting
  * Dynamic wallpaper setting
  * Mouse effect
  * Custom start menu
  * Taskbar setting
## Utilities
  * boss key
  * fast run
      *  menu start
## huá shuǐ

# Todo list
## Beautify
  * desktop effect
  * context menu setting
  * boot image setting
  * other
## System Tools
  * boot management
  * service management
  * background network management
  * windows settings
  * gpedit & register
  * fast search(taskbar win+q)
## Utilities
  * hardware information
  * startup assistant
  * fast run
      *  one key start
## huá shuǐ

# Features
![main](Screenshots/main.png)
**Main**      

![static_wallpaper](Screenshots/static_wallpaper.png)  
**Static wallpaper settings**  

![dynamic_wallpaper](Screenshots/dynamic_wallpaper.png)  
**Live Wallpaper Settings**  
Currently, only local videos are supported as the desktop background, and other types of dynamic wallpapers will be added later.  

![mouse_effect](Screenshots/mouse_effect.png)  
**Mouse Effects**  
For now, only a simple version is made, and some points to follow are drawn. You can add cool effects later  

![center_taskbar](Screenshots/center_taskbar.png)  
**Taskbar settings**  
At present, settings such as taskbar transparency and Windows 11-style taskbar have been supported, and other settings are still being gradually improved.  
**Note: If the taskbar or desktop display abnormally, you can restart the resource manager process. Find explorer.exe in task manager and restart it**  

![win98_startmenu](Screenshots/win98_startmenu.png)  
**Customize the start menu**  
At present, a simplified version of the Windows 98 start menu has been made, which shields the Win key and the start menu button. If you open the start menu in other ways (such as Ctrl + ESC), the system start menu will still be displayed.  
Some functions of the Windows 98 Start menu are still being perfected.  

![fast_run](Screenshots/fast_run.png)
**Quick Start Menu**  
This is a menu that supports quick startup. It was originally intended to be a menu effect similar to the weapon switching in "Shadow of the Tomb Raider".  
But time is limited and there are still some technical difficulties, so I made a simple quick start menu.  
How to use: After configuring the quick start item on the interface, press and hold the Alt key to display the menu at the mouse. At this time, press the 1/2/3/4 number key to run the corresponding menu item, or you can use the mouse to run the corresponding menu item. Click Run.  

![boss_key](Screenshots/boss_key.png)  
**Boss key**  
This is a convenient function for paddling. After starting the boss key function, you can choose 5 response methods  
They are: switch to task, end task, open specified program, switch to desktop, help me write code.  
The currently set keys are Alt + Q (convenient to use with one hand with the left hand), Ctrl + I (convenient to use with both hands).  
* Switch to task: When the boss key is pressed, it will switch to the specified process.  
* End task: When the boss key is pressed, the specified process will be ended  
* Open the specified program: can support URLs, local programs, control panel items, etc., the complete support items can be viewed https://github.com/zhaotianff/Windows-run-tool  
* Switch to desktop: this function is the same as Win + D  
* Help me write code: This function is very magical, copy a piece of code to the code box, then open the development environment, press the boss key, the software will automatically rewrite the code you just copied for you, pretending to be writing code.  
**Tips: At present, the software supports minimization to the taskbar, the boss key function can be used all day, and you can paddle happily**  

# Development Plan
- [ ] boss key

# License
[GPLV3](LICENSE)
