# trayclaw

A tray icon for Windows that shows what the pi coding agents in your terminal
tabs are doing, and which one is waiting for you.

You run pi in a few tabs. When one finishes, or stops to ask you something, you
find out when you alt-tab back to it. trayclaw puts that on the edge of the
screen: one icon, one tooltip, one small menu.

**Status: scaffold.** The icon, its menu, and the exit path exist. No agent state
is shown yet; the state contract is designed in
`openspec/changes/bootstrap/design.md` and built in the next change.

## Who it is for

One person, on one Windows 11 machine, running pi in Windows Terminal. Not a
team tool, not a dashboard, not a service.

## What it is not

- Not an orchestrator. It never sends input to pi, and never starts or stops a
  tab.
- Not cross-platform, and not a web app.
- No history, no analytics, no cloud, no account, no network call at all.
- It cannot tell you which tab a session is in: Windows Terminal exposes no such
  handle. It shows the working folder and the session name instead.

## Build and run

Needs the .NET 10 SDK and, for a release build, the MSVC linker for NativeAOT.

```bash
dotnet build trayclaw.sln -c Release
dotnet test trayclaw.sln -c Release
dotnet run --project src/Trayclaw -c Release
```

The icon appears in the notification area. Right-click it for Quit.

A release build is one exe with no runtime dependency:

```bash
dotnet publish src/Trayclaw/Trayclaw.csproj -c Release
```

That lands in `src/Trayclaw/bin/Release/net10.0/win-x64/publish/Trayclaw.exe`,
about 900 KB, and runs at roughly 3 MB of private memory and 11 MB of working
set.

## Where to start reading

- `AGENTS.md`: how work happens here. A human owns decisions; an agent writes
  the code.
- `adr/0001-native-tray-fed-by-pi-extension.md`: the stack, and why it is not a
  web view.
- `openspec/changes/bootstrap/`: this change, its design, and its threat model.
- `ARCHITECTURE.md`: the shape, in C4.
