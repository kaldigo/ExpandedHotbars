using System;

namespace ExpandedHotbars.Classes;

public static class RoundedPluginIcon {
    /// <summary>Antialiased action-style corners on an RGBA8 image; preserves existing alpha.</summary>
    public static void Apply(byte[] pixels, int width, int height, int pitch) {
        var radius = Math.Min(width, height) * 0.12f;
        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                var dx = Math.Max(radius - (x + 0.5f), (x + 0.5f) - (width - radius));
                var dy = Math.Max(radius - (y + 0.5f), (y + 0.5f) - (height - radius));
                var distance = MathF.Sqrt(Math.Max(dx, 0) * Math.Max(dx, 0) + Math.Max(dy, 0) * Math.Max(dy, 0));
                var coverage = Math.Clamp(radius + 0.5f - distance, 0, 1);
                var alpha = y * pitch + x * 4 + 3;
                pixels[alpha] = (byte)(pixels[alpha] * coverage);
            }
        }
    }
}
