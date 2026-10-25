using Dalamud.Bindings.ImGui;
using ExpandedHotbars.Enums;

namespace ExpandedHotbars.Windows;

public partial class ConfigWindow {
    private void DrawAnchorDirections() {
        if (selectedConfig is null) return;
        var changed = ImGui.Checkbox("Anchor at right edge (extend left)", ref selectedConfig.AnchorRight);
        changed |= ImGui.Checkbox("Anchor at bottom edge (extend upward)", ref selectedConfig.AnchorBottom);
        ImGui.TextWrapped("X/Y stays at the selected anchor. The bar extends left or upward from it. Ordinary slots follow that direction; Favourite Plugins keeps its own fill order.");
        if (!changed) return;
        selectedConfig.UpdateFlags |= ConfigChangedKind.NeedsUpdate;
        System.Config.Save();
    }
}
