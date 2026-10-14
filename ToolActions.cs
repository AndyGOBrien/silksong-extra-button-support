using BepInEx.Configuration;
using GlobalEnums;
using HarmonyLib;
using InControl;

namespace ExtraButtonSupport;

internal static class Tools
{
    internal static readonly string[] Names = { "Tool Up", "Tool Neutral", "Tool Down", "Scuttle" };
    internal const int ScuttleIndex = 3;
    internal static readonly AttackToolBinding[] Slots = { AttackToolBinding.Up, AttackToolBinding.Neutral, AttackToolBinding.Down };
    internal const int ButtonBase = 100;

    private static HeroActions? _owner;
    private static readonly PlayerAction[] Actions = new PlayerAction[4];

    internal static PlayerAction[] For(HeroActions heroActions)
    {
        if (!ReferenceEquals(_owner, heroActions))
        {
            for (int i = 0; i < Names.Length; i++)
                Actions[i] = heroActions.GetPlayerActionByName("ExtraButtonSupport " + Names[i]) ?? new PlayerAction("ExtraButtonSupport " + Names[i], heroActions);
            _owner = heroActions;
        }
        return Actions;
    }

    internal static HeroActions? Owner => _owner;

    internal static bool IsTool(PlayerAction? action) => _owner != null && action != null && System.Array.IndexOf(Actions, action) >= 0;

    internal static AttackToolBinding? Held(InputDevice? device)
    {
        if (_owner == null || device == null) return null;
        for (int i = 0; i < Slots.Length; i++)
            if (IsHeld(Actions[i], device)) return Slots[i];
        return null;
    }

    internal static bool ScuttleHeld(InputDevice? device) =>
        _owner != null && device != null && IsHeld(Actions[ScuttleIndex], device) && GlobalSettings.Gameplay.ScuttleCharmTool.IsEquipped;

    private static bool IsHeld(PlayerAction action, InputDevice device)
    {
        var bindings = action.Bindings;
        for (int b = 0; b < bindings.Count; b++)
            if (bindings[b].GetState(device)) return true;
        return false;
    }

    internal static ConfigEntry<InputControlType> Saved(GamepadType pad, int i) =>
        ExtraButtonSupportPlugin.Instance!.Config.Bind("Bindings." + pad, Names[i].Replace(" ", ""), InputControlType.None,
            "Controller button for " + Names[i] + " (set from the in-game Controller remap screen)");
}

[HarmonyPatch]
internal static class ToolActionPatches
{
    private static readonly AccessTools.FieldRef<HeroController, ToolItem> WillThrowTool =
        AccessTools.FieldRefAccess<HeroController, ToolItem>("willThrowTool");
    private delegate bool CanThrowToolFn(HeroController hero, ToolItem tool, AttackToolBinding binding, bool reportFailure);
    private static readonly CanThrowToolFn CanThrowTool = AccessTools.MethodDelegate<CanThrowToolFn>(
        AccessTools.Method(typeof(HeroController), "CanThrowTool", new[] { typeof(ToolItem), typeof(AttackToolBinding), typeof(bool) }));

    [HarmonyPostfix, HarmonyPatch(typeof(InputHandler), nameof(InputHandler.MapControllerButtons))]
    private static void ApplyToolBindings(InputHandler __instance, GamepadType gamePadType)
    {
        PlayerAction[] tools = Tools.For(__instance.inputActions);
        for (int i = 0; i < tools.Length; i++)
        {
            tools[i].ClearBindings();
            InputControlType saved = Tools.Saved(gamePadType, i).Value;
            if (saved != InputControlType.None) tools[i].AddBinding(new DeviceBindingSource(saved));
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(InputHandler), "SetupMappableControllerBindingsList")]
    private static void AddToMappable(InputHandler __instance) =>
        __instance.MappableControllerActions.AddRange(Tools.For(__instance.inputActions));

    [HarmonyPostfix, HarmonyPatch(typeof(InputHandler), nameof(InputHandler.SendButtonBindingsToGameSettings))]
    private static void SaveToolBindings(InputHandler __instance)
    {
        PlayerAction[] tools = Tools.For(__instance.inputActions);
        for (int i = 0; i < tools.Length; i++) Tools.Saved(__instance.activeGamepadType, i).Value = __instance.GetButtonBindingForAction(tools[i]);
    }

    [HarmonyPrefix, HarmonyPatch(typeof(InputHandler), nameof(InputHandler.ResetDefaultControllerButtonBindings))]
    private static void ResetToolBindings(InputHandler __instance)
    {
        for (int i = 0; i < Tools.Names.Length; i++) Tools.Saved(__instance.activeGamepadType, i).Value = InputControlType.None;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(InputHandler), nameof(InputHandler.ActionButtonToPlayerAction))]
    private static bool RowToToolAction(InputHandler __instance, HeroActionButton actionButtonType, ref PlayerAction __result)
    {
        int i = (int)actionButtonType - Tools.ButtonBase;
        if (i < 0 || i >= Tools.Names.Length) return true;
        __result = Tools.For(__instance.inputActions)[i];
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(PlayerAction), "UpdateBindings")]
    private static void DriveVanillaActions(PlayerAction __instance, ulong updateTick, float deltaTime)
    {
        HeroActions? owner = Tools.Owner;
        if (owner == null || owner.IsListeningForBinding) return;
        if (ReferenceEquals(__instance, owner.QuickCast) ? Tools.Held(InputManager.ActiveDevice) != null
            : ReferenceEquals(__instance, owner.Dash) && Tools.ScuttleHeld(InputManager.ActiveDevice))
            __instance.UpdateWithValue(1f, updateTick, deltaTime);
    }

    private static readonly AccessTools.FieldRef<OneAxisInputControl, InputControlState> ControlState =
        AccessTools.FieldRefAccess<OneAxisInputControl, InputControlState>("thisState");

    [HarmonyPrefix, HarmonyPatch(typeof(HeroController), "HeroDashPressed")]
    private static void ScuttleDirection(HeroController __instance, out InputControlState[]? __state)
    {
        __state = null;
        HeroActions? owner = Tools.Owner;
        if (owner == null || !Tools.ScuttleHeld(InputManager.ActiveDevice)) return;
        __state = new[] { ControlState(owner.Down), ControlState(owner.Left), ControlState(owner.Right) };
        ControlState(owner.Down) = new InputControlState { State = true, Value = 1f, RawValue = 1f };
        ControlState(owner.Left) = default;
        ControlState(owner.Right) = default;
    }

#pragma warning disable HARMONIZE004
    [HarmonyFinalizer, HarmonyPatch(typeof(HeroController), "HeroDashPressed")]
    private static void RestoreDirection(InputControlState[]? __state)
    {
        HeroActions? owner = Tools.Owner;
        if (__state == null || owner == null) return;
        ControlState(owner.Down) = __state[0];
        ControlState(owner.Left) = __state[1];
        ControlState(owner.Right) = __state[2];
    }
#pragma warning restore HARMONIZE004

    private static bool _wasHeld;
    private static readonly AccessTools.FieldRef<HeroController, bool> IsGameplayScene =
        AccessTools.FieldRefAccess<HeroController, bool>("isGameplayScene");

    [HarmonyPrefix, HarmonyPatch(typeof(HeroController), "LookForQueueInput")]
    private static void ToolBindCancelsSprint(HeroController __instance)
    {
        bool held = Tools.Held(InputManager.ActiveDevice) != null, pressed = held && !_wasHeld;
        _wasHeld = held;
        if (!pressed || __instance.IsInputBlocked() || __instance.IsPaused() || !IsGameplayScene(__instance)
            || InteractManager.BlockingInteractable != null || Tools.Owner?.IsListeningForBinding == true) return;
        PlayMakerFSM fsm = __instance.sprintFSM;
        if (!SprintFsm.Accepts(fsm, "TOOLTHROW") || !__instance.GetWillThrowTool(reportFailure: true)) return;
        string from = fsm.ActiveStateName;
        fsm.SendEvent("TOOLTHROW");
        ExtraButtonSupportPlugin.Log?.LogInfo($"[extra] tool bind interrupted sprint: {from} -> {fsm.ActiveStateName}");
    }

    [HarmonyPrefix, HarmonyPatch(typeof(HeroController), nameof(HeroController.GetWillThrowTool))]
    private static bool UseHeldSlot(HeroController __instance, bool reportFailure, ref bool __result)
    {
        if (Tools.Held(InputManager.ActiveDevice) is not AttackToolBinding slot) return true;
        ToolItem tool = ToolItemManager.GetBoundAttackTool(slot, ToolEquippedReadSource.Active, out AttackToolBinding used);
        WillThrowTool(__instance) = tool;
        __result = tool && CanThrowTool(__instance, tool, used, reportFailure);
        return false;
    }
}

internal static class SprintFsm
{
    internal static bool Accepts(PlayMakerFSM fsm, string eventName)
    {
        foreach (HutongGames.PlayMaker.FsmTransition t in fsm.Fsm.ActiveState?.Transitions ?? System.Array.Empty<HutongGames.PlayMaker.FsmTransition>())
            if (t.EventName == eventName) return true;
        return false;
    }
}
