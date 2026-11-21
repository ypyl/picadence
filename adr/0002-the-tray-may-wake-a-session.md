# 0002. The tray may wake a session on a cadence

- Status: Accepted
- Date: 2026-11-21

## Context

piclaw reads and never drives. A requirement in `agent-status` says so: it never
sends input to a session, never starts or stops one, and never opens a network
connection. That was right while the product was a mirror of what pi is doing.

Some sessions hold a standing instruction that only a clock can act on: check
the news, poll the deploy, look at the queue. pi has no timer, so the session
waits for the user to type, and the user forgets.

The obvious routes do not fit. Windows Task Scheduler can start `pi -p`, but
that is a second process on the same session file, invisible in the tab the user
watches, and it cannot reach a session that is already running. Driving a session
over RPC owns a headless session instead of the tab. Synthetic keystrokes need
the terminal's window handle, and Windows Terminal exposes none, as
[0001](0001-native-tray-fed-by-pi-extension.md) already records.

pi does ship a supported door. An extension loaded into the session can start a
turn, and pi's documentation carries a file-trigger example written for exactly
this: external systems sending messages to the agent. The extension is already
installed in every session that the tray can see.

## Decision

The tray owns the clock. It keeps one schedule per session id in its own state
directory, and when a schedule is due it writes one small file that the
extension already running in that session consumes. The extension turns it into
a custom message with `triggerTurn: true`, so a turn starts and the transcript
records that the clock started it, not the user.

The ping carries no instruction. It names piclaw, the time and the cadence, and
tells the agent to follow the session's standing instruction. What to do stays
the session's business, in its own words, wherever the user put them.

A schedule belongs to a session id and to nothing else. It survives a tray
restart, a reboot and a resumed session, and it fires only into that session,
only while it is live in a tab. Nothing is started, so a schedule with no tab
open stays dormant and counts its missed windows.

The tray gains exactly one verb. It still writes no input stream, sends no
keystrokes, starts and stops no session, touches no file inside pi's own session
storage, and opens no network connection. Every write stays inside its own
directory.

The cadences are 15 minutes, 1 hour, 1 week, 1 month and 1 year, each measured
from the last fire, and one of them is off.

This is the change's L2 boundary and not L3: nothing is deleted, migrated or
published, a revert removes the feature, and the agent gains no authority it did
not already have. The tray decides the moment, not the permission. If a later
change lets a scheduled run act on something the user did not arm for, that
change is L3.

## Consequences

piclaw stops being only a mirror. The requirement that says it only observes is
replaced, and the README line that calls it "not an orchestrator" goes with it.
The next reader of the specs sees a product that observes and pokes, with the
limits written down.

A ping is a full turn with tools, not a heartbeat. An hourly tab costs 24 turns a
day and a 15-minute tab costs 96, each one a full context read. The cadence is a
spending decision, so the panel shows it on the row rather than hiding it behind
a submenu.

A schedule can outlive its usefulness: rename the session, or start a new one,
and the record never fires again. It is a few hundred bytes and it is never
pruned automatically, because a yearly cadence looks abandoned for 364 days.
Turning it off is the delete.

Catch-up is deliberately lossy. At most one ping follows any gap, so a week
offline produces one wake-up and a count, never 168.

The fail mode is quiet. A ping into a session with no standing instruction burns
one turn and does nothing useful, and the tray cannot detect it, because it does
not know what the session was told. That is the price of keeping the instruction
out of piclaw.

## Alternatives

- Windows Task Scheduler running `pi -p --session-id`: fires with no tab open,
  but it is a second writer on a live session file, it runs where nobody looks,
  and it cannot serve the tab the user is watching.
- RPC mode (`pi --mode rpc`): supported and controllable, but it owns a headless
  session rather than the open tab, and it adds a process lifecycle to the tray.
- Synthetic keystrokes into the terminal: no handle exists, and a keystroke that
  lands in the wrong window cannot be recalled.
- A scheduler inside pi: the right long-term home, but it is another product's
  decision and its shape is unknown today.
