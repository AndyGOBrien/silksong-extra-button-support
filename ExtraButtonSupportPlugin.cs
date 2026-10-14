using System;
using BepInEx;
using HarmonyLib;
using Steamworks;
using UnityEngine;

namespace ExtraButtonSupport;

[BepInAutoPlugin(id: "io.github.andygobrien.extrabuttonsupport")]
public partial class ExtraButtonSupportPlugin : BaseUnityPlugin
{
    internal const int Slots = 12;
    private const string ActionSet = "Default";

    internal static ExtraButtonSupportPlugin? Instance;
    internal static BepInEx.Logging.ManualLogSource? Log;
    internal static readonly bool[] SlotDown = new bool[Slots];
    internal static InControl.InputControlType ControlFor(int slot) =>
        slot < 8 ? InControl.InputControlType.Action5 + slot : InControl.InputControlType.Button0 + (slot - 8);

    internal static int SlotFor(InControl.InputControlType type) =>
        type >= InControl.InputControlType.Action5 && type <= InControl.InputControlType.Action12 ? type - InControl.InputControlType.Action5
        : type >= InControl.InputControlType.Button0 && type <= InControl.InputControlType.Button3 ? 8 + (type - InControl.InputControlType.Button0)
        : -1;

    private bool _initialized;
    private float _nextInitAttempt;
    private InputActionSetHandle_t _set;
    private readonly InputDigitalActionHandle_t[] _actions = new InputDigitalActionHandle_t[Slots];
    private static readonly string[] ActionNames = BuildActionNames();
    private float _nextResolve;
    private readonly InputHandle_t[] _pads = new InputHandle_t[Steamworks.Constants.STEAM_INPUT_MAX_COUNT];
    private readonly EInputActionOrigin[] _origins = new EInputActionOrigin[Steamworks.Constants.STEAM_INPUT_MAX_ORIGINS];
    private int _padCount = -1;
    private readonly ulong[] _lastPads = new ulong[Steamworks.Constants.STEAM_INPUT_MAX_COUNT];
    private bool _padsChanged;
    private Callback<SteamInputConfigurationLoaded_t>? _configLoaded;

    private void Awake()
    {
        Instance = this;
        Log = Logger;
        var harmony = new Harmony(Id);
        harmony.PatchAll(typeof(DeviceInjection));
        harmony.PatchAll(typeof(Glyphs));
        harmony.PatchAll(typeof(ToolActionPatches));
        harmony.PatchAll(typeof(MenuRowPatches));
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }

    private void Update()
    {
        if (!_initialized) { TryInit(); return; }
        if (!_padsChanged) return;
        _padsChanged = false;
        try { LogPads(); }
        catch (Exception e) { Logger.LogError($"[extra] controller info failed: {e.Message}"); }
    }

    private void TryInit()
    {
        if (Time.unscaledTime < _nextInitAttempt) return;
        _nextInitAttempt = Time.unscaledTime + 1f;
        try
        {
            _configLoaded = Callback<SteamInputConfigurationLoaded_t>.Create(e => Logger.LogInfo(
                $"[extra] config loaded: app={e.m_unAppID} usesSteamInputAPI={e.m_bUsesSteamInputAPI} usesGamepadAPI={e.m_bUsesGamepadAPI}"));
            Logger.LogInfo($"[extra] SteamInput.Init(true) = {SteamInput.Init(true)}");
            _initialized = true;
        }
        catch (InvalidOperationException) { }
    }

    internal void Poll()
    {
        if (!_initialized) return;
        SteamInput.RunFrame();
        if (Time.unscaledTime >= _nextResolve) { _nextResolve = Time.unscaledTime + 1f; ResolveHandles(); }
        int count = SteamInput.GetConnectedControllers(_pads);
        if (PadsChanged(count)) { _padCount = count; _padsChanged = true; }
        if (_set.m_InputActionSetHandle != 0)
            for (int p = 0; p < count; p++) SteamInput.ActivateActionSet(_pads[p], _set);
        for (int s = 0; s < Slots; s++)
        {
            bool down = false;
            for (int p = 0; p < count && _actions[s].m_InputDigitalActionHandle != 0; p++)
            {
                InputDigitalActionData_t data = SteamInput.GetDigitalActionData(_pads[p], _actions[s]);
                down |= data.bActive != 0 && data.bState != 0;
            }
            if (down && !SlotDown[s]) LogPress(s, count > 0 ? _pads[0] : default);
            SlotDown[s] = down;
        }
    }

    private bool PadsChanged(int count)
    {
        bool changed = count != _padCount;
        for (int p = 0; p < count; p++)
        {
            changed |= _lastPads[p] != _pads[p].m_InputHandle;
            _lastPads[p] = _pads[p].m_InputHandle;
        }
        return changed;
    }

    internal EInputActionOrigin? OriginFor(int slot)
    {
        if (!_initialized || _padCount < 1 || _actions[slot].m_InputDigitalActionHandle == 0) return null;
        int n = SteamInput.GetDigitalActionOrigins(_pads[0], _set, _actions[slot], _origins);
        return n > 0 ? _origins[0] : null;
    }

    private void ResolveHandles()
    {
        if (_set.m_InputActionSetHandle == 0) _set = SteamInput.GetActionSetHandle(ActionSet);
        for (int s = 0; s < Slots; s++)
            if (_actions[s].m_InputDigitalActionHandle == 0) _actions[s] = SteamInput.GetDigitalActionHandle(ActionNames[s]);
    }

    private static string[] BuildActionNames()
    {
        var names = new string[Slots];
        for (int s = 0; s < Slots; s++) names[s] = $"extra_{s + 1}";
        return names;
    }

    private void LogPads()
    {
        Logger.LogInfo($"[extra] {_padCount} controller(s) connected");
        for (int p = 0; p < _padCount; p++)
            Logger.LogInfo($"[extra] pad{p} handle={_pads[p].m_InputHandle} type={SteamInput.GetInputTypeForHandle(_pads[p])}");
    }

    private void LogPress(int s, InputHandle_t pad)
    {
        int n = SteamInput.GetDigitalActionOrigins(pad, _set, _actions[s], _origins);
        string origins = "";
        for (int i = 0; i < n; i++) origins += $" '{SteamInput.GetStringForActionOrigin(_origins[i])}'";
        Logger.LogDebug($"[extra] extra_{s + 1} DOWN ->{origins} (InControl {ControlFor(s)})");
    }
}
