using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace ExpandedHotbars.Classes;

/// <summary>
/// Isolates the two installer-only APIs. Launching plugins uses the public API.
/// If Dalamud changes its internals, only this feature becomes unavailable.
/// </summary>
public sealed class FavouritePluginSource {
    private object? configuration;
    private PropertyInfo? favourites;
    private object? imageCache;
    private MethodInfo? tryGetIcon;
    private FieldInfo? localPlugin;
    private bool iconsUnavailable;
    private bool initialized;
    private long nextRefresh;
    private IExposedPlugin[] snapshot = [];

    public string? Error { get; private set; }

    public IReadOnlyList<IExposedPlugin> GetPlugins() {
        if (Error is not null) return snapshot;
        if (Environment.TickCount64 < nextRefresh) return snapshot;
        nextRefresh = Environment.TickCount64 + 1000;
        try {
            Initialize();
            var names = ((IEnumerable)favourites!.GetValue(configuration)!).Cast<string>().ToHashSet(StringComparer.Ordinal);
            snapshot = ExpandedHotbars.PluginInterface.InstalledPlugins
                .Where(plugin => names.Contains(plugin.InternalName))
                .OrderBy(plugin => plugin.IsThirdParty ? 1 : 0)
                .ThenBy(GetRepositoryGroup, StringComparer.OrdinalIgnoreCase)
                .ThenBy(plugin => plugin.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(plugin => plugin.InternalName, StringComparer.Ordinal)
                .ToArray();
        }
        catch (Exception exception) {
            Fail(exception);
            snapshot = [];
        }
        return snapshot;
    }

    private string GetRepositoryGroup(IExposedPlugin plugin) {
        // The public manifest interface omits the install origin, but the local
        // manifest backing it retains the same URL used by Dalamud's updater.
        var local = localPlugin!.GetValue(plugin);
        var manifest = local?.GetType().GetProperty("Manifest")?.GetValue(local) ?? plugin.Manifest;
        var origin = manifest.GetType().GetProperty("InstalledFromUrl")?.GetValue(manifest) as string;
        return PluginRepositoryGroup.GetKey(origin, plugin.IsThirdParty, plugin.Manifest.RepoUrl);
    }

    /// <summary>Returns a borrowed installer texture; callers must take their own reference.</summary>
    public IDalamudTextureWrap? GetIcon(IExposedPlugin? plugin) {
        if (Error is not null || iconsUnavailable) return null;
        try {
            Initialize();
            if (imageCache is null) {
                imageCache = GetService("Dalamud.Interface.Internal.Windows.PluginImageCache");
                tryGetIcon = imageCache.GetType().GetMethod("TryGetIcon")
                    ?? throw new MissingMethodException("Dalamud plugin icons are unavailable.");
            }
            if (plugin is null) {
                return imageCache!.GetType().GetProperty("CorePluginIcon")?.GetValue(imageCache) as IDalamudTextureWrap;
            }
            object?[] arguments = [localPlugin!.GetValue(plugin), plugin.Manifest, plugin.IsThirdParty, null, null];
            tryGetIcon!.Invoke(imageCache, arguments);
            return arguments[3] as IDalamudTextureWrap;
        }
        catch (Exception exception) {
            iconsUnavailable = true;
            IPluginLog.Get().Warning(exception, "Favourite plugin installer icons are unavailable; using the packaged question mark.");
            return null;
        }
    }

    private void Initialize() {
        if (initialized) return;
        var assembly = typeof(IDalamudPluginInterface).Assembly;
        configuration = GetService("Dalamud.Configuration.Internal.DalamudConfiguration");
        favourites = configuration.GetType().GetProperty("FavoritePluginInternalName")
            ?? throw new MissingMemberException("Dalamud favourites are unavailable.");
        var exposedType = assembly.GetType("Dalamud.Plugin.ExposedPlugin", true)!;
        localPlugin = exposedType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(field => field.FieldType.FullName == "Dalamud.Plugin.Internal.Types.LocalPlugin");
        initialized = true;
    }

    private static object GetService(string name) {
        var assembly = typeof(IDalamudPluginInterface).Assembly;
        var type = assembly.GetType(name, true)!;
        var service = assembly.GetType("Dalamud.Service`1", true)!.MakeGenericType(type);
        return service.GetMethod("Get", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!.Invoke(null, null)!;
    }

    private void Fail(Exception exception) {
        if (Error is null) IPluginLog.Get().Warning(exception, "Favourite Plugins could not access the Dalamud installer APIs.");
        Error = "Favourite Plugins is unavailable with this Dalamud version. Other hotbar features are unaffected; check the plugin log for details.";
    }
}
