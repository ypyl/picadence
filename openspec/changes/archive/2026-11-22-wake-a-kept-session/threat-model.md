# Threat Model

The new surface is not a network call or a permission; it is **time**. The tray can
now act while nobody is in the room.

## What is worth protecting

| asset | why it matters |
|---|---|
| the person's attention and budget | an unattended session spends tokens |
| the person's machine | one program, `wt.exe`, on one command line |
| the conversations | a resumed tab can do whatever that session was told to do |
| the clock's honesty | a cadence that fires early or twice is a surprise |

## Who could act

The person, by pinning a session and arming a cadence. Nobody else: the pin files
live under the person's own `%LOCALAPPDATA%`, and there is no network surface.

## What changed about the risk

Before this, a process started only while someone was looking at the panel, so a
mistake was visible immediately. Now a mistake can run at 3am, which raises the
value of the guards and lowers the value of the person's eye.

## The guards, and what each one buys

| guard | what it prevents |
|---|---|
| a pin must exist | arming a cadence does not quietly mean "keep this alive" |
| a cadence must be armed | a pin does not quietly mean "run this hourly" |
| the plan's refusals | a broken pin cannot start a tab that cannot work |
| one start per thirty seconds | a race cannot open two tabs on one window |
| one catch-up message and a count | a week closed is one turn, not a hundred and sixty-eight |
| the same ping file | the delivery path is the one already tested, with its own staleness rule |

## What is accepted

- **Cost.** An hourly pin on a busy session is a real bill. It is the user's own
  cadence, and the message says it came from the clock rather than from them.
- **A conversation with no standing instruction** wakes, says nothing useful, and
  goes idle. That was true before; it is now true without anyone watching.
- **A machine that sleeps through a window** catches up once when it wakes, which
  is the existing gap rule and not new.

## What is refused

- Starting anything that is not a kept session with an armed cadence.
- Starting when the plan says it cannot work.
- Retrying a failed start every tick.
