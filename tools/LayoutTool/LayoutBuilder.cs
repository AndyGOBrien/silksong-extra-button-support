using System.Text.RegularExpressions;

namespace ExtraButtonSupport;

internal static class LayoutBuilder
{
    internal const string Set = "Default";
    internal const string Name = "Extra Button Support";

    internal sealed class Layout
    {
        internal readonly string Key, Title, Type, Template;
        internal readonly (string Input, int Slot)[] Binds;
        internal Layout(string key, string title, string type, string template, (string, int)[] binds) =>
            (Key, Title, Type, Template, Binds) = (key, title, type, template, binds);
        internal string FileName => $"extrabuttonsupport_{Key}_{Type}.vdf";
    }

    private static readonly (string, int)[] BackButtons =
    {
        ("button_back_left_upper", 1), ("button_back_right_upper", 2), ("button_back_left", 3), ("button_back_right", 4),
    };

    private static readonly (string Type, string Template)[] Types =
    {
        ("controller_triton", "controller_neptune_gamepad_joystick.vdf"),
        ("controller_neptune", "controller_neptune_gamepad_joystick.vdf"),
        ("controller_xboxone", "controller_xboxone_gamepad_joystick.vdf"),
        ("controller_xboxelite", "controller_xboxone_gamepad_joystick.vdf"),
        ("controller_xbox360", "controller_xbox360_gamepad_joystick.vdf"),
        ("controller_ps5", "controller_ps5_gamepad_joystick.vdf"),
        ("controller_ps5_edge", "controller_ps5_gamepad_joystick.vdf"),
        ("controller_ps4", "controller_ps4_gamepad_joystick.vdf"),
        ("controller_switch_pro", "controller_switch_pro_gamepad_joystick.vdf"),
        ("controller_8bitdo", "controller_generic_gamepad_joystick.vdf"),
        ("controller_generic", "controller_generic_gamepad_joystick.vdf"),
    };

    internal static readonly Layout[] Layouts = BuildList();

    private static Layout[] BuildList()
    {
        var list = new System.Collections.Generic.List<Layout>();
        foreach ((string type, string template) in Types)
            list.Add(new Layout("byo", $"{Name} – Build your own", type, template, System.Array.Empty<(string, int)>()));
        string Template(string type) => System.Array.Find(Types, t => t.Type == type).Template;
        list.Add(new Layout("ready", $"{Name} – Steam Controller", "controller_triton", Template("controller_triton"), BackButtons));
        list.Add(new Layout("ready", $"{Name} – Steam Deck", "controller_neptune", Template("controller_neptune"), BackButtons));
        list.Add(new Layout("ready", $"{Name} – Xbox Elite", "controller_xboxelite", Template("controller_xboxelite"), BackButtons));
        list.Add(new Layout("ready", $"{Name} – DualSense Edge", "controller_ps5_edge", Template("controller_ps5_edge"), BackButtons));
        list.Add(new Layout("vader5pro", $"{Name} \u2013 Vader 5 Pro", "controller_generic", Template("controller_generic"),
            Concat(BackButtons, ("button_macro2", 5), ("button_macro3", 6), ("button_macro0", 7), ("button_macro1", 8), ("button_macro4", 9))));
        return list.ToArray();
    }

    private static (string, int)[] Concat((string, int)[] first, params (string, int)[] rest)
    {
        var all = new (string, int)[first.Length + rest.Length];
        first.CopyTo(all, 0);
        rest.CopyTo(all, first.Length);
        return all;
    }

    internal static string Build(string template, Layout layout, string manifest)
    {
        string text = template.TrimStart('\ufeff');
        int firstGroup = text.IndexOf("\n\t\"group\"");
        if (firstGroup < 0) throw new System.FormatException("template has no groups");
        string timestamp = Regex.Match(text, "\"Timestamp\"\\s*\"(\\d+)\"", RegexOptions.IgnoreCase).Groups[1].Value;
        string description = layout.Binds.Length == 0
            ? "Steam's Gamepad layout plus Extra Button 1-12 actions. Bind any extra button to an Extra Button, then assign it in Silksong's Remap Controller screen."
            : "Steam's Gamepad layout with this controller's extra buttons sent as Extra Button actions. Assign them in Silksong's Remap Controller screen.";

        string header = "\"controller_mappings\"\n{\n" +
            "\t\"version\"\t\t\"3\"\n\t\"revision\"\t\t\"1\"\n" +
            $"\t\"title\"\t\t\"{layout.Title}\"\n" +
            $"\t\"description\"\t\t\"{description}\"\n" +
            $"\t\"progenitor\"\t\t\"template://{layout.Template}\"\n" +
            "\t\"export_type\"\t\t\"personal_local\"\n" +
            $"\t\"controller_type\"\t\t\"{layout.Type}\"\n" +
            "\t\"major_revision\"\t\t\"0\"\n\t\"minor_revision\"\t\t\"0\"\n" +
            $"\t\"Timestamp\"\t\t\"{(timestamp.Length > 0 ? timestamp : "0")}\"\n" +
            ManifestBlocks(manifest) +
            "\t\"action_layers\"\n\t{\n\t}";
        text = header + text.Substring(firstGroup);

        foreach ((string input, int slot) in layout.Binds) text = Bind(text, input, slot);

        var preset = new Regex("(\"preset\"\\s*\\{\\s*\"id\"\\s*\"\\d+\"\\s*\"name\"\\s*)\"Default\"");
        if (!preset.IsMatch(text)) throw new System.FormatException("Default preset not found");
        return preset.Replace(text, "$1\"" + Set + "\"", 1);
    }

    private static string BindingLine(int slot) =>
        $"\t\t\t\t\t\t\t\"binding\"\t\t\"game_action {Set} extra_{slot}, Extra Button {slot}, , \"";

    private static string Bind(string text, string input, int slot)
    {
        var bindings = new Regex("(\"" + Regex.Escape(input) + "\"\\s*\\{\\s*\"activators\"\\s*\\{\\s*\"Full_Press\"\\s*\\{\\s*\"bindings\"\\s*\\{)[^{}]*(\\})");
        if (bindings.IsMatch(text)) return bindings.Replace(text, m => m.Groups[1].Value + "\n" + BindingLine(slot) + "\n\t\t\t\t\t\t" + m.Groups[2].Value, 1);
        if (text.Contains("\"" + input + "\"")) throw new System.FormatException(input + " has an unexpected shape");

        var switches = new Regex("(\"mode\"\\s*\"switches\"[\\s\\S]*?\"inputs\"\\s*\\{)");
        if (!switches.IsMatch(text)) throw new System.FormatException("switches group not found");
        string block = $"\n\t\t\t\"{input}\"\n\t\t\t{{\n\t\t\t\t\"activators\"\n\t\t\t\t{{\n\t\t\t\t\t\"Full_Press\"\n\t\t\t\t\t{{\n" +
            $"\t\t\t\t\t\t\"bindings\"\n\t\t\t\t\t\t{{\n{BindingLine(slot)}\n\t\t\t\t\t\t}}\n\t\t\t\t\t}}\n\t\t\t\t}}\n\t\t\t}}";
        return switches.Replace(text, m => m.Groups[1].Value + block, 1);
    }

    private static string ManifestBlocks(string manifest)
    {
        string body = manifest.Trim();
        int open = body.IndexOf('{'), close = body.LastIndexOf('}');
        return body.Substring(open + 1, close - open - 1).Trim('\r', '\n') + "\n";
    }
}
