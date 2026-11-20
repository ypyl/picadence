# trayclaw

A tray icon for Windows that shows what the pi coding agents in your terminal
tabs are doing, and which one is waiting for you.

You run pi in a few tabs. When one finishes, or stops to ask you something, you
find out when you alt-tab back to it. trayclaw puts that on the edge of the
screen, so the answer is one glance away.

## What the icon means

| Colour | Meaning |
|---|---|
| red | a run ended in a failure pi did not retry |
| amber | a session is waiting for you to answer something |
| green | a session is working |
| grey | nothing needs you |

One icon covers every session, and the most urgent state wins: red over amber,
amber over green. The tooltip counts the sessions by state, and a right click
lists each one with its folder, what it is doing, and how long ago it changed.
Quit is the only thing a click can do; trayclaw never sends input to a session.

## Install the pi side

The tray is half the product. The other half runs inside pi and reports state.

```bash
pi install ./extensions/trayclaw.js
```

Install it once, globally, because it must report from every project you open pi
in. A session that was already running when you installed it reports nothing
until pi restarts.

State lands in `%LOCALAPPDATA%\trayclaw\agents`, one small file per session, and
is removed when pi exits. Nothing leaves the machine.

## Build and run

Needs the .NET 10 SDK and, for a release build, the MSVC linker for NativeAOT.

```bash
dotnet build trayclaw.sln -c Release
dotnet test trayclaw.sln -c Release && node --test extensions/trayclaw.test.mjs
dotnet run --project src/Trayclaw -c Release
```

A release build is one exe with no runtime dependency:

```bash
dotnet publish src/Trayclaw/Trayclaw.csproj -c Release
```

That lands in `src/Trayclaw/bin/Release/net10.0/win-x64/publish/Trayclaw.exe`,
about 1.5 MB, and runs at roughly 3.4 MB of private memory and 11 MB of working
set.

## What it is not

- Not an orchestrator. It never sends input to pi, and never starts or stops a
  tab.
- Not cross-platform, and not a web app.
- No history, no analytics, no cloud, no account, no network call at all.
- It cannot tell you which tab a session is in: Windows Terminal exposes no such
  handle. It shows the working folder and the session name instead.

## Where to start reading

- `AGENTS.md`: how work happens here. A human owns decisions; an agent writes
  the code.
- `adr/0001-native-tray-fed-by-pi-extension.md`: the stack, and why it is not a
  web view.
- `openspec/specs/agent-status/spec.md`: the behavior, in the words of what you
  can observe.
- `ARCHITECTURE.md`: the shape, in C4.
