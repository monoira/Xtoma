# AGENTS.md

Xtoma is a Unity 6 2D game on the stock URP 2D template. `SampleScene` holds only a Main Camera and a Global Light 2D;

## Working in Unity

- Unity `6000.6.11f1`, 2D mode, linear color space. Use 6000.6 docs, not older Unity APIs
- Drive the open Editor through the Unity MCP tools: `tools.unity.<group>({ command, args })` inside `execute`. Do not hand-edit `.unity`, `.prefab`, or `.asset` YAML
- Start with `tools.unity.editor({ command: "status" })` and `tools.unity.console({ command: "status" })`; the console status returns a cursor for later error reads
- Do not use `dotnet build` or IDE projects. Create scripts with `tools.unity.script({ command: "create" })`, compile with `recompile`, poll `recompile_status`, then attach with `attach`; read failures with `tools.unity.console({ command: "get", args: { since: <cursor>, level: "error" } })`
- Tests: `tools.unity.test({ command: "run", args: { mode: "EditMode" | "PlayMode", filter: "<name>" } })`. Test Framework 1.8 is installed but no test assemblies exist yet
- Use `tools.unity.gameobject`, `component`, `asset`, `prefab`, `scene`, and `material` to build content, `tools.unity.capture` for screenshots, and `tools.unity.editor_playmode` to play or stop
- MCP server `unity` is configured in `.opencode/opencode.jsonc`. The `unity` CLI is only a fallback for CI or batch jobs

## Project layout

- `Assets/Scenes/SampleScene.unity` is the only scene and the only Build Settings entry. Add new scenes with `tools.unity.scene({ command: "add_to_build", args: { path } })`, or edit `ProjectSettings/EditorBuildSettings.asset`
- `Assets/Settings/` holds `UniversalRP.asset`, `Renderer2D.asset`, the default volume profile, and `InputSystem_Actions.inputactions`
- `Assets/Welcome/` is Unity's 2D template welcome page. Leave it alone
- Scripts compile into `Assembly-CSharp`; no game `.asmdef` files exist yet
- Keep each asset's `.meta` file and commit it with the asset. Generated: `Library/`, `Temp/`, `Logs/`, `*.csproj`, `*.slnx`

## 2D specifics

- Sprites use lit materials by default. The scene's Global Light 2D lights them; if new sprites render black, check the material and light before touching import settings
- Input System package only (`activeInputHandler: 1`). Use `UnityEngine.InputSystem`; legacy `Input.*` throws at runtime
- Camera is orthographic, size 5, 16:9 at 1920x1080. Design around that framing unless the task says otherwise
- Available 2D packages: animation, Aseprite and PSD importers, tilemap + extras, sprite shape, sprite editor

## Palette

Catppuccin Mocha, hex format. Use for UI, art, and scene tinting.

- Backgrounds: `#1e1e2e` base, `#181825` mantle, `#11111b` crust
- Surfaces: `#313244` surface0, `#45475a` surface1, `#585b70` surface2
- Overlays: `#6c7086` overlay0, `#7f849c` overlay1, `#9399b2` overlay2
- Text: `#cdd6f4` text, `#bac2de` subtext1, `#a6adc8` subtext0
- Accents: `#f5e0dc` rosewater, `#f2cdcd` flamingo, `#f5c2e7` pink, `#cba6f7` mauve, `#f38ba8` red, `#eba0ac` maroon, `#fab387` peach, `#f9e2af` yellow, `#a6e3a1` green, `#94e2d5` teal, `#89dceb` sky, `#74c7ec` sapphire, `#89b4fa` blue, `#b4befe` lavender

Prefer dark backgrounds (base/mantle/crust) with text or subtext for copy, and accents for state (red for damage, green for health, yellow for warnings).
