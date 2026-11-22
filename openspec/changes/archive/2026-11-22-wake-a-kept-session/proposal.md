# Wake a kept session from its cadence (L2)

## Why

A cadence only fires into a running tab, and a kept session can only be restarted
by a click. So a person who arms an hourly cadence, keeps the session, and closes
the tab at the end of the day has built a clock that does nothing until they come
back. The two halves exist; the tray just refuses to join them.

## What Changes

- A due cadence for a session that is not running but that the user kept opens one
  tab, the same way a click does, and delivers the single catch-up message there.
- A due cadence for a session the user did not keep still wakes nothing.
- A kept session that cannot be started is not attempted at all: the window counts
  as missed, the next is a full cadence away, and the row says what is missing.
- One tab per window at most, including the seconds before the new session has
  written its state.

Running sessions are untouched: their cadence wakes them in place, as today.

## Artifacts

- `adr/0005-a-cadence-may-start-a-kept-session.md`: the decision, its five
  constraints, and why this supersedes one clause of `0004`.
- `design.md`: how the clock and the launcher become one path, and the one-tab
  rule.
- `threat-model.md`: unattended work is the new thing here.

## Capabilities

### Modified Capabilities

- `scheduled-ping`: what a cadence may do for a session that is not running.

## Impact

- `SchedulePlan` grows the kept list and the start decision; `Program` performs it.
- No new file, no new dependency, no new interop.

## Routing

0. **Does observable behavior change?** Yes, and unattended: a tab can now open at
   3am because a cadence says so.
1. **Can it be undone easily?** The code, yes. A tab it already opened and the
   tokens that session spent are facts, not undoable states.
2. **Can a machine prove it correct?** The decision is a pure function over the
   session list, the kept list, the records, and the clock: each branch is a test.
   That the tab really opens was proven by hand when the click was built.
3. **Does anything outside the change depend on it?** An accepted ADR's constraint
   does, and it is superseded by `0005` in this change.

Level: **L2**, because a capability boundary moves: the tray may now start a
program without anyone in the room. Not L3: nothing is deleted, nothing published
is retracted, and no one else's data is touched.

## Approval

- Gate: L2 ADR
- Approved by: Yauhen Pyl
- Approved at: 2026-11-22
- Artifact: adr/0005-a-cadence-may-start-a-kept-session.md sha256:df286886206f2f28d564e0d7fb958c764a40f6d5df9044823bc75be39b79d6bd
- Artifact: openspec/changes/wake-a-kept-session/proposal.md sha256:3ce8883a370de5c584ffe622688de27c3e3cced9a5b40cc2abaaf4fc82e80645
- Words: "(none)"

## Approval

- Gate: L2 delta spec
- Approved by: Yauhen Pyl
- Approved at: 2026-11-22
- Artifact: openspec/changes/wake-a-kept-session/specs/scheduled-ping/spec.md sha256:eef8e70ce519ac88693d436c0f4ad6b9fcfbf78e8dd8c8e30e150d31d68f39f1
- Artifact: openspec/changes/wake-a-kept-session/proposal.md sha256:3ce8883a370de5c584ffe622688de27c3e3cced9a5b40cc2abaaf4fc82e80645
- Words: "(none)"
