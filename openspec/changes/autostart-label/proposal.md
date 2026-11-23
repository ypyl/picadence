# The checkbox says Autostart (L0)

## Why

`Start with Windows` describes the mechanism. The checkbox is already labelled by
its own box and its position beside Quit, so the word it carries should be the
name of the setting, not a sentence about what Windows does.

## What Changes

- The checkbox label becomes `Autostart`, in the panel, the tests, and the three
  documents that repeat the old wording.

## Capabilities

None. Copy, so `skip_specs: true`. The requirement it leaves intact: **Starting
with Windows is a control, not a config file**.

## Impact

- `PanelModel`, two tests, `README.md`, `DESIGN.md`, `ARCHITECTURE.md`.

## Routing

0. **Does observable behavior change?** No: the control toggles the same registry
   value; only its word moves.
1. **Can it be undone easily?** Yes, one line.
2. **Can a machine prove it correct?** Yes: the tests assert the label.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
