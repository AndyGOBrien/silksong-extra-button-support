<!-- human-only-edit-section-start -->
## Human Only Section

I have been a professional software engineer since 2016 and have worked professionally in technology since 2012. However my experience is not traditionally with modding games. Keep that in mind when you use this. This project is (clearly) built with AI assistance. I have personally reviewed all code. Concept, architecture (what little of it there is in this project), design, are all mine.

Any artwork used is AI generated as I am not an artist. If anyone with any artistic talent wants to contribute what little artwork there is in this project please feel free to put up a PR. However, there really is only the project icon for Thunderstore at this point. There is potential for in game controller and button glyphs, scuttle available indicator with button glyph, etc... For art only PRs I will only accept non AI generated art as I can easily prompt LLMs myself for Gen AI imagery and inconography.

<!-- human-only-edit-section-end -->

# Extra Button Support

Use your controller's **extra buttons** (back paddles, extra bumpers, macro buttons such as C/Z) in
Hollow Knight: Silksong as real, separate buttons:

- Bind any action to an extra button in the game's own **Remap Controller** screen.
- Use the new **Tool Up / Tool Neutral / Tool Down** binds to fire the tool or silk skill in that slot
  directly, without holding a direction. A dedicated tool bind also interrupts a sprint.
- Use the new **Scuttle** bind to scuttle (needs Scuttlebrace equipped) without holding Down + Dash.
- Extra buttons show your controller's real button glyphs.
- No keyboard keys are involved, so the prompts stay on controller glyphs.

> **Gameplay change:** the mod adds one mechanic that isn't in the vanilla game. Pressing a **Tool Up /
> Tool Neutral / Tool Down** bind while sprinting interrupts the sprint and uses the tool. It doesn't
> interrupt a sprint direction change or a dash animation: a tool pressed during those is queued
> exactly the way vanilla queues it. The vanilla Skill / Tool button behaves as it always has with no interrupt. If you want your silk skill to interrupt sprint you must remap your silk skill to Tool Neutral.

<img width="50%" height="50%" alt="image" src="https://github.com/user-attachments/assets/def2993b-c7c4-4661-b87f-e7e9c8545d5e" />

## Supported controllers
This mod is for controllers that Steam supports through SDL and/or DirectInput and whose extra buttons can
be mapped in Steam Input. The extra buttons reach the game as Steam Input actions, so **the game must be
launched through Steam** for the mod to work.

Tested with:

| Controller | Community layout |
|---|---|
| 8BitDo Ultimate 2 | Extra Button Support - 8BitDo Ultimate 2 |
| Steam Controller 2.0 | Extra Button Support - Steam Controller 2.0 |
| Flydigi Vader 5 Pro | Extra Button Support – Vader 5 Pro |

### Should also work (untested)
Steam Input can map the extra buttons on these controllers, so the mod should work with them, but they haven't been tested. Steam only lists a community layout for the controller type it was made for, so a layout from a similar controller won't always show up.

| Controller | Extra buttons | Layout |
|---|---|---|
| Flydigi Vader 4 Pro | Back buttons, C, Z | Extra Button Support – Vader 5 Pro. It's published for Steam's generic controller type, so it should show up if Steam treats the Vader 4 Pro as a generic controller. |
| Steam Deck | L4, R4, L5, R5 | Same buttons as the Steam Controller 2.0, but Steam counts the Deck as a different controller type, so that layout doesn't show up. No Steam Deck layout yet: see **Build your own layout**. |
| Xbox Elite Series 2 | Four back paddles | No layout yet: see **Build your own layout**. |
| DualSense Edge | Two back buttons | No layout yet: see **Build your own layout**. |

## Requirements
- Silksong on Steam, launched through Steam, with **Steam Input enabled** for your controller (the default).
- [BepInExPack_Silksong](https://thunderstore.io/c/hollow-knight-silksong/p/silksong_modding/BepInExPack_Silksong/).

## Setup
1. Install the mod with a mod manager (r2modman, Gale, Cogfly), or extract the zip into
   `BepInEx/plugins/`. On Linux / Steam Deck (Proton), BepInEx itself needs the usual
   `WINEDLLOVERRIDES="winhttp=n,b" %command%` launch option (or a `winhttp` override in the game's
   prefix via protontricks/winecfg).
2. In Steam, open Silksong's controller settings (Steam overlay → Controller settings) →
   **Browse layouts** → **Community**, and apply the **Extra Button Support** layout for your
   controller (see **Supported controllers**). It already has the extra buttons bound. If there's none
   for your controller, see **Build your own layout** below.
3. In Silksong: **Options → Controller → Remap Controller**. Select an action (or Tool Up / Neutral /
   Down, or Scuttle) and press an extra button.

## How it works
Steam Input reads the extra buttons. An Extra Button Support layout has twelve Steam Input **game
actions**, **Extra Button 1** to **Extra Button 12**, alongside the normal gamepad bindings. When you press
a physical button bound to one of them, Steam tells the mod, and the mod gives the game a new, separate
controller button. The game treats it like any other button: you can bind it in **Remap Controller**, and
its prompt shows Steam's picture of the physical button. Your normal gamepad buttons keep working as usual.

The mod installs nothing into Steam: the Extra Button actions come from the layout you apply. On any
other layout the game behaves exactly as without the mod.

### Binding an extra button in Steam
1. Apply an **Extra Button Support** layout, or your own layout from **Build your own layout**.
2. Choose **Edit Layout** and select the physical button you want to use (a back paddle, for example).
3. Open **Game Actions** and pick the **Extra Button** whose number matches the button's position (see
   the table below).
4. Save the layout. In Silksong, go to **Remap Controller**, select an action and press the button.

> **Steam's editor can't add the Extra Button actions.** Only Extra Button Support layouts, and layouts
> you've added them to (see **Build your own layout**), carry them. Converting any other layout to game
> actions in Steam's editor removes all of its gamepad bindings.

### Extra Button numbering
Each Extra Button number stands for a position on the controller, not a manufacturer's button name. Use
the same numbers on every controller and your in-game binds carry over when you switch.

| Steam Input action | Position | Button the mod gives the game |
|---|---|---|
| Extra Button 1 | Upper left paddle | Action5 |
| Extra Button 2 | Upper right paddle | Action6 |
| Extra Button 3 | Lower left paddle | Action7 |
| Extra Button 4 | Lower right paddle | Action8 |
| Extra Button 5 | Extra left bumper | Action9 |
| Extra Button 6 | Extra right bumper | Action10 |
| Extra Button 7–12 | Any other extra buttons | Action11, Action12, Button0–Button3 |

Examples from the tested layouts:

| Controller | Extra Button bindings |
|---|---|
| Steam Controller 2.0 | L4 = 1, R4 = 2, L5 = 3, R5 = 4 |
| Flydigi Vader 5 Pro | back paddles = 1–4, extra bumpers = 5 / 6, C = 7, Z = 8, Circle = 9 |

### Build your own layout
If there's no Extra Button Support layout for your controller, add the Extra Button actions to a layout
yourself. Steam's editor can't add them, so this is a one-time edit of the layout file. After that you
bind buttons in Steam as usual.

1. In Steam, open Silksong's controller settings, set up the layout you want to start from (Steam's
   **Gamepad** template keeps your normal buttons) and save it as a new personal layout.
2. Exit Steam completely (**Steam → Exit**). Steam rewrites layout files while it's running.
3. Open the layout file in a text editor. Silksong's personal layouts are in
   `Steam/steamapps/common/Steam Controller Configs/<account ID>/config/1030300/`, and the file name
   starts with the name you gave the layout.
4. Make these changes:
   - If the file has a top-level `"localization"` block (indented one tab), delete the whole block,
     from `"localization"` to its closing `}`.
   - Paste the block below on a new line just above the first line that reads `"group"`.
   - If the `"title"` or `"description"` line near the top says `#Title` or `#Description`, replace it
     with your own text.
5. Save the file, start Steam and apply your layout. Then bind your extra buttons as in **Binding an
   extra button in Steam**.

```
	"actions"
	{
		"Default"
		{
			"title"		"#Set_Default"
			"legacy_set"		"0"
			"Button"
			{
				"extra_1"		"#Action_Extra1"
				"extra_2"		"#Action_Extra2"
				"extra_3"		"#Action_Extra3"
				"extra_4"		"#Action_Extra4"
				"extra_5"		"#Action_Extra5"
				"extra_6"		"#Action_Extra6"
				"extra_7"		"#Action_Extra7"
				"extra_8"		"#Action_Extra8"
				"extra_9"		"#Action_Extra9"
				"extra_10"		"#Action_Extra10"
				"extra_11"		"#Action_Extra11"
				"extra_12"		"#Action_Extra12"
			}
		}
	}
	"action_layers"
	{
	}
	"localization"
	{
		"english"
		{
			"Set_Default"		"Default"
			"Action_Extra1"		"Extra Button 1"
			"Action_Extra2"		"Extra Button 2"
			"Action_Extra3"		"Extra Button 3"
			"Action_Extra4"		"Extra Button 4"
			"Action_Extra5"		"Extra Button 5"
			"Action_Extra6"		"Extra Button 6"
			"Action_Extra7"		"Extra Button 7"
			"Action_Extra8"		"Extra Button 8"
			"Action_Extra9"		"Extra Button 9"
			"Action_Extra10"		"Extra Button 10"
			"Action_Extra11"		"Extra Button 11"
			"Action_Extra12"		"Extra Button 12"
		}
	}
```

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
