# Quit looks like a button (L0)

## Why

Quit is drawn as plain text in the corner. Nothing on hover says it can be
clicked: no highlight, and the pointer stays an arrow. A person reading the panel
has to guess that the last line is a control, which is the one thing in the panel
that is clickable.

## What Changes

- Hovering Quit paints a rounded highlight behind it and turns the pointer into a
  hand. Leaving clears both.
- The label centres in its box instead of sitting against the right edge of the
  panel, so the highlight has a shape around the word.
- Two pure helpers make that checkable: which hit a point produces, and which
  pointer a hit asks for.

No requirement moves. **The panel is readable** and **the panel lists the live
sessions** stay as they are; this only makes the one control look like one.

## Capabilities

- **New**: none. `skip_specs` is set.
- **Modified**: none.

## Impact

- `src/Trayclaw/PanelTheme.cs`: a hover surface, added to the contrast pairs.
- `src/Trayclaw/PanelModel.cs`: `PanelCursor` mapping a hit to a pointer.
- `src/Trayclaw/Panel.cs`: mouse move, mouse leave, and the highlight paint.
- `src/Trayclaw/Native.cs`: cursor, mouse-leave tracking, rounded rectangle.
- Tests for the hover hit, the pointer mapping, and the new contrast pair.
- Changelog entry.

## Routing

0. **Does observable behavior change?** No requirement moves. It is styling with
   an interaction affordance.
1. **Can it be undone easily?** Yes, one commit, nothing stored.
2. **Can a machine prove it correct?** Yes: the hit test, the pointer mapping, and
   the contrast of the hover surface are all data.
3. **Does anything outside the change depend on it?** No.

Level L0: styling, one commit, machine-checkable, nothing depends on it.
