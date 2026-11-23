# The controls are as wide as their contents (L0)

## Why

Both controls were given fixed widths chosen by eye: 168 for the checkbox and 72
for `Quit`. The words need about a third of that, so the hover fill was mostly
empty space and the panel's foot looked like two fields waiting for input.

## What Changes

- The checkbox is as wide as its box, its label, and the inset around them, with
  the label measured in the panel's own font rather than estimated.
- `Quit` becomes an X: one glyph, 36 wide, at the trailing edge. The spec asks for
  a way to quit and an X is one, so no requirement moves.
- A glyph, a check mark, and a label are all one value on the control, so the
  painter draws what the model says rather than deciding from the action name.

## Capabilities

None. Widths and glyphs are rendering, so `skip_specs: true`. The requirement it
leaves intact: **Starting with Windows is a control, not a config file**.

## Impact

- `PanelModel` (measured widths), `Panel` (one GDI measurement, cached per string
  and scale), `Native` (one call), tests, `DESIGN.md`, `README.md`.
- No file, no dependency, no new interop that reaches outside the window.

## Routing

0. **Does observable behavior change?** The controls are narrower and one is an X.
   Nothing they do changes.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes: widths are arithmetic over a
   measurement, and the tests pass their own measurement to prove the arithmetic.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
