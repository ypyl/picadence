# A restart names the path it cannot take (L0)

## Why

On Windows pi is a `.cmd` shim, so the tab the tray opens runs through `cmd.exe`, and
cmd expands `%NAME%` wherever it appears in that command line, quotes or not. A project
folder named `%USERPROFILE%` is legal on NTFS, and resuming a session in one would hand
pi a different path than the pin recorded.

The audit proposed escaping the percent sign. That does not work, and the probes are
below. There is no in-string escape for percent expansion on a cmd command line: `%%`
becomes `%` and is then expanded, and a caret only leaves a caret in the path.

```
cmd /d /c echo "C:\%USERPROFILE%\x.jsonl"    -> "C:\C:\Users\ypyl\x.jsonl"
cmd /d /c echo "C:\%%USERPROFILE%%\x.jsonl"  -> "C:\%C:\Users\ypyl%\x.jsonl"
cmd /d /c echo "C:\^%USERPROFILE^%\x.jsonl"  -> "C:\^%USERPROFILE^%\x.jsonl"
```

The real fix is to refuse the restart when a pinned path holds a percent sign. That
adds a fourth reason to "A restart is offered only when it can work", which is a
requirement change and belongs in its own gated change the first time such a folder
turns up. Until then the limit is written down where the command line is built.

## What Changes

- One `shortcut:` marker on `Restart.CommandLine` naming the case and the moment to
  revisit it. Nothing else in the source moves.

## Capabilities

None. No requirement moves, and no behavior changes, so `skip_specs: true`.

## Impact

- `Restart.cs`, one comment.
- `scripts/check-change.sh` reads the marker and fails it if it names no trigger.

## Routing

0. **Does observable behavior change?** No; a comment.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** The claim is prose, and the machine check that
   applies to it is the marker rule: a `shortcut:` with no upgrade trigger fails
   `scripts/check-change.sh`. The build and the 185 tests prove nothing else moved.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
