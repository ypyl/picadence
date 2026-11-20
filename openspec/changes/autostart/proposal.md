# piclaw starts with Windows (L1)

## Why

Today the tray is a program you have to remember to start. A status indicator
that is only there when you thought to launch it is half a status indicator: the
first time you forget, you are back to alt-tabbing through pi tabs.

Windows already has the mechanism for this, per user, with no elevation: the
`Run` key under the current user's hive. What is missing is a way to turn it on
that does not involve regedit.

## What Changes

- The panel gains a second control: **Start with Windows**, with `on` or `off`
  next to it, and it looks and behaves like Quit does.
- The panel footer becomes two rows, one per control, so both are easy to hit.
- Turning it on registers the running executable with Windows for the current
  user. Turning it off removes that registration.
- If piclaw starts from a different path than the one registered, the
  registration is corrected, so moving the folder or rebuilding does not leave a
  dead entry behind.
- Nothing is elevated, and nothing is written outside the current user's hive.

## Capabilities

- **New**: none.
- **Modified**: `agent-status`. The panel requirement grows a control row, and a
  new requirement says what starting with Windows does.

## Impact

- `src/Piclaw/Autostart.cs`: the registry surface, the command string, and the
  follow-the-path rule.
- `src/Piclaw/PanelModel.cs`: a list of action rows instead of one Quit box, and
  a new hit kind.
- `src/Piclaw/Panel.cs`: paints and hit-tests both rows with the same hover.
- `src/Piclaw/Program.cs`: corrects the registration at startup, toggles it on
  click.
- `src/Piclaw/Native.cs`: the registry entry points, from `advapi32.dll`.
- Tests for the command string, the toggle, the follow rule, and the two rows.
- `README.md`, `CHANGELOG.md`, `CONVENTIONS.md`, and `ARCHITECTURE.md`.
- Not changed: the ADR. This adds a control, not a dependency: the registry is
  reached through the same Win32 surface as everything else.

## Routing

0. **Does observable behavior change?** Yes: a new control, and Windows starts
   the app at sign-in when it is on.
1. **Can it be undone easily?** Yes. Clicking the control off removes the
   registration; the app itself is unchanged either way.
2. **Can a machine prove it correct?** Yes: the command string, the toggle, the
   follow rule, and the row layout are pure functions with tests, and the
   registry write is one thin call over them.
3. **Does anything outside the change depend on it?** No document does. Windows
   reads the registration, and that is the point of the change.

Level L1: behavior moves, nothing outside holds a contract that moves with it.

## Out of scope

- A startup entry that survives the exe being deleted, uninstalled, or moved
  without ever running again.
- Delayed start, elevated start, or a scheduled task.
- Any setting other than this one. There is still no settings file.

## Approval

- Gate: L1 delta spec
- Approved by: Yauhen Pyl
- Approved at: 2026-11-20
- Artifact: openspec/changes/autostart/specs/agent-status/spec.md sha256:be7c816bd347678208315b23305f524eaa61664ce54fa0346c6802b5cf0569d1
- Artifact: openspec/changes/autostart/proposal.md sha256:0d77c091c32d28dbcdfd97fa3f87942199daea5cc064958872889ef277364d3d
- Words: "(none)"
