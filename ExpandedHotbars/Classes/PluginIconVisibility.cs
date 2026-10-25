namespace ExpandedHotbars.Classes;

public static class PluginIconVisibility {
    /// <summary>Ignore row padding and almost transparent pixels in RGBA8 readbacks.</summary>
    public static bool HasVisiblePixels(byte[] pixels, int width, int height, int pitch) {
        if (width <= 0 || height <= 0 || pitch < width * 4 || pixels.Length < (long)pitch * height) return false;
        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                if (pixels[y * pitch + x * 4 + 3] >= 32) return true;
            }
        }
        return false;
    }
}
