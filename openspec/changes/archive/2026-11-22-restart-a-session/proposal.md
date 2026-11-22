# Restart a session from the tray (L2)

## Why

Closing a tab throws away the session's place on the tray even though its
conversation is already on disk. Getting it back means opening a terminal in the
right folder and picking one file out of a directory of timestamps. The tray
already knows which session it was; it only refuses to act on what it knows.

The user asked for it in one sentence: keep a session on the tray, close the tab,
click once to get it back.

## What Changes

- A row can be kept. Kept rows survive the process, a tray restart and a reboot.
- A kept row that is not running offers a restart, which opens one Windows
  Terminal tab in that session's folder and resumes the session there.
- The restart is offered only when it can work, and the row names what is missing
  when it cannot.
- The product definition and the requirement that forbade starting a process both
  change to permit exactly this and nothing more.

Everything else stands: no input into a running session, no stopping, no writes
into pi's storage, no network.

## Artifacts

- `adr/0004-the-tray-may-restart-a-session.md`: the decision, its four
  constraints, and the alternatives.
- `design.md`: the pin lane, the transcript lookup, the validation, the one
  command line, and the panel's shape.
- `threat-model.md`: the tray becomes a process launcher, so the analysis is
  written down before the code exists.

## Capabilities

### Modified Capabilities

- `agent-status`: what the panel shows when a session ends, and what the tray is
  allowed to start.

## Impact

- New: a pin file per kept session under `%LOCALAPPDATA%\picadence`, and a launcher.
- Changed: `PanelModel`, `Panel`, `Program`, `AGENTS.md` (What this is),
  `README.md`, `CHANGELOG.md`.
- No new dependency. `CreateProcessW` is already reachable through the interop the
  tray uses.

## Routing

0. **Does observable behavior change?** Yes. A row appears that survived its
   session, and a click can start a process where nothing could before.
1. **Can it be undone easily?** The code, yes, in one commit. What it has done, no:
   a pin the user made and a tab it started are facts about their machine.
2. **Can a machine prove it correct?** The grid and the refusals yes, with tests on
   the pure command builder and on each refusal branch. That a tab really opens and
   resumes is a human check, recorded in the change.
3. **Does anything outside the change depend on it?** The product definition in
   `AGENTS.md` and the durable requirement that forbid it. Both are part of this
   change.

Level: **L2**, because a capability boundary moves: the tray goes from reading
files to running one program. Not L3: nothing is deleted, nothing published is
retracted, and no one else's data is touched.

## Approval

- Gate: L2 ADR
- Approved by: Yauhen Pyl
- Approved at: 2026-11-22
- Artifact: adr/0004-the-tray-may-restart-a-session.md sha256:6f317fed4143c8d52c5963aa64207fdf4e0887f7f2f82ccef568a790d23b7da4
- Artifact: openspec/changes/restart-a-session/proposal.md sha256:b303b9906352ed785100f249abfd3a33260dabc19f5e74b82b8bdf87732a7262
- Words: "(none)"

## Approval

- Gate: L2 delta spec
- Approved by: Yauhen Pyl
- Approved at: 2026-11-22
- Artifact: openspec/changes/restart-a-session/specs/agent-status/spec.md sha256:76b465635d99bbf5f027ee2326fb4019c49796e138f6fe84579b24280b2d1f56
- Artifact: openspec/changes/restart-a-session/proposal.md sha256:b303b9906352ed785100f249abfd3a33260dabc19f5e74b82b8bdf87732a7262
- Words: "(none)"
