using System.Reflection;
using System.Text.Json;
using ExpandedHotbars.Classes;
using ExpandedHotbars.Configuration;
using ExpandedHotbars.Extras;

internal static class PluginBarChecks {
    public static void Run() {
        Check(PluginRepositoryGroup.GetKey("https://love.puni.sh/ment.json", true, null) !=
              PluginRepositoryGroup.GetKey("https://puni.sh/api/repository/veyn", true, null), "distinct install feeds were combined");
        Check(PluginRepositoryGroup.GetKey("https://raw.githubusercontent.com/kaldigo/DalamudPlugins/main/pluginmaster.json", true, null) !=
              PluginRepositoryGroup.GetKey("https://raw.githubusercontent.com/kaldigo/AnotherFeed/main/repo.json", true, null), "distinct repositories from one owner were combined");
        Check(PluginRepositoryGroup.GetKey("OFFICIAL", false, "https://github.com/PunishXIV/APlugin") == "Dalamud official", "official plugin grouped by source author instead of install repo");
        Check(PluginRepositoryGroup.GetKey(null, true, "https://github.com/kaldigo/ExpandedHotbars") == "https://github.com/kaldigo/ExpandedHotbars", "missing origin fallback");
        Check(PluginRepositoryGroup.GetKey("https://puni.sh.example.org/repo.json", true, null) != "puni.sh", "unrelated domain grouped as Punish");
        Console.WriteLine("PASS: Repository groups preserve individual install feeds and distinguish the default repository");
        foreach (var direction in Enum.GetValues<PluginFillDirection>()) {
            foreach (var reverseWrap in new[] { false, true }) {
                for (var count = 0; count <= 121; count++) {
                    var layout = PluginBarLayout.Create(count, 3, 2, direction, reverseWrap);
                    var horizontal = direction is PluginFillDirection.LeftToRight or PluginFillDirection.RightToLeft;
                    Check(horizontal ? layout.Columns == 3 : layout.Rows == 2, "configured axis was changed");
                    Check(layout.Rows * layout.Columns >= count, "a favourite was truncated");
                    var occupied = new HashSet<(int, int)>();
                    for (var index = 0; index < count; index++) {
                        var (row, column) = layout.GetSlot(index);
                        Check(row >= 0 && row < layout.Rows && column >= 0 && column < layout.Columns, "slot outside layout");
                        Check(occupied.Add((row, column)), "overlapping plugins");
                    }
                }
            }
        }
        Check(PluginBarLayout.Create(7, 3, 2, PluginFillDirection.LeftToRight).GetSlot(3) == (1, 0), "left-to-right wrap");
        Check(PluginBarLayout.Create(7, 3, 2, PluginFillDirection.RightToLeft).GetSlot(3) == (1, 2), "right-to-left wrap");
        Check(PluginBarLayout.Create(7, 3, 2, PluginFillDirection.TopToBottom).GetSlot(2) == (0, 1), "top-to-bottom wrap");
        Check(PluginBarLayout.Create(7, 3, 2, PluginFillDirection.BottomToTop).GetSlot(2) == (1, 1), "bottom-to-top wrap");
        (PluginFillDirection Direction, (int Row, int Column)[] Slots)[] reversedLayouts = [
            (PluginFillDirection.LeftToRight, [(1, 0), (1, 1), (1, 2), (0, 0), (0, 1)]),
            (PluginFillDirection.RightToLeft, [(1, 2), (1, 1), (1, 0), (0, 2), (0, 1)]),
            (PluginFillDirection.TopToBottom, [(0, 2), (1, 2), (0, 1), (1, 1), (0, 0)]),
            (PluginFillDirection.BottomToTop, [(1, 2), (0, 2), (1, 1), (0, 1), (1, 0)]),
        ];
        foreach (var (direction, slots) in reversedLayouts) {
            var layout = PluginBarLayout.Create(5, 3, 2, direction, true);
            Check(Enumerable.Range(0, 5).Select(layout.GetSlot).SequenceEqual(slots), "reversed wrap changed fill order or placed the partial row/column on the wrong side");
            Check(PluginBarLayout.Create(1, 1, 1, direction, true).GetSlot(0) == (0, 0), "single-slot reversed wrap");
        }
        Console.WriteLine("PASS: All eight fill/wrap combinations, empty lists and multi-row/column overflow preserve correct unique slot positions");

        var launcher = typeof(PluginLauncher).GetMethod("Open")!;
        var pluginType = launcher.GetParameters()[0].ParameterType;
        foreach (var main in new[] { false, true }) {
            foreach (var config in new[] { false, true }) {
                foreach (var settings in new[] { false, true }) {
                    var plugin = DispatchProxy.Create(pluginType, typeof(LauncherPluginProxy));
                    var proxy = (LauncherPluginProxy)plugin;
                    proxy.Main = main;
                    proxy.Config = config;
                    var opened = (bool)launcher.Invoke(null, [plugin, settings])!;
                    var expected = (settings && config) || !main ? config ? "OpenConfigUi" : null : "OpenMainUi";
                    Check(opened == (expected is not null) && proxy.Opened == expected, "incorrect click fallback");
                    proxy.Loaded = false;
                    proxy.Opened = null;
                    Check(!(bool)launcher.Invoke(null, [plugin, settings])! && proxy.Opened is null, "launched disabled plugin");
                }
            }
        }
        Console.WriteLine("PASS: Left/right click opens the expected UI for both, either or neither available button; unloaded plugins stay closed");

        var dalamudLauncher = typeof(PluginLauncher).GetMethod("OpenDalamud")!;
        foreach (var settings in new[] { false, true }) {
            var pluginInterface = DispatchProxy.Create(dalamudLauncher.GetParameters()[0].ParameterType, typeof(DalamudInterfaceProxy));
            var proxy = (DalamudInterfaceProxy)pluginInterface;
            Check((bool)dalamudLauncher.Invoke(null, [pluginInterface, settings])!, "Dalamud launcher did not report success");
            Check(proxy.Opened == (settings ? "OpenDalamudSettingsTo" : "OpenPluginInstallerTo"), "wrong Dalamud window");
            Check(proxy.Tab == (settings ? "General" : "AllPlugins") && proxy.Search is null, "unexpected Dalamud page or search filter");
            proxy.Result = false;
            Check(!(bool)dalamudLauncher.Invoke(null, [pluginInterface, settings])!, "failed Dalamud launch reported success");
        }
        Console.WriteLine("PASS: Dalamud opens the main plugin list or settings and propagates unavailable-window results");

        var options = new JsonSerializerOptions { IncludeFields = true };
        var legacyFeature = JsonSerializer.Deserialize<FavouritePlugins>("""{"Enabled":true,"Direction":1}""", options)!;
        Check(legacyFeature.Direction == PluginFillDirection.RightToLeft && !legacyFeature.ReverseWrap, "existing features must keep their wrap order");
        var bar = JsonSerializer.Deserialize<HotbarConfig>("""{"Size":{"X":3,"Y":2},"Actions":{"0,0":{"DragDropType":63,"ActionId":224}}}""", options)!;
        bar.ExtraFeatures.Add(new FavouritePlugins { Enabled = true, Direction = PluginFillDirection.BottomToTop, ReverseWrap = true });
        var restored = JsonSerializer.Deserialize<HotbarConfig>(JsonSerializer.Serialize(bar, options), options)!;
        Check(restored.ExtraFeatures.Single() is FavouritePlugins { Enabled: true, Direction: PluginFillDirection.BottomToTop, ReverseWrap: true }, "feature export/import");
        Check(restored.Actions.Count == 1 && restored.Size == bar.Size, "population destroyed manual bar data");
        Console.WriteLine("PASS: Feature settings export/import alongside original manual actions and dimensions");

        var pixels = Enumerable.Repeat((byte)255, 80 * 80 * 4).ToArray();
        RoundedPluginIcon.Apply(pixels, 80, 80, 80 * 4);
        Check(pixels[3] == 0 && pixels[(40 * 80 + 40) * 4 + 3] == 255, "icon corner mask");
        Check(pixels.Where((_, index) => index % 4 == 3).Any(alpha => alpha is > 0 and < 255), "missing antialiasing");
        Check(pixels.Where((_, index) => index % 4 != 3).All(channel => channel == 255), "mask changed icon colors");
        Array.Fill(pixels, (byte)0);
        RoundedPluginIcon.Apply(pixels, 80, 80, 80 * 4);
        Check(pixels.All(channel => channel == 0), "mask destroyed existing transparency");
        Console.WriteLine("PASS: Rounded icon corners preserve colors and existing transparency");

        byte[] translucent = [255, 0, 0, 255, 0, 0, 0, 0, 0, 0, 0, 128];
        RoundedPluginIcon.CompositeOnWhite(translucent, 3, 1, 12);
        Check(translucent.SequenceEqual(new byte[] { 255, 0, 0, 255, 255, 255, 255, 255, 127, 127, 127, 255 }), "Dalamud white background composition");
        Console.WriteLine("PASS: Dalamud gets an opaque white background while preserving its opaque logo colors");

        var temporary = Path.Combine(Path.GetTempPath(), "expanded-hotbar-icons-" + Guid.NewGuid());
        var directory = Path.Combine(temporary, "images", "plugin-overrides");
        Directory.CreateDirectory(directory);
        var icon = Path.Combine(directory, "Aetherworks.png");
        try {
            File.WriteAllBytes(icon, [1]); // Lookup is independent of the texture decoder.
            Check(PluginIconOverrides.Find(temporary, "Aetherworks") == icon, "packaged override was not found");
            Check(PluginIconOverrides.Find(temporary, "UnknownPlugin") is null, "unknown plugin did not fall back");
            Check(PluginIconOverrides.Find(temporary, "../Aetherworks") is null &&
                  PluginIconOverrides.Find(temporary, "..\\Aetherworks") is null &&
                  PluginIconOverrides.Find(temporary, "") is null, "override lookup escaped its asset folder");
        }
        finally {
            File.Delete(icon);
            Directory.Delete(directory);
            Directory.Delete(Path.GetDirectoryName(directory)!);
            Directory.Delete(temporary);
        }
        Console.WriteLine("PASS: Overrides resolve by internal identity; missing entries fall back and invalid paths are rejected");
    }

    private static void Check(bool condition, string description) {
        if (!condition) throw new InvalidOperationException(description);
    }
}

public class LauncherPluginProxy : DispatchProxy {
    public bool Main;
    public bool Config;
    public bool Loaded = true;
    public string? Opened;

    protected override object? Invoke(MethodInfo? method, object?[]? arguments) => method!.Name switch {
        "get_IsLoaded" => Loaded,
        "get_HasMainUi" => Main,
        "get_HasConfigUi" => Config,
        "OpenMainUi" or "OpenConfigUi" => Open(method.Name),
        _ => throw new NotSupportedException(method.Name),
    };

    private object? Open(string name) {
        Opened = name;
        return null;
    }
}

public class DalamudInterfaceProxy : DispatchProxy {
    public bool Result = true;
    public string? Opened;
    public string? Tab;
    public string? Search;

    protected override object? Invoke(MethodInfo? method, object?[]? arguments) {
        Opened = method!.Name;
        Tab = arguments![0]!.ToString();
        Search = (string?)arguments[1];
        return Result;
    }
}
