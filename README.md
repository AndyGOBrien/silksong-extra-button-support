# Extra Button Support

Use your controller's **extra buttons** (back paddles, extra bumpers, macro buttons such as C/Z) in
Hollow Knight: Silksong as real, separate buttons:

- Bind any action to an extra button in the game's own **Remap Controller** screen.
- Use the new **Tool Up / Tool Neutral / Tool Down** binds to fire the tool or silk skill in that slot
  directly, without holding a direction. A dedicated tool bind also interrupts a sprint.
- Use the new **Scuttle** bind to scuttle (needs Scuttlebrace equipped) without holding Down + Dash.
- Extra buttons show your controller's real button glyphs.
- No keyboard keys are involved, so the prompts stay on controller glyphs.

Works through Steam Input, so any controller whose extra buttons Steam can bind is supported.

## Requirements
- Silksong on Steam, with **Steam Input enabled** for your controller (the default).
- [BepInExPack_Silksong](https://thunderstore.io/c/hollow-knight-silksong/p/silksong_modding/BepInExPack_Silksong/).

## Setup
1. Install the mod with a mod manager (r2modman, Gale, Cogfly), or extract the zip into
   `BepInEx/plugins/`. On Linux / Steam Deck (Proton), BepInEx itself needs the usual
   `WINEDLLOVERRIDES="winhttp=n,b" %command%` launch option (or a `winhttp` override in the game's
   prefix via protontricks/winecfg).
2. In Steam, open Silksong's controller settings (Steam overlay → Controller settings) →
   **Browse layouts** → **Community**, and apply an **Extra Button Support** layout:
   - **Extra Button Support – <your controller>** already has that controller's extra buttons bound.
   - **Extra Button Support – Build your own** is Steam's normal Gamepad layout plus the Extra Button
     actions. In **Edit Layout**, bind each extra button to an Extra Button (Game Actions).
3. In Silksong: **Options → Controller → Remap Controller**. Select an action (or Tool Up / Neutral /
   Down, or Scuttle) and press an extra button.

The mod installs nothing into Steam: the Extra Button actions come from the layout you apply. On any
other layout the game behaves exactly as without the mod.

### Extra Button numbering
Extra Buttons mean positions, so one set of in-game binds works on every controller:

| Extra Button | Position |
|---|---|
| 1 / 2 | Upper left / upper right paddle (L4 / R4) |
| 3 / 4 | Lower left / lower right paddle (L5 / R5) |
| 5 / 6 | Extra left / right bumper |
| 7–12 | Any other extra buttons (e.g. Vader 5 Pro: C = 7, Z = 8, Circle = 9) |

> **Start from an Extra Button Support layout.** A plain layout has no Extra Button actions, and
> adding game actions to one in Steam's editor removes all its gamepad bindings.

## Uninstalling
1. **Before removing the mod**, open **Remap Controller** and choose **Reset Defaults** (or move every
   action off the extra buttons and the Tool / Scuttle rows). Without the mod, actions bound to extra
   buttons do nothing, and an action left unbound by a Tool row stays unbound.
2. Remove the mod, and pick another layout for Silksong in Steam if you like.

## Notes and limits
- Single player: extra-button presses go to the active controller.
- Tested on the Windows build under Proton. Native Windows and native Linux are supported by design,
  but untested.
- The mod doesn't modify Steam, game files or saves. Your Tool / Scuttle binds are saved in
  `BepInEx/config/io.github.andygobrien.extrabuttonsupport.cfg`.
- Silksong's official layout (Team Cherry's) is untouched; you pick an Extra Button Support layout once.

## Troubleshooting
Check `BepInEx/LogOutput.log` for lines starting with `[extra]`. `usesSteamInputAPI=True` in a
`config loaded` line means an Extra Button Support layout is active.
