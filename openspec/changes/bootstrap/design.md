# Design

Change-local approach for bootstrap. The stack decision and its rejected
alternatives are in `adr/0001-native-tray-fed-by-pi-extension.md`; this file
covers what bootstrap actually builds and the contract it fixes for the next
change.

## Scaffold

```
trayclaw.sln
src/Trayclaw/
  Trayclaw.csproj      net10.0, WinExe, win-x64, PublishAot, no package refs
  Program.cs           entry point, hidden window, tray icon, menu, message loop
  Native.cs            the Win32 surface, one file so the rest stays readable
  SessionRef.cs        encodes and decodes the state file name
tests/Trayclaw.Tests/
  Trayclaw.Tests.csproj   xunit, test-only dependencies
  SessionRefTests.cs
```

Two projects, because the executable is published with NativeAOT and a test
runner is not: keeping them apart stops the test SDK from being dragged into the
native image. The solution is the unit both `dotnet build` and `dotnet test`
take, so Project facts name one command each.

`Program.cs` creates a message-only window, adds one tray icon with a tooltip and
a right-click menu carrying Quit, and pumps messages. That is the whole of it:
no timer yet, no state, no files. The next change adds the poll and the menu
content; keeping the window, icon, and menu plumbing here means that change is
about behavior, not about Win32.

`SessionRef` exists because the file name is the contract between the extension
and the tray, and it is the one piece of the contract a machine can test today.
It maps `(pid, sessionId, cwd)` to `<pid>-<sanitized-session>.json` and back,
refusing anything that is not a plain file name, so a session id containing a
separator cannot escape the state directory.

## State contract (fixed here, built next)

- Directory: `%LOCALAPPDATA%\trayclaw\agents\`, created on first write, per user,
  no admin.
- One file per session, `<pid>-<sessionId>.json`. The session id is sanitized to
  `[A-Za-z0-9-]`; anything else is dropped, so a name cannot become a path.
- Fields: `pid`, `sessionId`, `cwd`, `name`, `model`, `state`, `detail`,
  `updatedAt`. `state` is one of `idle`, `working`, `blocked`, `done`, `error`.
- Written by the extension on every transition: temp file in the same directory,
  then rename over the target, so a reader never sees a half-written file.
- Deleted on `session_shutdown`. A crash leaves the file behind, and the tray
  treats a file whose `pid` is not a running process as gone, then deletes it.
- The tray never executes, evaluates, or renders anything from these files as
  markup. They hold state and labels only.

The states are not invented: they are the ones pi already reports to the
terminal over OSC 7501, driven by `session_start`, `agent_start`,
`agent_settled`, `ui_prompt_start`, and `ui_prompt_end`.

## Verification

`dotnet build` and `dotnet test` over the solution, `dotnet format
--verify-no-changes` as the lint, and `scripts/verify.sh` on top of the invariant
checks. The scaffold has to pass all of it before this change archives, because
an empty project that does not build is a trap left for the next change.

`dotnet publish` with AOT is not in the gate: it needs the MSVC linker, takes a
minute, and changes no behavior that the build and tests do not already show. It
is the release step, and the size numbers in the ADR were measured with it.

## Deferred

The extension, the poll, the tooltip and menu content, toasts, autostart, a
settings file, and the tab mapping that Windows Terminal does not offer.
