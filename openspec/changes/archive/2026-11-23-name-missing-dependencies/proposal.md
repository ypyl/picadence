# Name a missing dependency in the panel (L1)

## Why

A machine without pi shows an empty panel that says "No sessions are running.
Start pi to see it here", which is a lie in the one case where the person needs
the truth: pi is not installed at all and starting it is not an option. A machine
without Windows Terminal shows sessions happily and then refuses every restart
with a note on a row, which is discovered one row at a time.

## What Changes

- The tray checks for pi and for Windows Terminal when it refreshes, and the
  panel says what is missing and what that costs.
- With pi missing and nothing live, the empty state names pi instead of claiming
  the machine is quiet.
- With Windows Terminal missing, the sessions are still listed and one line says
  a session cannot be started.
- The tray keeps running in both cases. It is a reporter first.

## Capabilities

### Modified Capabilities

- `agent-status`: what the panel says when it has nothing to show, and what it
  says about a missing dependency.

## Impact

- `Paths` already knows both answers; the panel gains a notice line and a
  dependency value. Tests cover both states without either dependency installed.
- No file, no dependency, no new interop.

## Routing

0. **Does observable behavior change?** Yes: the panel says something new.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes: the notice and the empty text are
   layout values produced from a dependency value.
3. **Does anything outside the change depend on it?** No.

Level: **L1**.

## Approval

- Gate: L1 delta spec
- Approved by: Yauhen Pyl
- Approved at: 2026-11-22
- Artifact: openspec/changes/name-missing-dependencies/specs/agent-status/spec.md sha256:a682998f9acf4736e9974dd8fb3d64828c58d04728833bb3f3426e6991fe25f1
- Artifact: openspec/changes/name-missing-dependencies/proposal.md sha256:806ed4d171dab3a5aa6b985eb6778165a7c51db3d8994deb35a3ac4b168ecde7
- Words: "(none)"
