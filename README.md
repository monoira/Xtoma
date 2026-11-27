# README

Roguelike Platformer made with Unity 2D, Unity MCP, OpenCode and Deepseek v4.1 Flash.  
Playable on: Windows, Linux, Android and web.  
Uses catppuccin color scheme.

Play online for free:  
https://monoira.github.io/Xtoma/

## building and deploying

```bash
unity build --target StandaloneLinux64  -o Builds/linux/Xtoma.x86_64
unity build --target StandaloneWindows64 -o Builds/windows/Xtoma.exe
unity build --profile "Web - Mobile - Release" -o Builds/web
unity build --profile "Android™" -o Builds/android/Xtoma.apk
```
