# Visible missing icons and Stylist

Favourite Plugins now ships an opaque question-mark icon on a quiet slate field.
It appears when a plugin's installer icon is absent, still downloading, transparent
or unreadable. Available real icons still replace it. The existing rounded-corner
treatment is applied to both. Icon failures do not freeze the favourite list.

Stylist has a dedicated original wardrobe/gear icon on teal. Its primary click
dispatches the registered `/stylist` command through Dalamud's command manager;
right-click opens Stylist settings. The tooltip describes both actions. Disabled
Stylist cannot dispatch the command, and an unregistered command does not report
a successful activation or close the bar. If settings are unavailable, either
click uses the command, matching the existing single-action fallback convention.

The command exception is keyed only to the exact installed identity `Stylist`.
Other plugins retain their existing Open/Settings behaviour. Tests use an injected
command dispatcher and never execute gear-changing commands in the running game.

These changes stay in fork-owned Favourite Plugins files. Upstream integration
points, saved bar configuration, plugin identities, dependencies and upstream
approval rules are unchanged. Icons/prompts are in `images/plugin-overrides` and
`assets/icon-overrides/prompts.json`; the shared fallback is `images/plugin-fallback.png`.
