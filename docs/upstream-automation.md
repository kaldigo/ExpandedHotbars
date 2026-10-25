# Upstream review and release boundary

The daily sync still proposes an ordinary merge, builds it, runs all configuration,
policy and package checks, and merges/releases only the exact validated commit.
No workflow approves a review, bypasses branch protection or force-pushes.

## Eligible for automatic review

- Cleanly merged README/docs changes, retaining fork documentation.
- Body-only updates to untouched condition implementations, with the same syntax,
  contract, saved-data and native-code checks as other source.
- An increase to the single numeric `Version` in the upstream plugin project,
  with every other project byte unchanged. SDK/dependency/build changes bundled
  with a version bump are not eligible.
- Body-only edits to existing ordinary C# methods outside protected code and data.
  This includes a different method in a file modified by the fork. Merely sharing
  a filename no longer causes rejection.

For C# review, the SDK's Roslyn parser compares the common ancestor, fork,
incoming upstream and exact proposed merge. Members added or changed by the fork
seed the protected set. Identifier references expand that set transitively through
helpers, fields and properties, including other files. Name matching is deliberately
conservative across types/overloads. Edits to those members, and changed methods
referencing them, remain manual. Comments/string contents cannot impersonate C#
declarations; directives and disabled branches are checked separately.

## Still manual

Conflicts, edits to protected members, persisted fields, properties, initializers,
signatures, constructors, type structure, imports, attributes, preprocessor
directives/disabled source, native/unsafe code or files importing native client structures/interop APIs,
reflection, dependencies/SDKs and
submodules. Build/publishing files, workflow/policy tooling, icons, plugin lifecycle
files, source additions/removals and unrecognised paths also remain manual.
The established condition category permits changes to an untouched condition's
ordinary `EvaluateCondition` override. Other protected references, native code,
imports, contracts and saved fields still remain manual. Conditions are not an
exception to syntax review, and new or removed condition types need review.

An unknown syntax shape, ambiguous member, missing parser or failed parse stops
automatic approval. Cleanly merged code is not automatically assumed compatible:
all existing build/regression/package gates are still mandatory. The analysis is
conservative static review, not proof of runtime behaviour; dynamic coupling and
in-game/native behaviour cannot be fully established by these tests.

## Reduced upstream overlap

The original plugin project is restored to upstream, including its Version.
`Directory.Build.props` selects the fork assembly identity early, and
`Directory.Build.targets` supplies fork branding/assets. Neither applies to the
tooling or dependency projects. Planned releases still pass their own `Version`;
upstream version changes cannot rename the installed plugin or select its update
version. Package checks assert the fork name, internal identity and repository.

Fork configuration fields and activation logic live in `HotbarConfig.Fork.cs`;
position/config UI and controller helpers live in partial files. Shared files retain
only the integration points that require access to upstream lifecycle/layout.
Saved field names, exported data and plugin behaviour are retained. Data converters,
configuration/serialization helpers and input/keybind helpers remain explicitly
manual because reflection and native dispatch can hide those dependencies from
source-name analysis.

The sync compiles the reviewer from the trusted default branch before merging
upstream and retains its compiled output and Python policy tools. Proposed changes
cannot replace their own reviewer. The PR report records accepted categories and
manual-review reasons; publication rechecks the same commit receipt and policy.

## Verification

`tests/test_upstream_policy.py` exercises real isolated Git histories and merges:
safe shared-file edits, direct/transitive feature edits, schema/signature changes,
version-only versus combined project edits, unavailable analysis, conflicts,
submodules, PR/branch changes and atomic concurrent-push rejection. Configuration
and package tests verify the isolated build still preserves features and identity.
The current upstream tip is already included; synthetic proposals are explicitly
rehearsals, never represented as a real new upstream release.
