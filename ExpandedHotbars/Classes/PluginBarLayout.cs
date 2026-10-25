using System;
using ExpandedHotbars.Extras;

namespace ExpandedHotbars.Classes;

public readonly record struct PluginBarLayout(int Columns, int Rows, PluginFillDirection Direction, bool ReverseWrap = false) {
    public static PluginBarLayout Create(int count, int width, int height, PluginFillDirection direction, bool reverseWrap = false) {
        width = Math.Clamp(width, 1, 50);
        height = Math.Clamp(height, 1, 50);
        count = Math.Max(count, 0);
        return direction switch {
            PluginFillDirection.TopToBottom or PluginFillDirection.BottomToTop =>
                new(Math.Max(1, (count + height - 1) / height), height, direction, reverseWrap),
            _ => new(width, Math.Max(1, (count + width - 1) / width), direction, reverseWrap),
        };
    }

    public (int Row, int Column) GetSlot(int index) {
        var (row, column) = Direction switch {
            PluginFillDirection.RightToLeft => (index / Columns, Columns - 1 - index % Columns),
            PluginFillDirection.TopToBottom => (index % Rows, index / Rows),
            PluginFillDirection.BottomToTop => (Rows - 1 - index % Rows, index / Rows),
            _ => (index / Columns, index % Columns),
        };
        if (ReverseWrap) {
            if (Direction is PluginFillDirection.TopToBottom or PluginFillDirection.BottomToTop) {
                column = Columns - 1 - column;
            }
            else {
                row = Rows - 1 - row;
            }
        }
        return (row, column);
    }
}
