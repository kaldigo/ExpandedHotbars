using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.Json;
using ExpandedHotbars.Configuration;

// Load the real plugin's managed data types without starting Dalamud or native UI.
var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
var references = Environment.GetEnvironmentVariable("DALAMUD_HOME")
    ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XIVLauncher/addon/Hooks/dev");
AssemblyLoadContext.Default.Resolving += (_, name) => {
    foreach (var folder in new[] { Path.Combine(root, "ExpandedHotbars/bin/Release"), references }) {
        var path = Path.Combine(folder, name.Name + ".dll");
        if (File.Exists(path)) return AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(path));
    }
    return null;
};
RunChecks();

[MethodImpl(MethodImplOptions.NoInlining)]
static void RunChecks() {
    var options = new JsonSerializerOptions { IncludeFields = true };
    var legacy = JsonSerializer.Deserialize<HotbarConfig>("{\"HotbarName\":\"Legacy\",\"IsEnabled\":true}", options)!;
    Check(!legacy.CloseAfterUse && !legacy.TryCloseAfterUse() && legacy.IsEnabled, "Legacy settings keep the bar open");
    var first = new HotbarConfig { CloseAfterUse = true };
    var other = new HotbarConfig { CloseAfterUse = true };
    Check(first.TryCloseAfterUse() && !first.IsEnabled && other.IsEnabled, "Closing affects only the chosen bar");
    Check(!first.TryCloseAfterUse(), "Already-closed bars do not close or save again");
    var saved = JsonSerializer.Serialize(first, options);
    var loaded = JsonSerializer.Deserialize<HotbarConfig>(saved, options)!;
    Check(loaded.CloseAfterUse && !loaded.IsEnabled && loaded.Guid == first.Guid, "Option, closed state and identity survive saving");
    loaded.IsEnabled = true; // The existing show/toggle/config controls reopen the bar.
    Check(loaded.TryCloseAfterUse() && !loaded.IsEnabled, "Reopened bars close on their next activation");
    var ordinary = new HotbarConfig();
    Check(!ordinary.TryCloseAfterUse() && ordinary.IsEnabled, "New bars default to leaving actions open");
}

static void Check(bool condition, string description) {
    if (!condition) throw new InvalidOperationException(description);
    Console.WriteLine("PASS: " + description);
}
