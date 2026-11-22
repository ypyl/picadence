# picadence

A tray icon for Windows that shows what the pi coding agents in your terminal
tabs are doing, and which one is waiting for you.

You run pi in a few tabs. When one finishes, or stops to ask you something, you
find out when you alt-tab back to it. picadence puts that on the edge of the
screen, so the answer is one glance away.

It can also wake a session for you on a cadence you pick, and bring a session
back into its own tab after you closed it.

![The picadence panel: three sessions, one working, one waiting for an answer, one
failed, with the age of each change and the two controls](docs/panel.png)

## What the icon means

| Colour | Meaning |
|---|---|
| red | a run ended in a failure pi did not retry |
| amber | a session is waiting for you to answer something |
| green | a session is working |
| grey | nothing needs you |

One icon covers every session, and the most urgent state wins: red over amber,
amber over green. A click on the icon opens a small dark panel with a summary, one
row per session (folder, state, what it is doing, how long ago it changed, and the
state's colour), and Quit. It closes when it loses focus or on Escape, and it
keeps up while it is open. A row does nothing: picadence never sends input to a
session.

## What the tab says

The pi half of picadence also writes to the terminal it runs in, so a tab says
what its session is doing even when the tray is not running:

```
  ● docs-site     working
  ▲ checkout      waiting for you
  ✖ infra         failed
  ✓ traylclaw     done
    notes         idle, and no mark at all
```

The mark goes before the session's name, or its folder when the session has no
name. It never replaces the label, so a tab is always identifiable.

Windows Terminal draws the same state as a ring on the tab and a progress bar on
the taskbar, through the ConEmu progress sequence. That is on automatically when
picadence recognises the terminal it runs in.

- `PICADENCE_TAB_PROGRESS=0` turns it off.
- `PICADENCE_TAB_PROGRESS=1` forces it on in a terminal picadence does not
  recognise.

Set it in the environment pi starts in, not inside pi. If you also turn on pi's
own `terminal.showTerminalProgress`, the two write to the same channel and take
turns; leave one of them off.

Two Terminal behaviours can override the title, and neither is ours to fix: a tab
you rename by hand stops following the title the process sets, and a shell prompt
that sets its own title can overwrite ours between turns.

## Start with Windows

Open the panel and click **Start with Windows**; it reads `on`, and Windows
starts picadence when you sign in. Click it again to stop. It writes one value under
your own user's `Run` key (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`),
needs no administrator, and if picadence later runs from another path it corrects
that entry itself.

## Wake a session on a cadence

Right-click a session row and pick a cadence: every 15 minutes, every hour, every
week, every month, every year, or off. The row then reads `every hour, next in
24m`.

![The Repeat ping menu open on a session row, listing the five cadences and
off](docs/repeat-ping.png)

When the time comes and the session is idle, picadence sends it one message. The
message names picadence, the time and the cadence, and asks the session to follow
its own standing instructions. It carries no instruction of its own: a session
meant to check the news every hour carries that instruction itself, in its
conversation or in a file it can read, and it is that session's business.

What follows from that:

- Only a session running in a tab is woken. picadence starts nothing, so a closed
  tab is never woken, and the windows it missed are counted.
- A session that is working, or waiting for your answer, is skipped rather than
  queued behind the run you started.
- A gap produces one wake-up, and the message says how many windows it missed.
- The cadence belongs to the session, not to the process. It survives a restart
  of picadence, a reboot, and a session you resume. Resume with `pi -c`, `pi
  --resume`, or `pi --session-id <id>`: a brand new session is a different
  session, so it is never woken.
- Turning a cadence off deletes it, and nothing else ever does. A cadence that
  fires once a year looks abandoned for the rest of it, and that is fine.

## Keep a session, and bring it back

Click the pin on a session's row, the small pin beside the age. A solid pin means
it is kept: the row stays when the tab closes, saying `not running`, and
right-clicking it offers **Restart**. Click the pin again to stop keeping it.

Restart opens one new Windows Terminal tab in that session's folder and continues
the same session, conversation included. Because it is the same session, the tab
mark, the row and any cadence you armed all come back with it.

The restart is offered only when it would work. The row says what is missing
instead: the conversation is gone, the folder is gone, or Windows Terminal is not
installed. A session that is running offers no restart, because two processes
writing one conversation is not something to leave to a misclick.

This is the one thing picadence starts. It opens `wt.exe` on one command, which
runs pi on that session's conversation, and nothing else, ever. On Windows pi is a
`.cmd` shim rather than a program, so the tab hands it to `cmd.exe` the way your
own shell does.

## Install the pi side

The tray is half the product. The other half runs inside pi and reports state.

```bash
pi install ./extensions/picadence.js
```

Install it once, globally, because it must report from every project you open pi
in. A session that was already running when you installed it reports nothing
until pi restarts.

State lands in `%LOCALAPPDATA%\picadence\agents`, one small file per session, and
is removed when pi exits. Nothing leaves the machine.

## Build and run

Needs the .NET 10 SDK and, for a release build, the MSVC linker for NativeAOT.

```bash
dotnet build picadence.sln -c Release
dotnet test picadence.sln -c Release && node --test extensions/picadence.test.mjs
dotnet run --project src/Picadence -c Release
```

A release build is one exe with no runtime dependency:

```bash
dotnet publish src/Picadence/Picadence.csproj -c Release
```

That lands in `src/Picadence/bin/Release/net10.0/win-x64/publish/Picadence.exe`,
about 1.8 MB, and it costs a few megabytes to run: with five sessions, around
12 MB of working set and 4 MB of private memory, drifting up slowly as garbage
accumulates and back down when the collector runs. It holds nothing it does not
need, because a session's state is parsed again only when pi rewrites it.

## What it is not

- Not an orchestrator. It cannot type into a session, press a key in one, or stop
  one. The only things it may start are a scheduled wake-up you armed, and one tab
  that resumes a session you kept, both while you watch and neither without you
  asking.
- Not cross-platform, and not a web app.
- No history, no analytics, no cloud, no account, no network call at all.
- It cannot tell you which tab a session is in: Windows Terminal exposes no such
  handle. It shows the working folder and the session name instead.

## License

MIT. Take it, change it, ship it; the terms are in [`LICENSE`](LICENSE).

## Where to start reading

- `AGENTS.md`: how work happens here. A human owns decisions; an agent writes
  the code.
- `adr/0001-native-tray-fed-by-pi-extension.md`: the stack, and why it is not a
  web view.
- `openspec/specs/agent-status/spec.md`: the behavior, in the words of what you
  can observe.
- `ARCHITECTURE.md`: the shape, in C4.
- `DESIGN.md`: what the panel looks like and why, with the tokens and the
  contrast floor.
