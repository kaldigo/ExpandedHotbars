using System.Numerics;
using ExpandedHotbars.Classes;

namespace ExpandedHotbars.Nodes;

public sealed partial class HotbarOverlayNode {
    private Vector2 anchorOffset;

    public void SetAnchorPosition(Vector2 position) => Position = position - anchorOffset;

    private Vector2 GetAnchorPosition() => Position + anchorOffset;

    private void UpdateAnchor() {
        var next = HotbarAnchor.Offset(Size, Config.Scale, Config.AnchorRight, Config.AnchorBottom);
        // Apply only the change in offset so dragging isn't reset every frame.
        Position += anchorOffset - next;
        anchorOffset = next;
    }
}
