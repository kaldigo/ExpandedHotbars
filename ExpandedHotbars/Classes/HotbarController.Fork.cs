using System.Numerics;
using Dalamud.Utility;
using ExpandedHotbars.Configuration;

namespace ExpandedHotbars.Classes;

public sealed partial class HotbarController {
    public FavouritePluginSource FavouritePlugins { get; } = new();

    public void SetHotbarPosition(HotbarConfig config, Vector2 position) {
        ThreadSafety.AssertMainThread();
        config.Position = position;
        config.HasCustomPosition = true;
        if (hotbarNodes.TryGetValue(config, out var node)) {
            node.SetAnchorPosition(position);
        }
    }
}
