# Hotbar anchor direction

The main configuration tab has two options below **Hotbar Size**:

- **Anchor at right edge (extend left)**: the right edge meets X, and ordinary
  slots run from right to left. A B C immediately becomes C B A.
- **Anchor at bottom edge (extend upward)**: the bottom edge meets Y, and ordinary
  rows run from bottom to top.

Enable either or both. X/Y stays unchanged when selecting an anchor: the bar
moves to the corresponding side of that point immediately. For a right-aligned
bar, increasing its width from 15 to 16 leaves the right edge and first slot in
place and adds the new space on the left. Scale and spacing are accounted for.
Dragging and editing X/Y update the anchor position.

Actions, keybinds and drag/drop use their original logical slot coordinates;
direction only changes their on-screen positions. There is no action-data
migration or rewriting when flipping a bar. The options are per bar and included
in exports; both default off, preserving existing positioning and slot order.

Favourite Plugins uses the same bar anchor but retains its own fill/wrap order
from Extras. Its generated dimensions also respect the selected anchor.

Most logic lives in new helper and partial files. Integration into existing code
is limited to saved options, a configuration UI call, slot positions, anchor
updates and position conversion when dragging or editing coordinates. No
dependency, publishing or automatic upstream approval policy is changed. Changes
from upstream touching fork-modified files still require manual review.
