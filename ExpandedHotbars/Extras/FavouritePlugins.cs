using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace ExpandedHotbars.Extras;

public enum PluginFillDirection {
    LeftToRight,
    RightToLeft,
    TopToBottom,
    BottomToTop,
}

public class FavouritePlugins : ExtrasBase {
    public PluginFillDirection Direction = PluginFillDirection.LeftToRight;

    public override string Name => "Favourite Plugins";
    public override string Label => "Populate with favourite Dalamud plugins";
    public override bool HasConfiguration => true;
    public override Vector2 ConfigSize => new(380, 290);

    public override void DrawConfig() {
        string[] labels = ["Left -> Right", "Right -> Left", "Top -> Bottom", "Bottom -> Top"];
        ImGui.Text("Fill direction");
        foreach (var direction in Enum.GetValues<PluginFillDirection>()) {
            if (ImGui.RadioButton(labels[(int)direction], Direction == direction)) {
                Direction = direction;
                System.Config.Save();
            }
        }
        ImGui.Separator();
        ImGui.TextWrapped("Horizontal directions keep the configured width and add rows. Vertical directions keep the configured height and add columns. Plugins are sorted by name and refresh automatically.");
        ImGui.TextWrapped("Left-click: Open. Right-click: Settings. If only one is available, both clicks use it. Saved actions return when this feature is disabled.");
        if (System.HotbarController.FavouritePlugins.Error is { } error) {
            ImGui.TextWrapped(error);
        }
    }

    // Population belongs to the owning hotbar, rather than modifying saved action data.
    public override void Update() { }
}
