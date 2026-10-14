using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ExtraButtonSupport;

[HarmonyPatch]
internal static class MenuRowPatches
{
    [HarmonyPrefix, HarmonyPatch(typeof(UIManager), nameof(UIManager.UIGoToRemapControllerMenu))]
    private static void AddToolRows(UIManager __instance)
    {
        try { MenuRows.Ensure(__instance.uiButtonSkins.mappableControllerButtons); }
        catch (System.Exception e) { ExtraButtonSupportPlugin.Log?.LogError($"[extra] tool rows not added: {e}"); }
    }

    private static readonly AccessTools.FieldRef<MappableControllerButton, InControl.PlayerAction> RowAction =
        AccessTools.FieldRefAccess<MappableControllerButton, InControl.PlayerAction>("playerAction");
    private static readonly AccessTools.FieldRef<MappableControllerButton, InControl.PlayerAction> ActionToSwap =
        AccessTools.FieldRefAccess<MappableControllerButton, InControl.PlayerAction>("actionToSwap");
    private static readonly AccessTools.FieldRef<MappableControllerButton, InControl.BindingSource> BindingToSwap =
        AccessTools.FieldRefAccess<MappableControllerButton, InControl.BindingSource>("bindingToSwap");

    [HarmonyPostfix, HarmonyPatch(typeof(MappableControllerButton), nameof(MappableControllerButton.OnBindingFound))]
    private static void UnbindInsteadOfSwap(MappableControllerButton __instance)
    {
        if (ActionToSwap(__instance) is InControl.PlayerAction other && (Tools.IsTool(RowAction(__instance)) || Tools.IsTool(other)))
            BindingToSwap(__instance) = null!;
    }
}

internal static class MenuRows
{
    private const string Prefix = "ExtraButtonSupport ";
    private const float RowHeight = 100f;

    internal static void Ensure(RectTransform rows)
    {
        if (rows.Find(Prefix + Tools.Names[0]) != null) return;
        MappableControllerButton? cast = Find(rows, "CastButton"), quickCast = Find(rows, "QCastButton");
        if (cast == null || quickCast == null) { ExtraButtonSupportPlugin.Log?.LogError("[extra] remap rows not found; tool rows not added"); return; }

        rows.anchoredPosition += new Vector2(0, RowHeight);

        Selectable? below = cast.navigation.selectOnDown;
        MappableControllerButton up = Clone(cast, 0, 1), down = Clone(cast, 2, 2), neutral = Clone(quickCast, 1, 1),
            scuttle = Clone(quickCast, Tools.ScuttleIndex, 2);

        Link(cast, down: up);
        Link(up, upTo: cast, down: down, right: neutral);
        Link(down, upTo: up, down: below, right: scuttle);
        Link(quickCast, down: neutral);
        Link(neutral, upTo: quickCast, down: scuttle, left: up);
        Link(scuttle, upTo: neutral, down: below, left: down);
        if (below != null) Link(below, upTo: down);
        ExtraButtonSupportPlugin.Log?.LogInfo("[extra] added tool rows to Remap Controller screen");
    }

    private static MappableControllerButton? Find(RectTransform rows, string name) =>
        rows.Find(name)?.GetComponent<MappableControllerButton>();

    private static MappableControllerButton Clone(MappableControllerButton template, int tool, int rowsBelow)
    {
        var holder = new GameObject("ExtraButtonSupport holder");
        holder.SetActive(false);
        MappableControllerButton row = Object.Instantiate(template, holder.transform);
        row.name = Prefix + Tools.Names[tool];
        row.actionButtonType = (GlobalEnums.HeroActionButton)(Tools.ButtonBase + tool);

        Transform label = row.transform.Find("Input Button Text");
        foreach (Component c in label.GetComponents<Component>())
            if (c.GetType().Name == "AutoLocalizeTextUI") Object.DestroyImmediate(c);
        label.GetComponent<Text>().text = Tools.Names[tool];

        var rect = (RectTransform)row.transform;
        rect.SetParent(template.transform.parent, false);
        rect.anchoredPosition = ((RectTransform)template.transform).anchoredPosition - new Vector2(0, RowHeight * rowsBelow);
        Object.Destroy(holder);
        return row;
    }

    private static void Link(Selectable s, Selectable? upTo = null, Selectable? down = null, Selectable? left = null, Selectable? right = null)
    {
        Navigation nav = s.navigation;
        nav.mode = Navigation.Mode.Explicit;
        if (upTo != null) nav.selectOnUp = upTo;
        if (down != null) nav.selectOnDown = down;
        if (left != null) nav.selectOnLeft = left;
        if (right != null) nav.selectOnRight = right;
        s.navigation = nav;
    }
}
