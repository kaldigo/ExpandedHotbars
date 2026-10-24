using Dalamud.Plugin;

namespace ExpandedHotbars.Classes;

public static class PluginLauncher {
    public static bool OpenDalamud(IDalamudPluginInterface pluginInterface, bool settings) => settings
        ? pluginInterface.OpenDalamudSettingsTo()
        : pluginInterface.OpenPluginInstallerTo();

    public static bool Open(IExposedPlugin plugin, bool settings) {
        if (!plugin.IsLoaded) return false;
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
