using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using ExpandedHotbars.Enums;
using ExpandedHotbars.Extensions;

namespace ExpandedHotbars.Windows;

public partial class ConfigWindow {
    private void DrawConfigTab() {
        if (selectedConfig is null) return;

        using var tabChild = ImRaii.Child("ConfigTab", ImGui.Area);
        if (!tabChild) return;

        using var itemSpacing = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(0.0f, 8.0f));

        ImGui.ScaledDummy(5.0f);

        ImGui.Label("Hotbar Name");
        ImGui.SetNextItemWidth(ImGui.AreaWidth);
        ImGui.InputTextWithHint("##HotbarName", "name", ref selectedConfig.HotbarName);

        if (ImGui.IsItemDeactivatedAfterEdit()) {
            System.Config.Save();
        }

        ImGui.Label("Hotbar GUID", "A unique value associated with this hotbar\nPrevents accidentally importing multiple copies of the same hotbar");
        ImGui.SetNextItemWidth(ImGui.AreaWidth);
        ImGui.TextColored(KnownColor.Gray.Vector(), selectedConfig.Guid.ToString().ToUpperInvariant());

        ImGui.Label("Hotbar Size");
        ImGui.SetNextItemWidth(ImGui.AreaWidth);
        ImGui.InputFloat2("##HotbarSize", ref selectedConfig.Size, 1.0f, 5.0f, "%.0f");
        selectedConfig.Size.X = Math.Clamp(selectedConfig.Size.X, 1.0f, 50.0f);
        selectedConfig.Size.Y = Math.Clamp(selectedConfig.Size.Y, 1.0f, 50.0f);

        if (ImGui.IsItemDeactivatedAfterEdit()) {
            selectedConfig.UpdateFlags |= ConfigChangedKind.NeedsRebuild;
            System.Config.Save();
        }

        DrawAnchorDirections();

        ImGui.Label("Hotbar Slot Spacing");
        ImGui.SetNextItemWidth(ImGui.AreaWidth);
        ImGui.InputFloat2("##HotbarSpacing", ref selectedConfig.Spacing, 1.0f, 5.0f, "%.0f");

        if (ImGui.IsItemDeactivatedAfterEdit()) {
            selectedConfig.UpdateFlags |= ConfigChangedKind.NeedsUpdate;
            System.Config.Save();
        }

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

        ImGui.Label("Scale");
        ImGui.SetNextItemWidth(ImGui.AreaWidth);
        ImGui.SliderFloat("##Scale", ref selectedConfig.Scale, 0.5f, 5.0f);

        if (ImGui.IsItemDeactivatedAfterEdit()) {
            selectedConfig.UpdateFlags |= ConfigChangedKind.NeedsUpdate;
            System.Config.Save();
        }

        ImGui.Label("Enabled");
        if (ImGui.Checkbox("##Enabled", ref selectedConfig.IsEnabled)) {
            foreach (var feature in selectedConfig.ExtraFeatures) {
                if (selectedConfig.IsEnabled) {
                    feature.OnEnable();
                }
                else {
                    feature.OnDisable();
                }
            }

            System.Config.Save();
        }

        ImGui.Label("Enable Moving");
        ImGui.Checkbox("##EnableMoving", ref selectedConfig.IsMovingEnabled);
        // Intentionally don't save this field, it's JsonIgnored.

        ImGui.Label("Enable Padlock Button");
        if (ImGui.Checkbox("##EnablePadlock", ref selectedConfig.ShowPadlockButton)) {
            selectedConfig.UpdateFlags |= ConfigChangedKind.NeedsUpdate;
            System.Config.Save();
        }

        ImGui.Label("Allow Clicking Actions", "When checked allows invoking actions via mouse clicks.");
        if (ImGui.Checkbox("##EnableClicking", ref selectedConfig.EnableClicking)) {
            selectedConfig.UpdateFlags |= ConfigChangedKind.NeedsUpdate;
            System.Config.Save();
        }

        ImGui.Label("Close After Use", "Closes this bar after activating an action by click or keybind, even if the action cannot be used.\n" +
                                      "Reopen it with /exhotbar show <hotbar name>, /exhotbar toggle <hotbar name>, or Enabled above.");
        if (ImGui.Checkbox("##CloseAfterUse", ref selectedConfig.CloseAfterUse)) {
            System.Config.Save();
        }

        ImGui.Label("Delete Hotbar");
        using (ImRaii.Disabled(!IKeyState.Get().DeleteKeybindPressed)) {
            if (ImGui.Button("Delete")) {
                System.Config.Hotbars.Remove(selectedConfig);
                System.HotbarController.RemoveHotbar(selectedConfig);
                selectedConfig = null;
                System.Config.Save();
            }
        }

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled) && !IKeyState.Get().DeleteKeybindPressed) {
            ImGui.SetTooltip("Hold Control + Shift to enable button.");
        }
    }
}
