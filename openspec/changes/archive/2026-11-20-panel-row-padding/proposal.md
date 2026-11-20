# The age gets a margin from the row edge (L0)

## Why

The age sits flush against the right edge of the row's grey box, so it reads as if
it were clipped by it. Every other element in the panel has 12 DIPs of breathing
room: the header, the footer, the row's own left side. The age is the one thing
touching a border.

## What Changes

- The row carries an inner padding of 12 DIPs on the right, and the age is drawn
  inside it, right-aligned.
- The age's box becomes part of the row model instead of arithmetic inside the
  paint call, so a test can check the margin rather than an eye.
- The text column shrinks by the same 12 DIPs.

No behavior named in a spec moves. The requirement this leaves intact is **The
panel is readable** and **The panel lists the live sessions**: same rows, same
information, same order.

## Capabilities

- **New**: none. `skip_specs` is set.
- **Modified**: none.

## Impact

- `src/Trayclaw/PanelModel.cs`: `RowPadding`, and `AgeBounds` on each row.
- `src/Trayclaw/Panel.cs`: draws the age into that box.
- One test on the margin.
- Changelog entry. No document changes.

## Routing

0. **Does observable behavior change?** No requirement moves. The gap is styling.
1. **Can it be undone easily?** Yes, one commit, nothing stored.
2. **Can a machine prove it correct?** Yes: the margin is a number in the row
   model, and the test reads it.
3. **Does anything outside the change depend on it?** No.

Level L0: styling, one commit, machine-checkable, nothing depends on it.
