# Expanded Hotbars

This is a plugin for FFXIV/Dalamud that allows you to create and manage an unlimited number of additional hotbars built into the games native ui!

You can manage various show conditions to make certain bars only appear in certain places in the game, or when playing certain jobs, or many many other conditions!

## Fork development

This working copy is being prepared for independent development. The original
project is [MidoriKami/ExpandedHotbars](https://github.com/MidoriKami/ExpandedHotbars).
No destination repository or release feed has been selected yet. Upstream authorship
and the existing license are retained. Plugin behavior is unchanged at this stage.

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

The local `upstream` remote points to the original project. The working branch is
`codex/expanded-hotbars-features`. When a destination is chosen, add it as `origin`
and push the existing history rather than copying the source into a new repository.
Use merge commits for upstream updates to preserve shared ancestry and our changes.

The **Check upstream updates** workflow checks upstream `master` daily at 06:23 UTC
and can also be run manually. It merges updates into `codex/upstream-sync`, builds
the result, and opens or updates a pull request against the destination's default
branch. It never merges the pull request or publishes a release. A merge conflict
or build failure stops the run and needs manual resolution. Merge these update PRs
with a merge commit, rather than squash or rebase, to retain upstream ancestry.
The existing update branch is reused without force pushes; reserve it for updates.

To activate this after publishing the source:

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
original author's repository. Fork distribution, naming, and release automation
will be configured once a destination is selected.

### First planned change

Auto-close after using a hotbar slot is the first area to work through. Mouse and
keybind activation use different paths inside KamiToolKit, so both need checking.
Before implementation, decide whether closing follows an activation attempt or a
successful action, how the bar reopens, and whether the option applies per bar.
