# A stale process id is not a session (L0)

## Why

A row for `folio` sat in the panel for eleven hours after that tab was gone. The
state file belonged to pid 29240, and that pid was still openable, so the tray
believed the session was alive.

It was not. Windows had given the number to a different process:

```
OpenProcess(0x1000, false, 29240)   -> a handle, no error
Get-Process -Id 29240               -> nothing: not a process we can enumerate
```

That is pid reuse, and "the id can be opened" was never a question about identity.
The panel promises that an ended session stops being shown, so this is a defect
that makes the tray do something its own spec already forbids.

## What Changes

- The session is checked against the process that holds its id: the process's start
  time has to be no later than the state file it supposedly wrote. A process cannot
  write a file before it starts, so a later start means the id was handed out again.
- The state file of a session that fails that check is removed, as it already was
  for an id that cannot be opened at all.
- Two seconds of slack, because a file can be stamped a moment before the kernel
  records the creation time in odd situations, and showing a row too long is the
  lesser fault.

## Capabilities

None. The requirement it restores: **An ended session stops being shown**, so
`skip_specs: true`.

## Impact

- `SessionStore` (the liveness question now carries the file's write time),
  `Program` (asks the kernel for the start time), `Native` (one call), tests.
- No new dependency. `GetProcessTimes` is the same handle the tray already opens.

## Routing

0. **Does observable behavior change?** A stale row stops appearing. No requirement
   moves; a requirement is being honoured.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes: whether a process could have written a
   file is arithmetic on two timestamps, and the store's decision is a test.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
