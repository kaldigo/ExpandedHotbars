using System;
using ExpandedHotbars.Extras;

namespace ExpandedHotbars.Classes;

public readonly record struct PluginBarLayout(int Columns, int Rows, PluginFillDirection Direction) {
    public static PluginBarLayout Create(int count, int width, int height, PluginFillDirection direction) {
        width = Math.Clamp(width, 1, 50);
        height = Math.Clamp(height, 1, 50);
        count = Math.Max(count, 0);
        return direction switch {
            PluginFillDirection.TopToBottom or PluginFillDirection.BottomToTop =>
                new(Math.Max(1, (count + height - 1) / height), height, direction),
            _ => new(width, Math.Max(1, (count + width - 1) / width), direction),
        };
    }

    public (int Row, int Column) GetSlot(int index) => Direction switch {
        PluginFillDirection.RightToLeft => (index / Columns, Columns - 1 - index % Columns),
        PluginFillDirection.TopToBottom => (index % Rows, index / Rows),
        PluginFillDirection.BottomToTop => (Rows - 1 - index % Rows, index / Rows),
        _ => (index / Columns, index % Columns),
    };
}
