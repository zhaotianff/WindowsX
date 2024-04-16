<font size=4> README: 中文 | <a href="./README-en.md">English</a>  </font>

# WindowsX
Windows工具箱

<p align="center">
<a href="https://github.com/zhaotianff/WindowsX" target="_blank">
<img align="center" alt="WindowsX" src="logo.png" />
</a>
</p>
<p align="center">
<a href="https://github.com/zhaotianff/WindowsX/stargazers" target="_blank">
 <img alt="GitHub stars" src="https://img.shields.io/github/stars/zhaotianff/WindowsX.svg" />
</a>
<a href="https://github.com/zhaotianff/WindowsX/releases" target="_blank">
 <img alt="All releases" src="https://img.shields.io/github/downloads/zhaotianff/WindowsX/total.svg" />
</a>
<a href="https://github.com/zhaotianff/WindowsX/network/members" target="_blank">
 <img alt="Github forks" src="https://img.shields.io/github/forks/zhaotianff/WindowsX.svg" />
</a>
<a href="https://github.com/zhaotianff/WindowsX/issues" target="_blank">
 <img alt="All issues" src="https://img.shields.io/github/issues/zhaotianff/WindowsX.svg" />
</a>
</p>
<h1 align="center">WindowsX :hammer_and_wrench: </h1>

# 开发环境
* .NET 6 ~~.NET5~~ ~~.NET Core 3.1~~
* Visual C++ Toolset 142
* Visual Studio 2022 17.9.6~~Visual Studio 2019 16.10.4~~

# 构建
需要设置项目为x64
<p align="center">
    <img align="center" alt="error dll format" src="Screenshots/error_dll_format.png" />
</p>
<p align="center">
    <img align="center" alt="build x64" src="Screenshots/build_x64.png" />
</p>

# 直接运行
如果不想编译代码直接运行 。安装.NET Core 6.0.25 Desktop Runtime 和Visual C++ 2019 x64 Redistributable Package后执行WindowsX.Shell.exe即可。
* [.NET Core 6.0.15 Desktop Runtime](https://dotnet.microsoft.com/zh-cn/download/dotnet/thank-you/runtime-desktop-6.0.15-windows-x64-installer)
* [Visual C++ 2019 Redistributable Package (x64)](https://aka.ms/vs/16/release/VC_redist.x64.exe)

# 功能列表
## 美化
  * 静态壁纸设置
  * 动态壁纸设置
  * 鼠标特效(基础版本)
  * 自定义开始菜单(基础版本)
  * 任务栏设置(部分功能)
## 实用工具
  * 老板键
  * 快速启动
      *  菜单启动

# 正在开发的功能
  * 实用工具 ->老板键

# 待完成的功能
## 美化
  * 桌面特效
  * 右键菜单管理
  * 开机图像修改
  * 其它
## 系统工具
  * 启动项管理
  * 服务管理
  * 后台网络管理
  * 系统设置
  * 组策略和注册表管理
  * 快速搜索（基于任务栏）
## 实用工具
  * 一键优化系统设置
  * 硬件信息
  * 开机助手
  * 快速启动
      *  一键启动
  * 右键扩展
## 划水

# 功能介绍

https://github.com/zhaotianff/WindowsX/assets/22126367/ed30324a-16e1-42d3-8519-863e574eef43

**静态壁纸设置**   

---

https://github.com/zhaotianff/WindowsX/assets/22126367/0f319b6f-b69c-4bb3-876a-0ca7ff60d205

**动态壁纸设置**  
目前仅支持将本地视频设置为桌面背景，其它类型的动态壁纸后续再加入。  

---

https://github.com/zhaotianff/WindowsX/assets/22126367/4354bbfc-90c0-4566-a89d-5469ec6c1e82

**自定义开始菜单**  
目前就做了一个简易版的Windows 98开始菜单，屏蔽了Win键和开始菜单按钮，其它方式打开开始菜单(如Ctrl + ESC)，还是会显示系统开始菜单。    
Windows 98开始菜单的一些功能还在完善中。    

---

https://github.com/zhaotianff/WindowsX/assets/22126367/de833f95-83dc-4cb3-9ade-0d57ea3c6b56

**快速启动菜单**  
这是一个支持快速启动的菜单，类似《古墓丽影 暗影》里武器切换的那种菜单效果。      
使用方法：在界面配置快速启动项后，按住Alt键，会在鼠标处显示菜单，此时，再按1/2/3/4/Q/W/E/R键，即可运行相应的菜单项，也可以通过鼠标点击运行。    
在《古墓丽影 暗影》中是按1/2/3/4键切换武器，我这里设置了一个功能键，不然太容易按出来了，后面再增加按键配置吧，目前还没有。  

---
![boss_key](Screenshots/boss_key.png)  
**老板键**  
这是一个方便划水的功能，启动老板键功能后，可以选择5种响应方式  
分别是：切换到任务、结束任务、打开指定的程序、切换到桌面、帮我写代码。  
目前设置的按键是Alt + Q(方便左手单手使用),Ctrl + I(方便双手一起使用)。  
* 切换到任务:当老板键按下时，会切换到指定的进程。
* 结束任务:当老板键按下时，会结束指定的进程
* 打开指定的程序:可以支持网址、本地程序、控制面板项等，完整的支持项可以查看 https://github.com/zhaotianff/Windows-run-tool
* 切换到桌面:该功能和Win + D一样
* 帮我写代码：这个功能很魔幻，复制一段代码到代码框，然后打开开发环境，按下老板键，软件会自动帮你把刚才复制的代码重写一遍，假装自己在写代码。  
**小提示：目前软件支持最小化到任务栏，老板键功能可以全天使用，愉快的划水**
  
---
# License
[GPLV3](LICENSE)



