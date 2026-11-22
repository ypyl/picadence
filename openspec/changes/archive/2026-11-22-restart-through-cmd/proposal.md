# A restart starts pi the way a shell does (L0)

## Why

Pressing Restart opened the tab and then Windows Terminal reported:

```
[error 2147942402 (0x80070002) when launching `pi --session …`]
The system cannot find the file specified.
```

`0x80070002` is `ERROR_FILE_NOT_FOUND`, and the file it could not find is `pi`.
On Windows, pi is not an executable: the installer writes a `.cmd` shim, a `.ps1`,
and a POSIX script into `%USERPROFILE%\.pi\agent\bin`. `CreateProcess`, which is
what Windows Terminal uses, appends only `.exe` when it searches the path, so a
batch file is invisible to it. Typing `pi` in a shell works because the shell
resolves the shim and hands it to `cmd.exe`.

## What Changes

- The restart finds pi the way a shell does: `pi.exe` when one exists, otherwise
  the `.cmd` shim, in pi's own bin folder first and then on the path.
- A shim is started through `%ComSpec% /d /c`, with the payload quoted the way
  `cmd` unwraps it. A real `pi.exe`, when a future installer ships one, is started
  directly.
- A machine with no pi at all says `pi is not installed` on the row, instead of
  opening a tab that fails.

No behaviour moves: a kept session still restarts into its own folder with its
conversation, which is what the spec already says.

## Capabilities

None. The requirement it restores: **A kept session can be restarted**, so
`skip_specs: true`.

## Impact

- `Restart`, `Paths`, `Program`, tests.
- One extra process in the tab (`cmd.exe`), which is exactly what a shell puts
  there when a person types `pi`.

## Routing

0. **Does observable behavior change?** The restart starts working, and a machine
   without pi says so. No requirement moves.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes: the command line is a string, and the
   three shapes (shim, executable, nothing) are three tests. The tab itself was
   proven by hand, once, with that exact string.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
