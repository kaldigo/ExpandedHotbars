using System.Linq;
using Dalamud.Plugin.Services;
using ExpandedHotbars.Extras;
using ExpandedHotbars.Enums;

namespace ExpandedHotbars.Nodes;

public sealed partial class HotbarOverlayNode {
    private bool UpdateFavouritePluginsIfEnabled() {
        if (Config.ExtraFeatures.OfType<FavouritePlugins>().FirstOrDefault(feature => feature.Enabled) is { } favourites) {
            UpdateFavouritePlugins(favourites);
            IGameConfig.Get().UiConfig.TryGetBool("HotbarLock", out var locked);
            padlockNode.IsLocked = locked;
            return true;
        }
        if (showingPlugins) {
            ClearPluginNodes();
            Config.UpdateFlags |= ConfigChangedKind.NeedsRebuild;
        }

        return false;
    }

    private void OnActionInvoked() {
        if (!Config.TryCloseAfterUse()) return;
        IsVisible = false;

        foreach (var feature in Config.ExtraFeatures) {
            if (feature is { IsValid: true, Enabled: true }) {
                feature.OnDisable();
            }
        }

        System.Config.Save();
    }
}
