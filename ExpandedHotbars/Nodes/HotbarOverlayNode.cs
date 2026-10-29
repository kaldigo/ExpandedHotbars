using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Plugin.Services;
using ExpandedHotbars.Classes;
using ExpandedHotbars.Configuration;
using ExpandedHotbars.Enums;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.BaseTypes;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using KamiToolKit.UiOverlay;
using KamiToolKit.Extensions;

namespace ExpandedHotbars.Nodes;

public sealed partial class HotbarOverlayNode : OverlayNode {
    public required HotbarConfig Config { get; init; }

    public override OverlayLayer OverlayLayer => OverlayLayer.BehindUserInterface;

    protected override unsafe void OnUpdate() {
        IsVisible = Config.ShouldShowHotbar() && Config.IsEnabled && (Config.AllowInPvP || !IClientState.Get().IsPvP);
        if (!IsVisible) return;

        foreach (var feature in Config.ExtraFeatures) {
            if (feature is { IsValid: true, Enabled: true }) {
                feature.Update();
            }
        }

        Scale = new Vector2(Config.Scale, Config.Scale);

        if (UpdateFavouritePluginsIfEnabled()) return;

        if (Config.UpdateFlags.HasFlag(ConfigChangedKind.NeedsRebuild)) {
            RebuildLayout();
            Config.UpdateFlags &= ~ConfigChangedKind.NeedsRebuild;
        }

        if (Config.UpdateFlags.HasFlag(ConfigChangedKind.NeedsUpdate)) {
            RecalcLayout();
            Config.UpdateFlags &= ~ConfigChangedKind.NeedsUpdate;
        }

        UpdateAnchor();
        EnableMoving = Config.IsMovingEnabled;

        IGameConfig.Get().UiConfig.TryGetBool("HotbarLock", out var isHotbarLocked);
        IGameConfig.Get().UiControl.TryGetBool("HotbarEmptyVisible", out var isHotbarEmptyVisible);

        padlockNode.IsVisible = Config.ShowPadlockButton;
        padlockNode.IsLocked = isHotbarLocked;

        ref var dragDropManager = ref AtkStage.Instance()->DragDropManager;
        var isDragging = dragDropManager is { IsDragging: true, MouseMoved: true };

        foreach (var node in hotbarNodes) {
            node.IsDraggable = !isHotbarLocked;
            node.IsBackgroundShown = isHotbarEmptyVisible || isDragging || System.ConfigWindow.IsOpen;

            node.Update();

            // A keybind can close the bar during this update. Do not process
            // any remaining slots on the now-closed bar.
            if (!Config.IsEnabled) return;
        }
    }

    public unsafe HotbarOverlayNode() {
        padlockNode = new PadlockButtonNode {
            NodeId = 1,
            Size = new Vector2(20.0f, 24.0f),
            TextTooltip = "Left Click to lock/unlock hotbar slots.\n" +
                          "Right Click to enable moving hotbar.\n" +
                          "Control + Click to open config.",
        };
        padlockNode.AddEvent(AtkEventType.MouseClick, OnPadlockButtonClick);
        padlockNode.RemoveEvent(AtkEventType.ButtonClick);
        padlockNode.AttachNode(this);

        OnMoveComplete = OnMoveCompleted;
    }

    private void RebuildLayout() {
        foreach (var node in hotbarNodes) {
            node.Dispose();
        }
        hotbarNodes.Clear();

        foreach (var row in Enumerable.Range(0, (int) Config.Size.Y)) {
            foreach (var column in Enumerable.Range(0, (int) Config.Size.X)) {
                var newHotbarNode = new HotbarSlotNode {
                    NodeId = (uint) (column + row * Config.Size.X) + 2,
                    Position = HotbarAnchor.SlotPosition(Config, row, column),
                    Size = new Vector2(44.0f, 44.0f),
                    IsClickable = true,
                    OnActionInvoked = OnActionInvoked,
                };

                hotbarNodes.Add(newHotbarNode);

                newHotbarNode.OnPayloadAccepted += (_, payload) => OnPayloadAccepted(row, column, payload);
                newHotbarNode.OnDiscard += _ => OnPayloadDiscard(row, column);

                if (Config.Actions.TryGetValue((row, column), out var actionInfo)) {
                    var payload = new DragDropPayload {
                        Int1 = actionInfo.ContainerId,
                        Int2 = (int)actionInfo.ActionId,
                        ReferenceIndex = actionInfo.ReferenceId,
                        Type = actionInfo.DragDropType,
                    };

                    newHotbarNode.SetSlot(payload);
                }
                else {
                    newHotbarNode.ClearSlot();
                }

                if (Config.Keybinds.TryGetValue((row, column), out var keybindInfo)) {
                    newHotbarNode.KeyBind = keybindInfo;
                }

                newHotbarNode.AttachNode(this);
            }
        }

        Size = Config.Size * (new Vector2(44.0f, 44.0f) + Config.Spacing) + new Vector2(16.0f, 16.0f);
        Origin = Size / 2.0f;
        padlockNode.Position = Size + new Vector2(8.0f, -42.0f);
    }

    private void RecalcLayout() {
        foreach (var row in Enumerable.Range(0, (int) Config.Size.Y)) {
            foreach (var column in Enumerable.Range(0, (int) Config.Size.X)) {
                var node = hotbarNodes[(int) (column + row * Config.Size.X)];

                node.Position = HotbarAnchor.SlotPosition(Config, row, column);

                if (Config.Keybinds.TryGetValue((row, column), out var keybindInfo)) {
                    node.KeyBind = keybindInfo;
                }
                else {
                    node.KeyBind = new KeySetting {
                        Key = SeVirtualKey.NO_KEY,
                        KeyModifier = KeyModifierFlag.None,
                    };
                }

                node.IsClickable = Config.EnableClicking;
            }
        }

        Size = Config.Size * (new Vector2(44.0f, 44.0f) + Config.Spacing) + new Vector2(16.0f, 16.0f);
        Origin = Size / 2.0f;
        padlockNode.Position = Size + new Vector2(8.0f, -42.0f);
    }

    private void OnPayloadAccepted(int row, int column, DragDropPayload payload) {
        Config.Actions[(row, column)] = new ActionInfo {
            DragDropType = payload.Type,
            ActionId = (uint) payload.Int2,
            ContainerId = payload.Int1,
            ReferenceId = payload.ReferenceIndex,
        };

        System.Config.Save();
    }

    private void OnPayloadDiscard(int row, int column) {
        Config.Actions.Remove((row, column));
        Update();

        System.Config.Save();
    }

    private void OnMoveCompleted(NodeBase _) {
        Config.Position = GetAnchorPosition();
        Config.HasCustomPosition = true;
        Update();

        System.Config.Save();
    }

    private unsafe void OnPadlockButtonClick(AtkEventListener* thisPtr, AtkEventType eventType, int eventParam, AtkEvent* atkEvent, AtkEventData* atkEventData) {
        if (atkEventData->IsControlHeld) {
            System.ConfigWindow.Toggle();
            return;
        }

        if (atkEventData->IsLeftClick) {
            if (!IGameConfig.Get().UiConfig.TryGetBool("HotbarLock", out var isHotbarLocked)) return;
            IGameConfig.Get().UiConfig.Set("HotbarLock", !isHotbarLocked);
        }
        else if (atkEventData->IsRightClick) {
            Config.IsMovingEnabled = !Config.IsMovingEnabled;
        }
    }

    private readonly List<HotbarNode> hotbarNodes = [];
    private readonly PadlockButtonNode padlockNode;
}
