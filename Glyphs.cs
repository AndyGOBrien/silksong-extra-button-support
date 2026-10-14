using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using InControl;
using Steamworks;
using UnityEngine;

namespace ExtraButtonSupport;

[HarmonyPatch]
internal static class Glyphs
{
    private static readonly Dictionary<EInputActionOrigin, Sprite> Cache = new();
    private static readonly Dictionary<EInputActionOrigin, float> RetryAt = new();

    [HarmonyPostfix, HarmonyPatch(typeof(UIButtonSkins), "GetButtonSkinFor", typeof(InputControlType))]
    private static void UseSteamGlyph(UIButtonSkins __instance, InputControlType inputControlType, ButtonSkin __result)
    {
        int slot = ExtraButtonSupportPlugin.SlotFor(inputControlType);
        if (slot < 0) return;
        EInputActionOrigin? found;
        try { found = ExtraButtonSupportPlugin.Instance?.OriginFor(slot); }
        catch (Exception) { return; }
        if (found is not EInputActionOrigin origin) return;
        if (!Cache.TryGetValue(origin, out Sprite? sprite))
        {
            if (RetryAt.TryGetValue(origin, out float at) && Time.unscaledTime < at) return;
            sprite = GlyphHelpers.Load(origin, __instance.blankKey);
            if (sprite == null) { RetryAt[origin] = Time.unscaledTime + 10f; return; }
            Cache[origin] = sprite;
        }
        __result.sprite = sprite;
    }
}

internal static class GlyphHelpers
{
    private static readonly HashSet<EInputActionOrigin> Reported = new();

    internal static Sprite? Load(EInputActionOrigin origin, Sprite reference)
    {
        Texture2D? tex = null;
        try
        {
            string path = SteamInput.GetGlyphPNGForActionOrigin(origin, ESteamInputGlyphSize.k_ESteamInputGlyphSize_Large, 0);
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) throw new FileNotFoundException($"no glyph file '{path}'");
            tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(File.ReadAllBytes(path))) throw new InvalidDataException($"can't decode '{path}'");
            return Loaded(origin, path, tex, reference);
        }
        catch (Exception e)
        {
            if (tex != null) UnityEngine.Object.Destroy(tex);
            if (Reported.Add(origin)) ExtraButtonSupportPlugin.Log?.LogWarning($"[extra] glyph for origin {(int)origin} not loaded: {e.Message}");
            return null;
        }
    }

    private static Sprite Loaded(EInputActionOrigin origin, string path, Texture2D tex, Sprite reference)
    {
        float ppu = tex.width * reference.pixelsPerUnit / reference.rect.width;
        ExtraButtonSupportPlugin.Log?.LogInfo($"[extra] glyph {(int)origin} {path} {tex.width}x{tex.height} ppu={ppu} (ref {reference.rect.width}x{reference.rect.height} ppu={reference.pixelsPerUnit})");
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), ppu);
    }
}
