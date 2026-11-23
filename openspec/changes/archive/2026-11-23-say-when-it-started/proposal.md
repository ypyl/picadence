# The state file says when its process started (L0)

## Why

The previous fix compared the state file's write time with the process's start time:
a process cannot write a file before it starts, so a later start means the pid was
reused. That is true, and it was not enough. The process holding pid 29240 reports a
start time *earlier* than the file, so the rule accepts it, and `folio` is still in
the panel two hours after the tab closed.

The only fact that actually identifies the writer is the writer's own start time, and
no amount of looking at the file from outside produces it. So the file carries it.

## What Changes

- The extension writes `startedAt`, derived from `process.uptime()`, on every state
  write. It is the same value every time that process writes.
- The tray compares it with the kernel's creation time for the pid, within two
  seconds, because two clocks wrote them.
- A file without `startedAt`, written by an older extension, falls back to the write
  time rule, so an update one way or the other never drops a live session.
- The liveness question moves after the parse, because the answer needs what the
  file says. The cache still means one stat per file per tick.

## Capabilities

None. The requirement it restores: **An ended session stops being shown**, so
`skip_specs: true`.

## Impact

- `extensions/picadence.js` and its tests, `StateFile`, `AgentSession`,
  `SessionStore`, `Program`, tests, one line in `REPO-FACTS.md`.

## Routing

0. **Does observable behavior change?** A stale row stops appearing. No requirement
   moves.
1. **Can it be undone easily?** Yes, one commit; the field is additive.
2. **Can a machine prove it correct?** Yes: the comparison is arithmetic on two
   timestamps, tested either side of the tolerance, and the file parse is tested.
3. **Does anything outside the change depend on it?** The state file is a contract
   with the extension, which writes it in the same commit; an old extension's files
   still read.

Level: **L0**.
