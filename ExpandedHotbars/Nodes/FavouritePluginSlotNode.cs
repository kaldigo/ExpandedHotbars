using System;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ExpandedHotbars.Classes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using KamiToolKit.Extensions;

namespace ExpandedHotbars.Nodes;

public sealed class FavouritePluginSlotNode : ImGuiIconButtonNode {
    private static readonly Vector2 IconInset = new(2, 3);
    private readonly CancellationTokenSource lifetime = new();
    private ulong currentIcon;
    private volatile bool loading;
    private long nextIconCheck;
    private bool overrideLoaded;
    private bool overrideFailed;
    private bool fallbackLoaded;

    /// <summary>Null denotes the leading Dalamud launcher rather than an installed plugin.</summary>
    public required IExposedPlugin? Plugin { get; set; }
    private string InternalName => Plugin?.InternalName ?? "Dalamud";
    public Action? OnInvoked { get; init; }
    public bool AllowClicking { get; set; } = true;

    public unsafe FavouritePluginSlotNode() {
        ShowBackground = false;
        // The standard button animates its image at (8, 8), overriding Position.
        // Match native action icons at (2, 3) in every animation state instead.
        LoadThreePartTimelines(this, BackgroundNode, ImageNode, IconInset);
        RemoveEvent(AtkEventType.ButtonClick);
        AddEvent(AtkEventType.MouseClick, OnMouseClick);
    }

    protected override void OnSizeChanged() {
        base.OnSizeChanged();
        ImageNode.Size = Size - new Vector2(4, 4);
        ImageNode.Position = IconInset;
    }

    public void Refresh(FavouritePluginSource source) {
        var usable = Plugin is null || Plugin.IsLoaded && (PluginLauncher.HasMainAction(Plugin) || Plugin.HasConfigUi);
        Alpha = usable ? 1.0f : 0.45f;
        TextTooltip = Plugin is null ? "Dalamud\nLeft-click: Plugin list\nRight-click: Settings" : Plugin.Name + (usable
            ? "\nLeft-click: " + PluginLauncher.MainActionLabel(Plugin) +
              "\nRight-click: " + (Plugin.HasConfigUi ? "Settings" : PluginLauncher.MainActionLabel(Plugin))
            : Plugin.IsLoaded ? "\nThis plugin has no Open or Settings button." : "\nEnable this plugin in the Dalamud installer to open it.");

        if (loading || Environment.TickCount64 < nextIconCheck) return;
        nextIconCheck = Environment.TickCount64 + 1000;
        if (!overrideFailed && PluginIconOverrides.Find(ExpandedHotbars.PluginInterface.AssemblyLocation.DirectoryName!, InternalName) is { } file) {
            if (overrideLoaded) return;
            loading = true;
            _ = LoadRoundedIcon(null, file);
            return;
        }
        if (source.GetIcon(Plugin) is not { } texture) {
            if (!fallbackLoaded && PluginIconOverrides.FindFallback(ExpandedHotbars.PluginInterface.AssemblyLocation.DirectoryName!) is { } fallback) {
                loading = true;
                _ = LoadRoundedIcon(null, fallback, true);
            }
            return;
        }
        if (texture.Handle.Handle == currentIcon) return;
        loading = true;
        _ = LoadRoundedIcon(texture.CreateWrapSharingLowLevelResource());
    }

    private async Task LoadRoundedIcon(IDalamudTextureWrap? reference, string? overrideFile = null, bool isFallback = false) {
        var token = lifetime.Token;
        var sourceHandle = reference?.Handle.Handle ?? 0;
        IDalamudTextureWrap? rounded = null;
        try {
            reference ??= await ITextureProvider.Get().GetFromFile(overrideFile!).RentAsync(token);
            using (reference) {
                // RGBA8 at the game's high-resolution action icon size; no image library needed.
                var (specification, bytes) = await ITextureReadbackProvider.Get().GetRawImageAsync(reference,
                    new TextureModificationArgs { DxgiFormat = 28, NewWidth = 80, NewHeight = 80 }, true, token);
                if (!PluginIconVisibility.HasVisiblePixels(bytes, specification.Width, specification.Height, specification.Pitch)) {
                    if (overrideFile is not null) throw new InvalidDataException("Packaged plugin icon is transparent.");
                    currentIcon = sourceHandle;
                    if (PluginIconOverrides.FindFallback(ExpandedHotbars.PluginInterface.AssemblyLocation.DirectoryName!) is { } fallback) {
                        await LoadRoundedIcon(null, fallback, true);
                    }
                    return;
                }
                if (Plugin is null) RoundedPluginIcon.CompositeOnWhite(bytes, specification.Width, specification.Height, specification.Pitch);
                RoundedPluginIcon.Apply(bytes, specification.Width, specification.Height, specification.Pitch);
                rounded = ITextureProvider.Get().CreateFromRaw(specification, bytes, "Favourite plugin: " + InternalName);
                await IFramework.Get().Run(() => {
                    if (IsDisposed) return;
                    LoadTexture(rounded);
                    ImageNode.TextureSize = new Vector2(specification.Width, specification.Height);
                    rounded = null; // Ownership transferred to the native image node.
                    if (overrideFile is null) currentIcon = sourceHandle;
                    overrideLoaded = overrideFile is not null && !isFallback;
                    fallbackLoaded = isFallback;
                }, token);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception exception) {
            IPluginLog.Get().Warning(exception, "Could not prepare favourite plugin icon for {Plugin}", InternalName);
            // A damaged/missing override must not prevent the original icon loading.
            overrideFailed |= overrideFile is not null && !isFallback;
            nextIconCheck = overrideFile is not null && !isFallback ? 0 : Environment.TickCount64 + 30000;
            if (overrideFile is null) {
                currentIcon = sourceHandle;
                if (PluginIconOverrides.FindFallback(ExpandedHotbars.PluginInterface.AssemblyLocation.DirectoryName!) is { } fallback) {
                    await LoadRoundedIcon(null, fallback, true);
                }
            }
        }
        finally {
            rounded?.Dispose();
            loading = false;
        }
    }

    private unsafe void OnMouseClick(AtkEventListener* listener, AtkEventType eventType, int eventParam, AtkEvent* atkEvent, AtkEventData* data) {
        if (!AllowClicking || (!data->IsLeftClick && !data->IsRightClick)) return;
        try {
            var opened = Plugin is null
                ? PluginLauncher.OpenDalamud(ExpandedHotbars.PluginInterface, data->IsRightClick)
                : PluginLauncher.Open(Plugin, data->IsRightClick);
            if (opened && !IsDisposed) OnInvoked?.Invoke();
        }
        catch (Exception exception) {
            IPluginLog.Get().Error(exception, "Could not open favourite plugin {Plugin}", InternalName);
        }
    }

    protected override void Dispose(bool isNativeDestructor) {
        if (IsDisposed) return;
        lifetime.Cancel();
        base.Dispose(isNativeDestructor);
    }
}
