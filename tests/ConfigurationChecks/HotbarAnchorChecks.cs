using System.Numerics;
using System.Text.Json;
using ExpandedHotbars.Classes;
using ExpandedHotbars.Configuration;

internal static class HotbarAnchorChecks {
    public static void Run() {
        var options = new JsonSerializerOptions { IncludeFields = true };
        var legacy = JsonSerializer.Deserialize<HotbarConfig>("""{"Size":{"X":3,"Y":2},"Position":{"X":1000,"Y":300}}""", options)!;
        Check(!legacy.AnchorRight && !legacy.AnchorBottom, "legacy alignment changed");
        Check(HotbarAnchor.Offset(Size(legacy), 2, false, false) == Vector2.Zero, "default native positioning changed");
        Near(HotbarAnchor.SlotPosition(legacy, 1, 2), new Vector2(96, 56), "legacy slot layout changed");

        foreach (var right in new[] { false, true }) {
            foreach (var bottom in new[] { false, true }) {
                var bar = new HotbarConfig { Size = new Vector2(3, 2), AnchorRight = right, AnchorBottom = bottom, Spacing = new Vector2(5, 7) };
                var positions = new HashSet<Vector2>();
                for (var row = 0; row < 2; row++) {
                    for (var column = 0; column < 3; column++) {
                        var position = HotbarAnchor.SlotPosition(bar, row, column);
                        Near(position, new Vector2(8 + (right ? 2 - column : column) * 49, 8 + (bottom ? 1 - row : row) * 51), "incorrect mirror mapping");
                        Check(positions.Add(position), "slots overlap after mirroring");
                    }
                }
            }
        }

        foreach (var scale in new[] { 0.5f, 1f, 1.5f, 3f }) {
            var bar = new HotbarConfig {
                Size = new Vector2(15, 2), Position = new Vector2(1000, 300),
                Scale = scale, Spacing = new Vector2(6, 8), AnchorRight = true, AnchorBottom = true,
            };
            var action = new ActionInfo { ActionId = 224 };
            var keybind = new KeybindInfo();
            bar.Actions[(0, 0)] = action;
            bar.Keybinds[(0, 0)] = keybind;
            var anchor = bar.Position;
            Near(ScreenPoint(bar, Size(bar)), anchor, "right/bottom edges miss X/Y at scale " + scale);
            var first = ScreenPoint(bar, HotbarAnchor.SlotPosition(bar, 0, 0));
            bar.Size = new Vector2(16, 3);
            Near(ScreenPoint(bar, Size(bar)), anchor, "resizing moved the anchor");
            Near(ScreenPoint(bar, HotbarAnchor.SlotPosition(bar, 0, 0)), first, "first slot moved relative to anchor when resized");
            Check(bar.Position == anchor, "alignment rewrote X/Y");
            Check(ReferenceEquals(bar.Actions[(0, 0)], action) && ReferenceEquals(bar.Keybinds[(0, 0)], keybind), "alignment rewrote logical actions/keybinds");
            var restored = JsonSerializer.Deserialize<HotbarConfig>(JsonSerializer.Serialize(bar, options), options)!;
            Check(restored.AnchorRight && restored.AnchorBottom && restored.Position == anchor && restored.Actions[(0, 0)].ActionId == 224 && restored.Keybinds.ContainsKey((0, 0)), "anchor export/import changed data");
        }

        var size = new Vector2(148, 64);
        var dragAnchor = new Vector2(1000, 300);
        var offset = HotbarAnchor.Offset(size, 1, true, false);
        var nativePosition = dragAnchor - offset;
        nativePosition += new Vector2(17, -9); // Dragging moves the native node.
        var next = HotbarAnchor.Offset(size, 1, true, false);
        nativePosition += offset - next;
        Near(nativePosition + next, dragAnchor + new Vector2(17, -9), "repeated layout reset dragging or saved the wrong anchor");
        nativePosition += next; // Disable alignment: offset becomes zero.
        Near(nativePosition, dragAnchor + new Vector2(17, -9), "disabling alignment failed to restore native positioning");
        Console.WriteLine("PASS: Anchor direction mirrors slots without rewriting actions/keybinds; X/Y, scaled edges, first-slot placement, dragging and export/import remain consistent");
    }

    private static Vector2 Size(HotbarConfig bar) => bar.Size * (new Vector2(44) + bar.Spacing) + new Vector2(16);
    private static Vector2 ScreenPoint(HotbarConfig bar, Vector2 local) {
        var size = Size(bar);
        var nativePosition = bar.Position - HotbarAnchor.Offset(size, bar.Scale, bar.AnchorRight, bar.AnchorBottom);
        return nativePosition + size / 2 + (local - size / 2) * bar.Scale;
    }
    private static void Near(Vector2 actual, Vector2 expected, string message) => Check(Vector2.Distance(actual, expected) < 0.001f, message);
    private static void Check(bool condition, string message) {
        if (!condition) throw new InvalidOperationException(message);
    }
}
