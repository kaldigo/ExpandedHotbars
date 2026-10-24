using System;
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
    private readonly CancellationTokenSource lifetime = new();
    private ulong currentIcon;
    private volatile bool loading;
    private long nextIconCheck;

    public required IExposedPlugin Plugin { get; set; }
    public Action? OnInvoked { get; init; }
    public bool AllowClicking { get; set; } = true;

    public unsafe FavouritePluginSlotNode() {
        ShowBackground = false;
        RemoveEvent(AtkEventType.ButtonClick);
        AddEvent(AtkEventType.MouseClick, OnMouseClick);
    }

    protected override void OnSizeChanged() {
        base.OnSizeChanged();
        ImageNode.Size = Size - new Vector2(4, 4);
        ImageNode.Position = new Vector2(2, 2);
    }

    public void Refresh(FavouritePluginSource source) {
        var usable = Plugin.IsLoaded && (Plugin.HasMainUi || Plugin.HasConfigUi);
        Alpha = usable ? 1.0f : 0.45f;
        TextTooltip = Plugin.Name + (usable
            ? "\nLeft-click: " + (Plugin.HasMainUi ? "Open" : "Settings") +
              "\nRight-click: " + (Plugin.HasConfigUi ? "Settings" : "Open")
            : Plugin.IsLoaded ? "\nThis plugin has no Open or Settings button." : "\nEnable this plugin in the Dalamud installer to open it.");

        if (loading || Environment.TickCount64 < nextIconCheck) return;
        nextIconCheck = Environment.TickCount64 + 1000;
        if (source.GetIcon(Plugin) is not { } texture || texture.Handle.Handle == currentIcon) return;
        loading = true;
        _ = LoadRoundedIcon(texture.CreateWrapSharingLowLevelResource());
    }

    private async Task LoadRoundedIcon(IDalamudTextureWrap reference) {
        var token = lifetime.Token;
        IDalamudTextureWrap? rounded = null;
        try {
            using (reference) {
                // RGBA8 at the game's high-resolution action icon size; no image library needed.
                var (specification, bytes) = await ITextureReadbackProvider.Get().GetRawImageAsync(reference,
                    new TextureModificationArgs { DxgiFormat = 28, NewWidth = 80, NewHeight = 80 }, true, token);
                RoundedPluginIcon.Apply(bytes, specification.Width, specification.Height, specification.Pitch);
                rounded = ITextureProvider.Get().CreateFromRaw(specification, bytes, "Favourite plugin: " + Plugin.InternalName);
                await IFramework.Get().Run(() => {
                    if (IsDisposed) return;
                    LoadTexture(rounded);
                    ImageNode.TextureSize = new Vector2(specification.Width, specification.Height);
                    rounded = null; // Ownership transferred to the native image node.
                    currentIcon = reference.Handle.Handle;
                }, token);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception exception) {
            IPluginLog.Get().Warning(exception, "Could not prepare favourite plugin icon for {Plugin}", Plugin.InternalName);
            nextIconCheck = Environment.TickCount64 + 30000;
        }
        finally {
            rounded?.Dispose();
            loading = false;
        }
    }

    private unsafe void OnMouseClick(AtkEventListener* listener, AtkEventType eventType, int eventParam, AtkEvent* atkEvent, AtkEventData* data) {
        if (!AllowClicking || (!data->IsLeftClick && !data->IsRightClick)) return;
        try {
            if (PluginLauncher.Open(Plugin, data->IsRightClick) && !IsDisposed) OnInvoked?.Invoke();
        }
        catch (Exception exception) {
            IPluginLog.Get().Error(exception, "Could not open favourite plugin {Plugin}", Plugin.InternalName);
        }
    }

    protected override void Dispose(bool isNativeDestructor) {
        if (IsDisposed) return;
        lifetime.Cancel();
        base.Dispose(isNativeDestructor);
    }
}
