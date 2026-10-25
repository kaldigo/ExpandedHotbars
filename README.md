# ExpandedHotbars Plus

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

Then search for **ExpandedHotbars Plus** in `/xlplugins`. This fork uses the distinct
internal identity `ExpandedHotbarsPlus`; Dalamud blocks custom repositories from
replacing the official `ExpandedHotbars` identity. Disable the upstream plugin
before enabling this fork, since their commands and hotbars overlap. Settings are
stored separately; use the existing hotbar export/import controls to transfer
layouts from upstream. The original configuration is preserved.
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
branch. It automatically merges routine updates after the build, configuration
regressions, merge-policy regressions and release-package validation pass. Successful
routine merges queue the release workflow automatically. The existing update branch is reused without force pushes;
reserve it for updates.

Automatic classification is deliberately conservative. Only condition implementation
files (excluding `ConditionBase.cs`) and Markdown under `docs/` or `README.md` can
qualify. Any file modified by the fork is excluded, even if it otherwise qualifies.
Hotbar configuration and activation code, dependencies and submodules, project and
lock files, publishing, workflows, icons, tests and policy tools require manual
review. Unrecognised paths, deleted condition types, native interop and references
to our activation hooks also require review. Upstream version/project changes
therefore remain manual even if their diff looks small.

Classification includes both the incoming upstream diff and the complete proposed
merge. Extra edits on the update branch cannot qualify as routine. The policy and
merge tools are copied from the trusted default branch before merging upstream.
Each PR explains the classification and links to the validation run. Conflicts
produce a separate review PR; build/test failures leave the proposal unmerged.

The workflow advances the default branch only to the exact tested merge commit,
using a regular fast-forward push. It checks both branch tips and the bot-created
PR's identity first; a concurrent default-branch update is rejected by Git as well.
There are no force pushes, automatic review approvals or branch-protection bypasses.
GitHub marks the PR merged when its head lands. If branch protection is added later,
it can block this push and the workflow will stop for review.

For manual update PRs, use a merge commit rather than squash or rebase to retain
upstream ancestry. Native click/keybind behavior still needs in-game testing;
the automated configuration regressions do not start the game or native UI.

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
runs **Release fork** manually; routine upstream merges dispatch it automatically.
It rebuilds the exact source, checks eligibility and regressions, allocates a newer
four-part DLL version, packages and validates assets, publishes a preview release,
and updates this repository's standalone `pluginmaster.json` only after assets are
public. The central
[DalamudPlugins](https://github.com/kaldigo/DalamudPlugins) feed refreshes hourly.

Publishing uses the shared version planner and publisher in DalamudPlugins. Draft
releases can resume after failures; published artifacts are immutable. Re-running
an already-published source repairs its source feed without allocating another
version. The daily upstream check also retries publication for its most recent
routine merge, including when no new upstream commits are available. Older source
and feed versions cannot replace a newer publication. Sensitive/manual merges do
not queue automatic releases. Development-preview status is preserved.

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

## Precise positioning

Select a hotbar in the configuration window and edit **Position X / Y** to move it
precisely. X increases to the right; Y increases downward. Changes apply immediately
and save when you finish editing. Mouse dragging remains available through
**Enable Moving**. An explicit position of (0, 0) is supported.

## Favourite plugin hotbars

Select a bar, open **Extras**, add **Favourite Plugins**, and enable it. Favourite
plugins in the Dalamud installer populate that bar automatically, grouped by install
repository and sorted alphabetically within each group. The default Dalamud
repository comes first; custom repositories follow alphabetically by their install
feed URL. Each actual repository is a separate group, including feeds from the same
publisher. Installed favourites retain their install origin for this grouping.
Dalamud itself is always the first slot in the selected fill direction: left-click
opens the main plugin list, and right-click opens Dalamud settings. Its logo uses
the same rounded icon treatment. It remains available even with no favourites.
The list refreshes about once per second while the bar is visible. Installed but
disabled favourites remain visible as dimmed icons; enable them in Dalamud to use
their buttons. Plugins with neither an Open nor Settings button are also dimmed.

Left-click invokes the installer's **Open** action; right-click invokes **Settings**.
If only one action exists, either click invokes it. The bar's **Allow Clicking
Actions**, **Close After Use**, scale, position and spacing still apply. Plugin
slots use the same icons as the installer, with rounded action-style corners.

Use **Configure** on the feature to choose its fill direction:

| Direction | Fixed dimension | Automatic growth |
| --- | --- | --- |
| Left -> Right | Configured width | Rows, filling each row from the left |
| Right -> Left | Configured width | Rows, filling each row from the right |
| Top -> Bottom | Configured height | Columns, filling each column from the top |
| Bottom -> Top | Configured height | Columns, filling each column from the bottom |

The automatic grid does not overwrite saved dimensions, actions or keybinds.
Disabling or removing the feature restores the ordinary bar. Use one Favourite
Plugins feature per bar; if duplicates are imported, the first enabled one wins.

Favourites and installer icons currently require a small compatibility adapter for
Dalamud's internal APIs. If those change, the feature reports that it is unavailable
and logs the problem. Plugin launching itself uses the public Dalamud API. In-game
acceptance testing of this feature remains pending; automated checks cover layouts,
click fallbacks, export/import compatibility and icon corner masking.

### Packaged hotbar icon overrides

The initial icon set covers Dalamud and 19 installed favourites. Each is a generated
reinterpretation of its original icon using native utility/menu icons as the visual
style reference, with simple raised symbols and quiet individual background colours.
Dalamud uses a white background. A common rounded-corner mask is applied
in game, including to the original installer icons used as fallbacks.

Overrides are isolated to these hotbar slots. They do not change plugin identities
or the icons shown in Dalamud's installer. Missing, unsupported or unreadable
overrides fall back to the original installer icon automatically.

To extend the set, add `ExpandedHotbars/images/plugin-overrides/<InternalName>.png`,
using the exact Dalamud internal plugin name, then update that directory's
`manifest.json` with its source reference and SHA-256. Keep matching source credits
in `CREDITS.md`. PNG artwork should be square, with the intended background already
painted in. The plugin scales it to an 80-pixel texture and applies the hotbar mask;
avoid adding a second rounded frame or transparent outside margins. Reload the
plugin after changing a packaged image.

The [icon review sheet](assets/icon-overrides/preview.png) includes 40-pixel previews.
Generation prompts are saved in `assets/icon-overrides/prompts.json`. These images
were created with the built-in image generator using the original plugin icons and
a style reference sheet extracted locally from the game; the game reference sheet
is not redistributed in this repository.
