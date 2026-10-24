using System.IO;
using System.Linq;

namespace ExpandedHotbars.Classes;

public static class PluginIconOverrides {
    /// <summary>Overrides are optional and keyed by Dalamud's stable internal plugin name.</summary>
    public static string? Find(string pluginDirectory, string internalName) {
        if (string.IsNullOrEmpty(internalName) || internalName[0] == '.' ||
            !internalName.All(character => char.IsAsciiLetterOrDigit(character) || character is '_' or '-' or '.')) return null;
        var path = Path.Combine(pluginDirectory, "images", "plugin-overrides", internalName + ".png");
        return File.Exists(path) ? path : null;
    }
}
