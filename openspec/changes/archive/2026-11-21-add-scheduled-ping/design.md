# Design

The decision is [ADR 0002](../../../adr/0002-the-tray-may-wake-a-session.md).
Behavior is in the delta specs. This file is the change-local approach: the file
contract, the arithmetic, and the two UI problems that need solving before
anything is typed.

## Context

Today the state directory is one-way: `agents/<pid>-<sid>.json`, written by
`extensions/piclaw.js`, read every 1000 ms by `Piclaw.exe`, deleted when the
process is gone (`SessionStore.ReadAll`). `SessionRef` rejects any session id
that is not a plain file stem, and `isPlainSessionId` does the same on the pi
side, so a session id is already a safe path segment in both languages.

Two facts constrain the design. The tray's only event loop is that timer, and the
panel hides itself on `WM_ACTIVATE`/`WA_INACTIVE`.

## Goals / Non-Goals

Goals: one schedule per session that outlives every process; a single one-way
door into a session; a way to arm and disarm from the panel; arithmetic that
cannot produce a burst after a long gap.

Non-goals: no new process, no writing to pi's input or its session storage, no
per-time-of-day ("at 09:00") scheduling, no ping history, no notification, no
second machine.

## Decisions

### Three lanes under one directory

```
%LOCALAPPDATA%\piclaw\
  agents\<pid>-<sid>.json     pi writes      tray reads, deletes when the process dies
  schedules\<sid>.json        tray writes    tray reads
  pings\<sid>.json            tray writes    pi reads and deletes
```

The schedule and the ping key on the session id alone. The state file keeps the
pid because liveness is a property of the process, and the pid is exactly what
changes when a session is resumed. Alternative rejected: keying schedules on
`<pid>-<sid>` with the state file, which is simpler but loses the schedule on
every restart, and that loss is the one thing this change exists to prevent.

Nothing is created until it is needed, so an untouched install gets no new
directories.

### The schedule record

```json
{"sessionId":"9f2c-...","name":"news-watch","folder":"triage",
 "every":"1h","nextAt":"2026-11-21T15:03:00Z",
 "lastIssuedAt":"2026-11-21T14:03:00Z","missed":0}
```

`every` is one of `15m`, `1h`, `1w`, `1mo`, `1y`. `name` and `folder` are a
human label for the file, refreshed whenever the session is seen; nothing reads
them. `lastIssuedAt` is named for what it is: the tray issues, it does not know
whether the session acted.

### Cadence arithmetic

`15m`, `1h`, `1w` are durations. `1mo` and `1y` are `AddMonths` and `AddYears`
on the stored UTC instant, so 31 January plus a month is 28 or 29 February
instead of 3 March. Everything is measured from the last fire, never from a wall
clock time, which is why there is no timezone parameter and no DST rule.
Alternative rejected: cron expressions, which need a local wall clock and a
policy for the hour that does not exist on one night a year, neither of which the
product asks for.

```text
due        nextAt <= now
fire       lastIssuedAt = now
           missed       += CountMissed(nextAt, now)
           nextAt        = Advance(now, every)        <- from now, never from nextAt
```

Advancing from `now` is what makes a burst impossible: a week offline produces
one wake-up and a count, never 168 queued turns.

`CountMissed` walks `Advance` from `nextAt` while the result is `<= now`, and the
result is capped. `shortcut: missed windows are counted up to 1000 and reported as "at least"; upgrade when someone needs an exact number for a gap longer than that.`

### The tray decides due, the session decides admissible

The tray reads the state file each tick and skips a window when the session is
`working` or `blocked`. The extension checks its own state again when it consumes
a ping, because the tray's view is up to a second stale and the user may have
typed in that second. A skipped ping is simply deleted.

The tray owns `nextAt`, `lastIssuedAt` and `missed`. The extension writes
nothing back. The cost is that a ping the extension skips is counted as issued;
that is one diagnostic number being slightly generous, and it is cheaper than a
fourth file or a second writer on the state file. One owner per fact.

### The ping is a sentence the tray composes

The ping file carries the composed text, not a template:

```json
{"at":"2026-11-21T14:03:00Z","every":"1h","missed":0,
 "prompt":"piclaw ping - 14:03, every hour\nAutomated wake-up from the piclaw tray app. Not a message from the user.\nFollow this session's standing instructions."}
```

The tray has the local time and the missed count; the extension has no business
formatting English. One owner for the words the agent reads. When `missed > 0`
the first line says `catch-up, at least 167 hourly windows missed`.

The extension sends it as a custom message, not a user message:

```js
pi.sendMessage({ customType: "piclaw-ping", content: ping.prompt, display: true },
               { triggerTurn: true });
```

A custom message starts a turn when the session is idle, reaches the model, and
stays visibly separate from what the user typed. `sendUserMessage` would put
words in the user's mouth in a transcript that outlives the session.

### The extension polls its own file

One `setTimeout` chain per session, started on `session_start` when the mode is
`tui`, every 10 seconds, cleared on `session_shutdown`. It reads
`pings/<sid>.json`, which it can name from the same id it already validates.

Alternatives rejected: `fs.watch`, whose rename semantics on Windows need a
debounce and a watcher per session; and a directory scan, which means every
session read every other session's file. `shortcut: a ping arrives up to 10 s after the tray writes it; upgrade if a cadence ever needs second precision.`

A ping older than five minutes is deleted unread, so a tray that dies between
writing and delivery cannot cause a wake-up an hour later at boot.

### The panel paints its own flyout

Right click on a row opens the cadence list. It is painted inside the panel
window, in the panel's own dark tokens, for three reasons: `TrackPopupMenu` takes
activation and the panel hides on `WM_ACTIVATE`/`WA_INACTIVE`; a native menu
would appear in the Windows light theme and break `DESIGN.md`; and the panel's
hit test, hover and invalidation already exist.

```text
+---------------------------------+
|  piclaw           2 working     |
|  (o) acme-api     working    12m|  <- right click
|  (o) news-watch   done       43m|     +-------------------+
|  (o) ops          waiting     2m|     |  every 15 minutes |
|                                 |     |  every hour    [*]|
|  [x] Start with Windows         |     |  every week       |
|  [ ] Quit                       |     |  every month      |
+---------------------------------+     |  every year       |
                                        |  off              |
                                        +-------------------+
```

Hit-test order: flyout, then rows, then controls. A click inside the flyout
chooses; a click anywhere else closes it and does nothing else, so the second
click of an accidental double click cannot arm something. Escape closes the
flyout first and the panel second. The flyout is clamped inside the panel, so it
can never cross the screen edge the panel itself already avoids.

An armed row's detail line reads `every hour - next in 24m`. It replaces the
detail rather than adding a line, because a row is 44 DIPs and the panel caps at
eight rows.

### One step added to the existing tick

```text
Refresh() every 1000 ms:
  read the state directory, delete what a dead process left
  read the schedule directory
  for each schedule whose session is live and due and idle: write a ping, advance the record
  for each schedule whose session is not live: count the missed windows, write nothing
  aggregate the icon, update the panel
```

The ping write happens after the prune, so the tray never writes into a session
it has just found dead.

## Risks / Trade-offs

| Risk | Mitigation |
|---|---|
| An unattended turn does something the user did not want | The cadence is armed by hand per session, it is visible on the row, and the ping asks for the session's own standing instruction. The tray grants no authority the agent did not already have. |
| Cost creeps up | The cadence is on the row, so 15 minutes looks as expensive as it is. No global schedule, no default. |
| A schedule survives to a session the user forgot | Never pruned by design, because a yearly one looks abandoned. `off` is one right-click away. |
| Two tabs resumed onto one session id both consume the ping | The first read deletes it; the other finds nothing. Harmless, and the alternative is a lock. |
| A tray killed mid-write leaves a half file | The extension parses defensively and deletes what it cannot read; a ping file is one `writeFileSync` of well under a page. |

## Migration Plan

None. Two new directories appear on first use. Rollback is a revert plus deleting
`schedules` and `pings`; no other component reads them and nothing in pi's own
storage is touched.

## Open Questions

None that change the shape. Whether the tray icon should reflect a ping that
ended in `error` is a follow-up: the panel already shows it, and the quiet
principle in `DESIGN.md` says the icon does not need to.
