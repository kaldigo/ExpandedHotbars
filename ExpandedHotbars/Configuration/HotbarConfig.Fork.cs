namespace ExpandedHotbars.Configuration;

public partial class HotbarConfig {
    public bool AllowInPvP = false;
    public bool AnchorRight = false;
    public bool AnchorBottom = false;

    /// <summary>Distinguishes an explicitly saved (0, 0) from an unset legacy position.</summary>
    public bool HasCustomPosition = false;

    /// <summary>
    /// Closes this hotbar after a populated slot is activated by click or keybind.
    /// </summary>
    public bool CloseAfterUse = false;

    /// <summary>
    /// Closes this bar once when the optional activation behavior is enabled.
    /// </summary>
    public bool TryCloseAfterUse() {
        if (!CloseAfterUse || !IsEnabled) return false;

        IsEnabled = false;
        return true;
    }
}
