# The icon stops showing a tooltip (L1)

## Why

Hovering the icon pops a tooltip with the session counts. The panel already says
the same thing on the summary line, and clicking is the way in. The tooltip is a
second, smaller copy of the panel that appears when nobody asked, covers whatever
is behind it, and stays for as long as the pointer rests there.

## What Changes

- The tray icon carries no tooltip: hovering it shows nothing.
- The counts it used to show stay in the panel's summary line, which is where
  they were read from anyway.
- `AgentStatus.Tooltip` and the tests that proved it go away with the behavior.

No other behavior moves. The icon still shows the most urgent state and still
opens the panel on a click.

## Capabilities

- **New**: none.
- **Modified**: `agent-status`. One requirement is removed and one sentence in
  another stops naming the tooltip.

## Impact

- `src/Piclaw/Program.cs`: the icon flags lose the tip, and the text update goes.
- `src/Piclaw/AgentStatus.cs`: `Tooltip` is removed.
- `tests/Piclaw.Tests`: the tooltip tests go, and one assertion inside the quiet
  test reads only the aggregate state.
- `README.md`, `CHANGELOG.md`.
- Not changed: the state file, the panel, the extension.

## Routing

0. **Does observable behavior change?** Yes: a promise in the spec is withdrawn,
   which is why this is not a styling change.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes: the icon flags are a constant the
   code sets, and the removed text has no test left to pass. What a tooltip
   looks like is not machine-checked either before or after.
3. **Does anything outside the change depend on it?** No.

Level L1: a requirement is removed, so the human reads the delta before apply.

## Out of scope

- A shorter tooltip, a tooltip on the panel, or a tooltip that appears only when
  something is waiting. The panel header is the place for the counts.

## Approval

- Gate: L1 delta spec
- Approved by: Yauhen Pyl
- Approved at: 2026-11-21
- Artifact: openspec/changes/no-tooltip/specs/agent-status/spec.md sha256:f06052c0273998e20591e2ff81bd90736fd0631ee7b56a4feaf44e8735cb592c
- Artifact: openspec/changes/no-tooltip/proposal.md sha256:9006d3a8c859a8442a5d17e981a64d82b77bc395d7566f41587712a05861af10
- Words: "(none)"
