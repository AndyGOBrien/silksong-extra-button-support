using System.Text.RegularExpressions;
using ExtraButtonSupport;

string root = args.Length > 0 && !args[0].StartsWith("--") ? args[0] : Path.Combine(Environment.GetEnvironmentVariable("HOME")!, ".local/share/Steam");
int outAt = Array.IndexOf(args, "--out");
string? outDir = outAt >= 0 && outAt + 1 < args.Length ? args[outAt + 1] : null;
if (outDir != null) Directory.CreateDirectory(outDir);
string manifest = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "actions.vdf"));
int failures = 0, checkedCount = 0;
int Count(string s, string needle) => (s.Length - s.Replace(needle, "").Length) / needle.Length;

foreach (LayoutBuilder.Layout layout in LayoutBuilder.Layouts)
{
    string path = Path.Combine(root, "controller_base/templates", layout.Template);
    if (!File.Exists(path)) { Console.WriteLine($"SKIP {layout.FileName}: no {layout.Template}"); continue; }
    string template = File.ReadAllText(path), built = LayoutBuilder.Build(template, layout, manifest);
    if (outDir != null) File.WriteAllText(Path.Combine(outDir, layout.FileName), built);
    var problems = new List<string>();

    string Strip(string s)
    {
        foreach ((string input, _) in layout.Binds) s = Regex.Replace(s, "\"" + Regex.Escape(input) + "\"\\s*\\{[\\s\\S]*?\n\t\t\t\\}", "");
        return s;
    }
    if (Count(Strip(built), "xinput_button") != Count(Strip(template), "xinput_button")) problems.Add("gamepad bindings changed");
    if (Count(built, "game_action") != layout.Binds.Length) problems.Add($"expected {layout.Binds.Length} action bindings, got {Count(built, "game_action")}");
    if (Count(built, "{") != Count(built, "}")) problems.Add("unbalanced braces");
    if (!Regex.IsMatch(built, $"\"name\"\\s*\"{LayoutBuilder.Set}\"")) problems.Add("preset not renamed");
    if (!built.Contains($"\"controller_type\"\t\t\"{layout.Type}\"")) problems.Add("controller_type missing");
    if (!built.Contains("\"extra_12\"")) problems.Add("actions block missing");
    foreach ((string input, int slot) in layout.Binds)
    {
        int at = built.IndexOf($"\"{input}\"");
        int next = at < 0 ? -1 : built.IndexOf("\"binding\"", at);
        if (at < 0 || next < 0 || !built.Substring(next, 80).Contains($"extra_{slot},")) problems.Add($"{input} not on extra_{slot}");
    }
    Console.WriteLine(problems.Count == 0 ? $"OK   {layout.FileName} \"{layout.Title}\"" : $"FAIL {layout.FileName}: {string.Join("; ", problems)}");
    failures += problems.Count == 0 ? 0 : 1;
    checkedCount++;
}
if (checkedCount == 0) { Console.WriteLine($"FAIL no templates found under {root}"); return 1; }
return failures;
