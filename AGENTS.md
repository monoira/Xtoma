# AGENTS.md

Xtoma is a Unity 6 2D game built from the stock URP 2D template. `SampleScene` contains only Main Camera, Global Light 2D, and the `Game` object running `GameBootstrap`.

## Working in Unity

- Unity `6000.6.11f1`, 2D mode, linear color space. Use 6000.6 docs
- Drive the open Editor through the Unity MCP server configured in `.opencode/opencode.jsonc`; use the `unity` CLI for builds/CI only. Do not hand-edit `.unity`, `.prefab`, or `.asset` YAML
- Start with `tools.unity.editor({ command: "status" })` and `tools.unity.console({ command: "status" })`; the console status returns a cursor for later error reads
- Do not use `dotnet build` or IDE projects. Create scripts with `tools.unity.script({ command: "create" })`, compile with `recompile`, poll `recompile_status`, then attach with `attach`; read failures with `tools.unity.console({ command: "get", args: { since: <cursor>, level: "error" } })`
- Test Framework 1.8 is installed but no test assemblies exist yet
- Build commands are in `README.md`; the Android profile is `Android™` and must be quoted

## Project layout

- `Assets/Scenes/SampleScene.unity` is the only scene and Build Settings entry. Add scenes with `tools.unity.scene({ command: "add_to_build", args: { path } })`
- The game is assembled at runtime: `GameBootstrap` wires it, `GameArt` generates every sprite/texture, `GameAudio` synthesizes clips, and `GameHud` builds the UI. There are no prefabs or imported game art
- Scripts compile into `Assembly-CSharp`; no game `.asmdef` files exist yet
- `Assets/Settings/` holds the URP assets and Build Profiles. `Assets/Welcome/` is template content; leave it alone
- Commit each asset's `.meta` file. `Library/`, `Temp/`, `Logs/`, `Builds/`, `*.csproj`, and `*.slnx` are generated

## 2D specifics

- Runtime sprites use the unlit `GameArt.SpriteMaterial`; the scene's Global Light 2D does not light them
- Input System only (`activeInputHandler: 1`). Use `UnityEngine.InputSystem`; legacy `Input.*` throws
- Camera is orthographic size 5, 16:9 at 1920x1080, configured in `GameBootstrap`. Design around that framing
- `PlatformConfig` targets 60fps on Android; desktop keeps `-1` and relies on vSync. HDR is off intentionally (unlit 2D, no post-processing)
- Cache generated sprites as static `GameArt` properties; `RoundedRect`/`Circle` create a new texture per call and hitch or leak if used per spawn

## Palette

- Use `Assets/Game/Scripts/Palette.cs` for every color (Catppuccin Mocha); `Palette.A(color, alpha)` adds alpha. Do not hardcode hex
- Prefer base/mantle/crust backgrounds with text/subtext; accents signal state (red damage, green health, yellow warnings)
