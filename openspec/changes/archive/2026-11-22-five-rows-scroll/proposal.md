# Five rows at a time, and a wheel to see the rest (L0)

## Why

The panel grew to eight rows and then said `+4 more` about the ones that did not
fit, which is a polite way of saying they are unreachable. With twelve sessions
open, four of them exist only as arithmetic.

## What Changes

- The window shows five rows. The list keeps every session and every kept row, in
  the same order, and the wheel moves it one row per notch.
- One line under the list says where the window is: `1–5 of 12 · scroll`. When
  everything fits, the line is absent and the wheel does nothing.
- A row that is scrolled out of the window has no context menu, because a row that
  is not on screen cannot be right-clicked.

## Capabilities

None. How many rows fit on a screen is rendering, and the requirement that the
panel lists every live session is now better kept rather than worse, so
`skip_specs: true`. The requirement it leaves intact: **The panel lists the live
sessions**.

## Impact

- `PanelModel` (window and legend), `Panel` (wheel, scroll kept across ticks),
  `Native` (one message constant), tests.
- No file, no dependency, no new interop.

## Routing

0. **Does observable behavior change?** The panel shows five rows instead of eight
   and can be scrolled, but no requirement moves.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes: which rows a scroll position yields, and
   where it clamps, are functions with tests.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
