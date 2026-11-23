# One row of controls, with a real checkbox (L0)

## Why

The foot of the panel spent two rows on two controls and put the state of the
first one in a word at the far right of it (`Start with Windows … off`), which
reads like a setting nobody can click. A checkbox says "this is a toggle" without
a sentence, and the two controls fit on one row.

## What Changes

- One row: a checkbox and its label on the left edge, `Quit` on the right edge.
- The checkbox shows `on` by a check mark inside its box, not by a word. Clicking
  it toggles, as before; the requirement that it shows its state is met by a shape
  rather than by a colour or a word.
- `Quit` is right-aligned in its own control, so the two never share a hit region.

## Capabilities

None. Where a control sits and how it draws its state is rendering, so
`skip_specs: true`. The requirement it leaves intact: **Starting with Windows is a
control, not a config file**.

## Impact

- `PanelModel` (one row, a box and a label per control), `Panel` (the check mark),
  tests.
- No file, no dependency, no new interop.

## Routing

0. **Does observable behavior change?** The layout moves and the state is drawn as
   a check mark instead of the word "on". No requirement moves.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes: two rectangles, their order, their
   alignment, and the checked flag are all model values with tests.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
