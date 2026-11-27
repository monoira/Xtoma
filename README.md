# README

Roguelike Platformer made with Unity 2D, Unity MCP, OpenCode and Deepseek v4.1 Flash.  
Playable on: Windows, Linux, Android and web.  
Uses catppuccin color scheme.

Play online for free:  
https://monoira.github.io/Xtoma/

## building and deploying

### build

builds each platform into Builds/

```bash
unity build --target StandaloneLinux64  -o Builds/linux/Xtoma.x86_64
unity build --target StandaloneWindows64 -o Builds/windows/Xtoma.exe
unity build --profile "Web - Mobile - Release" -o Builds/web
unity build --profile "Android™" -o Builds/android/Xtoma.apk
```

### release

zips the desktop builds and creates a github release

```bash
cd Builds
zip -qr Xtoma-linux.zip linux -x "linux/Xtoma_BackUpThisFolder*"
zip -qr Xtoma-windows.zip windows -x "windows/Xtoma_BackUpThisFolder*"
gh release create v1.2.0 --notes "Play online: https://monoira.github.io/Xtoma/" Xtoma-linux.zip Xtoma-windows.zip android/Xtoma.apk
```

### web

deploys the web build to github pages

```bash
TMP=$(mktemp -d)
git -C "$TMP" init -q -b gh-pages
cp -a Builds/web/. "$TMP/"
touch "$TMP/.nojekyll"
git -C "$TMP" add -A
git -C "$TMP" commit -qm "deploy web"
git -C "$TMP" remote add origin git@github.com:monoira/Xtoma.git
git -C "$TMP" push -f origin gh-pages
rm -rf "$TMP"
```

Live at https://monoira.github.io/Xtoma
