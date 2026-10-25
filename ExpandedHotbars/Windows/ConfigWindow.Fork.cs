using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using ExpandedHotbars.Extensions;

namespace ExpandedHotbars.Windows;

public partial class ConfigWindow {
    private void DrawForkPositionSettings() {
        if (selectedConfig is null) return;
        ImGui.Label("Position X / Y", "Exact screen coordinates. X increases to the right; Y increases downward.");
        var position = selectedConfig.Position;
        ImGui.SetNextItemWidth(ImGui.AreaWidth);
        if (ImGui.InputFloat2("##HotbarPosition", ref position, 1.0f, 10.0f, "%.0f") &&
            float.IsFinite(position.X) && float.IsFinite(position.Y)) {
            System.HotbarController.SetHotbarPosition(selectedConfig, position);
        }
        if (ImGui.IsItemDeactivatedAfterEdit()) {
            System.Config.Save();
        }
    }

    private void DrawForkActivationSettings() {
        if (selectedConfig is null) return;
        ImGui.Label("Close After Use", "Closes this bar after activating an action by click or keybind, even if the action cannot be used.\n" +
                                      "Reopen it with /exhotbar show <hotbar name>, /exhotbar toggle <hotbar name>, or Enabled above.");
        if (ImGui.Checkbox("##CloseAfterUse", ref selectedConfig.CloseAfterUse)) {
            System.Config.Save();
        }
    }
}
