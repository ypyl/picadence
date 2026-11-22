# Start a kept session from its row (L1)

## Why

`Restart` is the only reason to right-click a kept row, and it is the second time
this has happened: an action that belongs to a row lives in a menu. The pin moved
onto the row for the same reason. The start control should be beside it.

## What Changes

- A kept row that is not running and can be restarted carries a start control
  immediately left of its pin.
- The right-click menu loses `Restart`. A kept row then has no menu at all, and a
  running row's menu is the cadence list, which is what a menu is for here.
- A kept row whose start would not work shows no control, because a control that
  cannot act is furniture. The row already says what is missing.

## Capabilities

### Modified Capabilities

- `agent-status`: the row's controls, and what a click on the row does not do.

## Impact

- `PanelModel`, `Panel`, tests, the README's keep section.
- No file, no dependency, no new interop.

## Routing

0. **Does observable behavior change?** Yes: a control appears on a row that had
   none, and a menu item goes away.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes: the control's rectangle and what a hit
   there returns are model state, and the launch it triggers is the one already
   tested.
3. **Does anything outside the change depend on it?** No.

Level: **L1**.

## Approval

- Gate: L1 delta spec
- Approved by: Yauhen Pyl
- Approved at: 2026-11-22
- Artifact: openspec/changes/start-on-the-row/specs/agent-status/spec.md sha256:6abecbbe9aa7771d0daf3a491af1557862d9cd83db5691771eee7fef44581bb5
- Artifact: openspec/changes/start-on-the-row/proposal.md sha256:00402854d65c310295553b9af890086ce27d33a7593a417e369d281f75989eca
- Words: "(none)"
