using System;
using KamiToolKit.Nodes;

namespace ExpandedHotbars.Nodes;

/// <summary>
/// Reports slot activation after KamiToolKit has invoked the original action.
/// </summary>
public sealed class HotbarSlotNode : HotbarNode {
    public Action? OnActionInvoked { get; init; }

    public HotbarSlotNode() {
        // Mouse clicks use this delegate, while keybinds call the toolkit's
        // private action handler directly. Preserve both native execution paths.
        var executeSlot = OnClicked;
        OnClicked = node => {
            if (IsEmpty || executeSlot is null) return;

            executeSlot(node);
            OnActionInvoked?.Invoke();
        };
    }

    protected override void OnKeybindPressed() {
        if (IsEmpty) return;

        base.OnKeybindPressed();
        OnActionInvoked?.Invoke();
    }
}
