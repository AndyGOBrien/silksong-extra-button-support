using HarmonyLib;
using InControl;

namespace ExtraButtonSupport;

[HarmonyPatch]
internal static class DeviceInjection
{
    private static bool _pollFailed;

    [HarmonyPrefix, HarmonyPatch(typeof(InputManager), nameof(InputManager.AttachDevice))]
    private static void AddExtraControls(InputDevice inputDevice)
    {
        for (int s = 0; s < ExtraButtonSupportPlugin.Slots; s++)
            inputDevice.AddControl(ExtraButtonSupportPlugin.ControlFor(s), $"Extra {s + 1}");
        ExtraButtonSupportPlugin.Log?.LogInfo($"[extra] added Extra 1-{ExtraButtonSupportPlugin.Slots} to {inputDevice.Name} ({inputDevice.GetType().Name}, style {inputDevice.DeviceStyle})");
    }

    [HarmonyPostfix, HarmonyPatch(typeof(InputManager), "UpdateDevices")]
    private static void FeedExtraControls(float deltaTime)
    {
        try { ExtraButtonSupportPlugin.Instance?.Poll(); }
        catch (System.Exception e)
        {
            System.Array.Clear(ExtraButtonSupportPlugin.SlotDown, 0, ExtraButtonSupportPlugin.Slots);
            if (!_pollFailed) ExtraButtonSupportPlugin.Log?.LogError($"[extra] Steam Input poll failed: {e}");
            _pollFailed = true;
        }
        InputDevice active = InputManager.ActiveDevice;
        bool noActive = active == null || !active.HasControl(ExtraButtonSupportPlugin.ControlFor(0));
        ulong tick = InputManager.CurrentTick;
        foreach (InputDevice device in InputManager.Devices)
            for (int s = 0; s < ExtraButtonSupportPlugin.Slots; s++)
                if (device.HasControl(ExtraButtonSupportPlugin.ControlFor(s)))
                    device.GetControl(ExtraButtonSupportPlugin.ControlFor(s)).UpdateWithState((noActive || device == active) && ExtraButtonSupportPlugin.SlotDown[s], tick, deltaTime);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(DeviceBindingSourceListener), "ListenForControl")]
    private static void AcceptExtraSlots(InputDevice device, ref InputControlType __result)
    {
        if (__result != InputControlType.None || device == null) return;
        for (int s = 8; s < ExtraButtonSupportPlugin.Slots; s++)
            if (device.GetControl(ExtraButtonSupportPlugin.ControlFor(s)).Value > 0.5f) { __result = ExtraButtonSupportPlugin.ControlFor(s); return; }
    }
}
