# Design

## What is actually holding the terminal

Two processes, and neither is the tray.

1. `picadence` resolves to `%USERPROFILE%\.dotnet\tools\picadence.cmd`, because the
   CLI installs a native tool package as a batch file. That batch runs the exe.
2. cmd waits for the program a batch file starts, even a GUI one. The shell that
   typed `picadence` waits for cmd, because cmd is a console program.

The exe is GUI subsystem (`subsystem=2`), so it is not part of the problem: started
directly from PowerShell it returns the prompt in 36 ms.

Only the exit of the process the batch started ends the wait. So the fix cannot be
"hide the console": the tray has to be a different process from the one the batch
runs, and the one the batch runs has to exit at once.

## The decision

Three facts decide it, in this order:

| already a tray running | started by a waiting shell | this is the detached copy | what happens |
|---|---|---|---|
| yes | any | any | say so, exit 0 |
| no | yes | no | start the detached copy, exit 0 |
| no | yes | yes | run the tray here |
| no | no | any | run the tray here |

- **Already running** is first, so a second `picadence` says so in the terminal and
  does nothing else. It also means the launcher never fights the tray it finds.
- **Started by a waiting shell** is `cmd.exe` as the parent process. cmd is the only
  thing that waits for what it starts here. `dotnet run` has `dotnet.exe` as its
  parent, a shortcut has `explorer.exe`, and a shell typing the exe directly is
  `pwsh.exe` or `bash.exe`. Those all run the tray in place, which is what they do
  today, so a dev run keeps its Ctrl+C and a login entry keeps its one process.
- **The detached copy** carries `--tray` on its command line, and that marker is
  checked before anything else, so a bug in the parent lookup cannot become a
  process that keeps starting processes.

`Startup` holds that table as a pure function, which is what the tests read.

## Finding the parent

`GetProcessInformation` with `ProcessParentPid` is shorter, but that class is newer
than the oldest Windows this runs on and it answers a question the parent cannot
answer anyway: the parent may be gone by the time we look, and then the decision
silently falls back to "run in place" with no way to notice. One Toolhelp32 snapshot
(`CreateToolhelp32Snapshot` + `PROCESSENTRY32W`) gives the parent pid and, from the
parent's own entry, the base name of its image, with no second handle to open and no
extra access right. It runs once per start, in a few hundred microseconds.

A parent that cannot be found is treated as "not a waiting shell", which is today's
behavior.

## Starting the copy

`CreateProcessW` with the exe's own path and `--tray`:

- `CREATE_NO_WINDOW`, the same flag the tray already uses to start a terminal tab.
- `CREATE_BREAKAWAY_FROM_JOB`, because Windows Terminal associates a shell with a job
  object and a job with kill-on-close would otherwise take the tray down when the tab
  closes, which is the one thing this change is for. A job that does not allow
  breakaway makes that call fail with access denied, so it is retried without the
  flag rather than losing the start.
- If both attempts fail, the tray runs in the process that has the console. A start
  that cannot detach is still a start.

## One tray

A named mutex, `Local\PicadenceTray`, is created by the process that becomes the tray.
`Local\` is per session, which is what one person on one machine means here.

The launcher does not take it: it only asks whether it exists
(`Mutex.TryOpenExisting`). Taking it would race the copy it just started, and the
copy would find its own parent's mutex and quit. Two simultaneous launches both see
nothing, both start a copy, and exactly one copy wins the creation, because creation
is the only authority. The loser exits quietly.

## Saying it is already running

`Console.WriteLine` in the launcher, which has the console. A GUI-subsystem process
with no console writes to nothing, so the detached copy needs no special case: it
never prints, because it never reaches that branch with a console to print on.

## What is not done

- **Detaching always.** It would also make `dotnet run` return at once, which breaks
  the one place where a person wants the process they started to stay theirs. The
  parent check costs a few lines and keeps that.
- **A managed tool package**, so the CLI writes an exe shim instead of a batch. That
  is a different package with a runtime to install, for a flag we cannot reach
  anyway: the CLI decides the shim shape from the package, not from the exe.
- **A window that hides itself.** cmd waits for the process, not for its window.

## Proof

- `StartupTests` covers the table: four rows, four tests.
- `StartupProcessTests` writes the same batch shape the CLI writes, runs it through
  cmd, and fails if cmd is still running after the timeout. That is the reported bug,
  reproduced and then fixed.
- By hand, once, and recorded in `REPO-FACTS.md`: the prompt returns, the tray keeps
  running, and closing the terminal leaves the tray alive.
