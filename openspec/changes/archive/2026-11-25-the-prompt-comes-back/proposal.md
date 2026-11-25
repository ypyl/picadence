# The prompt comes back (L1)

## Why

Installed as a .NET tool, `picadence` is a batch file. The CLI writes one for a
native tool package, and all it does is run the exe:

```
@echo off
"%~dp0.store\ypyl.picadence\0.4.0\...\tools\any\win-x64\Picadence.exe" %*
```

The exe itself is GUI subsystem, so a shell that starts it directly gets its prompt
back in milliseconds and the tray keeps running. A command shell does not do that
inside a batch file: cmd waits for the program the batch started, and the shell then
waits for cmd. So `picadence` owns the tab for as long as the tray runs, and closing
the tab is what stops the tray.

Measured on this machine on 2026-11-25 with a copy of the 0.4.0 exe: started straight
from PowerShell the prompt returned after 36 ms with the tray alive; started through
a `.cmd` shim of the same shape, cmd was still running after 3081 ms. Nothing was
wrong with the tray: it was the two processes above it, waiting for each other.

## What Changes

- When the tray is started by a command shell that would wait for it, it starts one
  more copy of itself with no window and no console, and the first process exits, so
  the batch ends, cmd exits, and the shell is free while the tray keeps running.
- The detached tray does not belong to the terminal it came from, so closing that
  terminal, or the window it was in, leaves the tray alone.
- A second start adds no second icon: it says picadence is already running in the
  terminal it was started from, and exits.
- Every other way of starting it is unchanged. A shortcut, a login entry and
  `dotnet run` start the tray in the process that was started, because nothing is
  waiting for those and a dev run has to stay interruptible.

## Capabilities

Modified: `agent-status`. Two requirements are added, one for starting from a shell
and one for a single tray. No existing requirement moves.

## Impact

- `Program`: the start sequence, and a launch decision in a file of its own with its
  own tests.
- `Native`: one parent-process lookup.
- `README.md`: the paragraph that steers the reader away from the .NET tool route
  because of its console window is now wrong, and so is the one about the batch file
  in a shortcut.
- `CHANGELOG.md`: one entry.
- `REPO-FACTS.md`: how a command shell treats a batch file, with the measurement.

## Routing

0. **Does observable behavior change?** Yes. `picadence` gives the terminal back and
   stays running, and a second start says so instead of adding an icon. Both are new
   sentences in the spec.
1. **Can it be undone easily?** Yes. One commit, no data, no contract, and the exe's
   path and behavior toward sessions are untouched.
2. **Can a machine prove it correct?** Yes. The launch decision is a pure function
   over three facts, and the shell path is a process test that fails when the batch
   does not return. The proof that the tray outlives its terminal is by hand, and it
   is recorded in `REPO-FACTS.md` like every other one of those.
3. **Does anything outside the change depend on it?** No. The contract documents are
   `openspec/specs/`, the tool package and the state directory, and none of their
   shapes change.

Level: **L1**.

## Approval

- Gate: L1 delta spec
- Approved by: Yauhen Pyl
- Approved at: 2026-11-25
- Artifact: openspec/changes/the-prompt-comes-back/specs/agent-status/spec.md sha256:e96fef0b17248ca75294d67c4fe9290b785e3d5d2de15613bf390b42b636fbcf
- Artifact: openspec/changes/the-prompt-comes-back/proposal.md sha256:597da9ab0220070409d9ae1eaffa7a9624221d5585662be986b15bbbdaad93aa
- Words: "(none)"
