using System.Numerics;
using ExpandedHotbars.Configuration;

namespace ExpandedHotbars.Classes;

public static class HotbarAnchor {
    public static Vector2 Offset(Vector2 size, float scale, bool right, bool bottom)
        // Native nodes scale about their centre. This offset makes the selected
        // far edge meet the saved anchor, even when the bar is scaled.
        => new Vector2(right ? size.X : 0, bottom ? size.Y : 0) * ((1 + scale) / 2);

    public static Vector2 SlotPosition(HotbarConfig config, int row, int column) {
        if (config.AnchorRight) column = (int)config.Size.X - 1 - column;
        if (config.AnchorBottom) row = (int)config.Size.Y - 1 - row;
        return new Vector2(8) + new Vector2(column, row) * (new Vector2(44) + config.Spacing);
    }
}
