# Favourite Plugins row and column order

In the hotbar's **Extras** tab, open **Favourite Plugins → Configure**.
The existing **Fill direction** controls the icons inside each row or column.
The additional checkbox controls the order of those rows or columns:

| Fill direction | Checkbox | Result when checked |
| --- | --- | --- |
| Left → Right or Right → Left | Add rows bottom to top | Start with the bottom row, then fill rows above it |
| Top → Bottom or Bottom → Top | Add columns right to left | Start with the rightmost column, then fill columns to its left |

For example, with a height of two slots, Top → Bottom and **Add columns right to
left** places the first two entries in the rightmost column, then the next two in
the column immediately to its left. Dalamud remains the first entry.

The checkbox is off by default, including for existing saved bars and imports.
It is saved per bar and included in exports. Sorting, configured width/height,
saved manual actions and ordinary hotbars are unaffected.

Implementation is confined to the fork's Favourite Plugins extra, its layout
helper and its overlay partial file. No upstream core layout, dependency or
publishing code is changed. Automatic upstream review remains conservative:
updates touching the fork's files still need manual review, while updates that
meet the existing routine policy can continue to merge and release automatically.
