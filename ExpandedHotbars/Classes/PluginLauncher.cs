using System;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace ExpandedHotbars.Classes;

public static class PluginLauncher {
    public static bool OpenDalamud(IDalamudPluginInterface pluginInterface, bool settings) => settings
        ? pluginInterface.OpenDalamudSettingsTo()
        : pluginInterface.OpenPluginInstallerTo();

    public static string? MainCommand(string internalName) => internalName == "Stylist" ? "/stylist" : null;

    public static bool HasMainAction(IExposedPlugin plugin) => MainCommand(plugin.InternalName) is not null || plugin.HasMainUi;

    public static string MainActionLabel(IExposedPlugin plugin) => MainCommand(plugin.InternalName) ?? (plugin.HasMainUi ? "Open" : "Settings");

    public static bool Open(IExposedPlugin plugin, bool settings)
        => OpenWithCommand(plugin, settings, command => ICommandManager.Get().ProcessCommand(command));

    public static bool OpenWithCommand(IExposedPlugin plugin, bool settings, Func<string, bool> processCommand) {
        if (!plugin.IsLoaded) return false;
        if ((!settings || !plugin.HasConfigUi) && MainCommand(plugin.InternalName) is { } command) {
            return processCommand(command);
        }
        if ((settings && plugin.HasConfigUi) || !plugin.HasMainUi) {
            if (!plugin.HasConfigUi) return false;
            plugin.OpenConfigUi();
        }
        else {
            plugin.OpenMainUi();
        }
        return true;
    }
}
