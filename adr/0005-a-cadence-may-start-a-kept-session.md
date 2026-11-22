# 0005. The tray may start a kept session when its cadence fires

- Status: Accepted
- Date: 2026-11-22

## Context

Two pieces arrived one after the other. A cadence wakes a session on a schedule,
but only a session that is already running: "A cadence whose session is not
running SHALL wake nothing". A kept session can now be restarted from the tray,
but only by a click, because `0004` decided that "nothing starts without a click".

Together they leave a hole that is visible from the outside. A person arms an
hourly cadence on a session, keeps it, closes the tab at the end of the day, and
the cadence does nothing until they open it again. The clock is running, the
intent is recorded, and the one action the tray now knows how to take is the one
it refuses to take by itself.

## Decision

When an armed cadence falls due for a session that is not running but that the
user kept, the tray will open one terminal tab for it, as a click would, and
deliver the single catch-up message there.

Five constraints, part of the decision:

1. **Both halves must be the user's.** The session must carry a pin the user made
   and a cadence the user armed. Neither alone starts anything.
2. **One tab per due window, at most.** The tray records when it started a session
   and starts nothing again for that session for thirty seconds, so a tick cannot
   start a second tab before the first one has written its state.
3. **A start that cannot work is not attempted.** The same refusals as a click
   apply: no conversation, no folder, no Windows Terminal, no pi. The window counts
   as missed, the next one is scheduled normally, and the row already says why.
4. **The message is the same message.** The resumed session reads the ping file the
   tray writes before it starts it, so the wake-up path is the one that already
   exists, including its "automated wake-up from the tray" wording.
5. **A click is still the only way to start a session by hand.** Nothing else in
   the tray starts anything.

This supersedes `0004` on one point, its constraint that nothing starts without a
click. Everything else in `0004` stands.

## Consequences

- **Unattended work is now possible, and that is the point.** A yearly cadence on a
  kept session could start a tab at three in the morning and spend tokens while
  nobody watches. The guard is consent: a pin the user made and a cadence the user
  armed, both of which take a deliberate click.
- The tray's clock and its launcher become one path instead of two, so the
  refusals, the quoting, and the found-pi problem have one owner.
- A session that is already running is untouched: its cadence wakes it in place,
  exactly as before.
- A kept session whose conversation is gone is scheduled forward and counted as
  missed, rather than retried every second. The row is where the reason lives.
- The gap rule gets simpler to see: a session closed for a day has one overdue
  window; that is the one that opens the tab and carries the missed count.

## Alternatives

- **Leave it as it is and let the person open the tab.** Rejected: the case that
  motivates a cadence is not being at the machine, and a cadence that only works
  while the tab is open is a cadence for people who are already there.
- **Start any session with an armed cadence, pinned or not.** Rejected: a pin is
  how a person says "I want this one back". Without it, arming a cadence would
  quietly mean "keep this session alive", which nobody asked for.
- **Start the session and wait for it before writing the ping.** Rejected: it makes
  the tray wait on a program it does not own, and the ping file is already the
  door the extension opens on its own first poll.
- **Retry a failed start every tick.** Rejected: a folder that is gone stays gone,
  and a tray that tries once a second is a tray with a bug.
