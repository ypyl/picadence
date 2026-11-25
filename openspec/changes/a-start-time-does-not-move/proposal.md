# A start time does not move with the clock (L0)

## Why

The state file carries `startedAt`, when the writing process began, because the tray
uses it to tell a live session from a later process Windows gave the same pid. The
extension computes it as "the wall clock now, minus the process's uptime", and
recomputes it on every write.

Uptime is a monotonic counter; `Date.now()` is the wall clock. They only agree until
something steps the clock. If Windows time sync, a manual correction, or a resumed
machine moves the clock by more than two seconds, the number the extension reports
moves with it while the kernel's record of when the process started does not. The tray
compares the two within two seconds, decides the pid was reused, deletes the live
session's state file and drops its row, and the row stays gone until pi's next event
rewrites the file.

The requirement it restores is **An ended session stops being shown**: a session whose
process is still running must not be treated as gone.

## What Changes

- The extension computes `startedAt` once, when the session starts, and writes that
  value on every flush.
- A Node test drives the extension with a stepped clock and fails before this change:
  two writes either side of a one-minute correction report different start times.

## Capabilities

None. The behavior is the one already required, so `skip_specs: true`.

## Impact

- `extensions/picadence.js`: one captured value, and its reset on shutdown.
- `extensions/picadence.test.mjs`: one test.
- The file format does not change; a file written by an older extension still reads,
  because a missing `startedAt` falls back to the file's write time.

## Routing

0. **Does observable behavior change?** A running session no longer vanishes when the
   clock is corrected, which is what **An ended session stops being shown** already
   promises. No requirement moves.
1. **Can it be undone easily?** Yes, one commit; the extra field keeps its meaning.
2. **Can a machine prove it correct?** Yes. The test steps `Date.now` forward while the
   process keeps running and asserts the reported start is unchanged; before the fix
   it reports a minute later.
3. **Does anything outside the change depend on it?** No. The tray already accepts a
   file without the field.

Level: **L0**.
