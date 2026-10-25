using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ExpandedHotbars.Classes;
using ExpandedHotbars.Extras;

namespace ExpandedHotbars.Nodes;

public sealed partial class HotbarOverlayNode {
    private readonly List<FavouritePluginSlotNode> pluginNodes = [];
    private bool showingPlugins;
    private string[] pluginNames = [];

    private void UpdateFavouritePlugins(FavouritePlugins feature) {
        var source = System.HotbarController.FavouritePlugins;
        var plugins = source.GetPlugins();
        var names = plugins.Select(plugin => plugin.InternalName).ToArray();
        if (!showingPlugins || !pluginNames.SequenceEqual(names)) {
            foreach (var node in hotbarNodes) node.Dispose();
            hotbarNodes.Clear();
            ClearPluginNodes();
            pluginNames = names;
            showingPlugins = true;
            // Dalamud always occupies the first position in the selected fill direction.
            for (var index = 0; index <= plugins.Count; index++) {
                var node = new FavouritePluginSlotNode {
                    NodeId = (uint)index + 2,
                    Plugin = index == 0 ? null : plugins[index - 1],
                    Size = new Vector2(44, 44),
                    OnInvoked = OnActionInvoked,
                };
                node.AttachNode(this);
                pluginNodes.Add(node);
            }
        }

        var layout = PluginBarLayout.Create(plugins.Count + 1, (int)Config.Size.X, (int)Config.Size.Y, feature.Direction, feature.ReverseWrap);
        var step = new Vector2(44, 44) + Config.Spacing;
        Size = new Vector2(layout.Columns, layout.Rows) * step + new Vector2(16, 16);
        Origin = Size / 2;
        UpdateAnchor();
        padlockNode.Position = Size + new Vector2(8, -42);
        padlockNode.IsVisible = Config.ShowPadlockButton;
        EnableMoving = Config.IsMovingEnabled;

        for (var index = 0; index < pluginNodes.Count; index++) {
            var node = pluginNodes[index];
            var (row, column) = layout.GetSlot(index);
            node.Position = new Vector2(8, 8) + new Vector2(column, row) * step;
            node.Plugin = index == 0 ? null : plugins[index - 1];
            node.AllowClicking = Config.EnableClicking;
            node.Refresh(source);
        }
    }

    private void ClearPluginNodes() {
        foreach (var node in pluginNodes) node.Dispose();
        pluginNodes.Clear();
        pluginNames = [];
        showingPlugins = false;
    }
}
