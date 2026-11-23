# Stars, and an age column that fits (L0)

## Why

Two things wrong on the same row, both visible at 125%:

- The pin glyphs are unreadable. `E840` at 12 pixels looks like a stray squiggle,
  not a pin, and the solid one is a blob. I rendered the candidates at the size the
  panel actually draws before choosing this time: a star survives, a pin does not.
- The age column was a fixed 44 dips wide and right-aligned, so a two-character age
  left roughly 48 pixels of nothing between its pin and its digits. The column is
  now measured from the widest age on screen, and every row shares that width so
  the controls still line up.

The kept row also stopped saying `kept 22h`: the phrase forced the widest column,
and the row already says `not running` under a solid star.

## What Changes

- The keep control is an outline star (`E734`) and a solid star (`E735`).
- The age column is `clamp(widest measured age, 24, 52)` instead of a fixed 44.
- A kept row's age is a plain age, like every other row's.

## Capabilities

None. Glyphs and widths are rendering, so `skip_specs: true`. The requirement it
leaves intact: **The panel lists the live sessions**, which asks for an age, not a
particular column width.

## Impact

- `PanelModel`, `Panel`, tests, `README.md`.
- The screenshots in `docs/` are now one change behind; they are refreshed once
  this look is confirmed rather than twice.

## Routing

0. **Does observable behavior change?** What the controls look like, and how wide
   the age column is. Nothing they do.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes: the column width is `clamp` over a
   measurement, the pin is one gap before it, and both are asserted.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
