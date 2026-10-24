using System;

namespace ExpandedHotbars.Classes;

public static class PluginRepositoryGroup {
    public static string GetKey(string? installedFrom, bool thirdParty, string? sourceRepository) {
        if (!thirdParty) return "Dalamud official";
        if (!Uri.TryCreate(installedFrom, UriKind.Absolute, out var uri) || uri.Scheme is not ("https" or "http")) {
            if (!Uri.TryCreate(sourceRepository, UriKind.Absolute, out uri)) return "Other plugins";
        }
        return uri.AbsoluteUri;
    }
}
