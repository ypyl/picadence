# Buttons look like buttons, and the look is written down (L0)

## Why

The two controls are full-width rows whose label and value sit flush against the
hover fill, so a hovered control reads as a selected list item rather than a
button. Everything else in the panel has 12 DIPs of breathing room; the controls
have none.

There is also no document that says what the panel is supposed to look like. The
numbers live in `PanelModel` and the reasoning lived in one change's design note,
which is not where a person looks before changing a colour.

## What Changes

- A control becomes a real button: 32 DIPs tall, its hover fill inset 2 DIPs from
  the row, its label inset 12 inside the fill, its value ending 12 inside it.
- `DESIGN.md`: the visual language in one place, with the tokens, the layout, the
  states, the accessibility floor, and what the panel deliberately does not do.
- `README.md` and `CONVENTIONS.md` point at it, so the next edit starts there.

No requirement moves. The requirement this leaves intact is **The panel lists the
live sessions** and **Starting with Windows is a control**: same controls, same
order, same behavior.

## Capabilities

- **New**: none. `skip_specs` is set.
- **Modified**: none.

## Impact

- `src/Piclaw/PanelModel.cs`: the action rows carry their label and value boxes,
  and the height and padding constants.
- `src/Piclaw/Panel.cs`: draws into those boxes.
- `tests/Piclaw.Tests`: two tests, one on the insets and one on the values.
- `DESIGN.md`, `README.md`, `CONVENTIONS.md`, `CHANGELOG.md`.

## Routing

0. **Does observable behavior change?** No requirement moves. The controls keep
   their labels, their actions, and their hover.
1. **Can it be undone easily?** Yes, one commit, nothing stored.
2. **Can a machine prove it correct?** The padding is: it is a constant and a
   test. The prose is not, and prose is L0 by the same rule that makes any
   documentation L0.
3. **Does anything outside the change depend on it?** No.

Level L0: styling and documentation, one commit, machine-checkable where it
matters.
