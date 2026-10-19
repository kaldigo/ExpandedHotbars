# Expanded Hotbars

This is a plugin for FFXIV/Dalamud that allows you to create and manage an unlimited number of additional hotbars built into the games native ui!

You can manage various show conditions to make certain bars only appear in certain places in the game, or when playing certain jobs, or many many other conditions!

## Kaldigo fork

This fork is maintained at [kaldigo/ExpandedHotbars](https://github.com/kaldigo/ExpandedHotbars),
based on [MidoriKami/ExpandedHotbars](https://github.com/MidoriKami/ExpandedHotbars).
Upstream authorship and the existing license are retained.

Add this custom repository in `/xlsettings` → Experimental → Custom Plugin Repositories:

```text
https://raw.githubusercontent.com/kaldigo/DalamudPlugins/main/pluginmaster.json
```

Then install ExpandedHotbars through `/xlplugins`. Disable the upstream plugin
before loading this fork; both use the same plugin identity and configuration.
The initial fork release is a development preview. In-game acceptance checks for
auto-close remain pending.

### Local setup

Install the .NET 10 SDK and Dalamud API 15 development references. The normal
XIVLauncher development directory is detected automatically; for a different
location, set `DALAMUD_HOME` to the directory containing `Dalamud.dll`.

```powershell
git submodule update --init --recursive
dotnet build ExpandedHotbars/ExpandedHotbars.csproj -c Release
```

The plugin output is in `ExpandedHotbars/bin/Release`. KamiToolKit stays pinned
to upstream's submodule commit. Keep feature changes in the plugin where possible
so dependency updates remain straightforward.

### Keeping up with upstream

The local `upstream` remote points to the original project; `origin` points to
`kaldigo/ExpandedHotbars`. The published default branch is `master`. Use merge
commits for upstream updates to preserve shared ancestry and our changes.

The **Check upstream updates** workflow checks upstream `master` daily at 06:23 UTC
and can also be run manually. It merges updates into `codex/upstream-sync`, builds
the result, and opens or updates a pull request against the destination's default
branch. It never merges the pull request or publishes a release. A merge conflict
or build failure stops the run and needs manual resolution. Merge these update PRs
with a merge commit, rather than squash or rebase, to retain upstream ancestry.
The existing update branch is reused without force pushes; reserve it for updates.

Repository setup for this workflow:

1. Put these workflows on the destination's default branch and enable GitHub Actions.
2. Under **Settings → Actions → General → Workflow permissions**, allow GitHub Actions
   to create pull requests. Repository or organization policies must also allow this.
3. Run **Check upstream updates** manually once to verify repository permissions.

The built-in repository token is used; no personal access token is required.
GitHub suppresses push workflows from that token; PR workflows may require approval,
so the sync workflow builds the merged source itself before pushing. The separate
**Build fork** workflow builds normal pushes and pull requests and stores development
artifacts. GitHub may delay scheduled runs or disable schedules in inactive public
repositories; manual runs remain available.

The inherited **Update Dalamud Plugin Repository** workflow is restricted to the
original author's repository. Pushing a four-part version tag such as `v1.0.1.1`
runs **Release fork**, builds and packages the plugin, publishes a preview release,
and updates this repository's standalone `pluginmaster.json`. The central
[DalamudPlugins](https://github.com/kaldigo/DalamudPlugins) feed refreshes hourly.

### Icon

The fork's icon matches the other Hotbar Tools icons: six hotbar slots and a plus
sign, with a green-to-magenta gradient and a rounded border on charcoal black.
The generated original and generation prompt are in `assets/icons/`; the 512x512
plugin export is `ExpandedHotbars/images/icon.png` and is copied into build output.
The plugin manifest points to this icon in the fork's published source.

### Close after use

Each hotbar's Config tab has a **Close After Use** checkbox, off by default.
When enabled, activating a populated slot by mouse click or keybind invokes its
action and then closes that hotbar. Activation attempts also close the bar when
the game rejects the action, for example because it is on cooldown. Empty slots
and dragging actions do not close the bar. Other hotbars are unaffected.

Closing clears the bar's **Enabled** setting and saves it. Reopen the bar using
`/exhotbar show <hotbar name>`, `/exhotbar toggle <hotbar name>`, or its **Enabled**
checkbox. Existing show conditions still apply when reopening. With **Allow Clicking
Actions** off, keybinds can still activate actions and close the bar.

The activation adapter is part of this plugin; KamiToolKit is unchanged.

In-game acceptance checks (pending):

- With the option off, actions leave the bar open by click and keybind.
- With it on, a populated slot runs its action and closes only its own bar by
  click and keybind, including an action currently on cooldown.
- Empty slots and drag-and-drop do not close the bar.
- Reopening with `show`, `toggle`, or **Enabled** allows another activation.
- Keybind activation still closes the bar with **Allow Clicking Actions** off.
- Extra features that hide native hotbars restore them when the bar closes.
- After reloading, the option and the bar's closed state are retained. Existing
  configurations without this option continue to leave bars open after use.
