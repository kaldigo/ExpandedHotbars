using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using ExpandedHotbars.Configuration;
using ExpandedHotbars.Nodes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.UiOverlay;

namespace ExpandedHotbars.Classes;

public sealed partial class HotbarController : IAsyncDisposable {
    private readonly OverlayController overlayController;

    private readonly Dictionary<HotbarConfig, HotbarOverlayNode> hotbarNodes = [];

    public HotbarController() {
        ThreadSafety.AssertMainThread();

        overlayController = new OverlayController();

        foreach (var hotbarConfig in System.Config.Hotbars) {
            AddHotbar(hotbarConfig);

            foreach (var feature in hotbarConfig.ExtraFeatures) {
                feature.OnEnable();
            }
        }
    }

    public async ValueTask DisposeAsync() {
        await IFramework.Get().Run(() => {
            foreach (var config in hotbarNodes.Keys) {
                foreach (var feature in config.ExtraFeatures) {
                    feature.OnDisable();
                }
            }

            overlayController.Dispose();
        });
    }

    public unsafe void AddHotbar(HotbarConfig hotbarConfig) {
        ThreadSafety.AssertMainThread();

        var position = hotbarConfig.Position;

        // Preserve legacy centering while allowing an explicitly saved (0, 0).
        if (!hotbarConfig.HasCustomPosition && hotbarConfig.Position == Vector2.Zero) {
            position = (Vector2)AtkStage.Instance()->ScreenSize / 2.0f;
        }

        hotbarConfig.Position = position;

        var newHotbarNode = new HotbarOverlayNode {
            NodeId = hotbarConfig.GetNameHash(),
            Position = position,
            Config = hotbarConfig,
        };

        overlayController.AddNode(newHotbarNode);
        hotbarNodes[hotbarConfig] = newHotbarNode;
    }

    public void RemoveHotbar(HotbarConfig hotbarConfig) {
        ThreadSafety.AssertMainThread();

        overlayController.RemoveNode(hotbarNodes[hotbarConfig]);
        hotbarNodes.Remove(hotbarConfig);
    }
}
