# A kept row says when it wakes next (L0)

## Why

Arm a session with every hour and its row says `every hour, next in 24m`. Close
the tab and keep the session, and the row still says `not running` but loses that
line, even though the cadence is armed and is exactly what will open the tab.

The panel reads cadences from the tick's view, and the branch that plans a session
which is not running never writes one into it. Only sessions that are live put
their cadence there. So the one row whose cadence matters most, because nothing
else will tell the user when the tab comes back, is the one row that does not say.

The requirement it restores is **A session can be armed with a cadence**: "The row
SHALL show the armed cadence and how long until the next ping."

## What Changes

- `SchedulePlan.Plan` puts a cold session's record in the view as well, keyed by
  session id alone because no process holds it.
- `PlanCold` returns the record the row should read: the stored one when nothing
  happens, and the advanced one when the window fired or was refused.
- Two tests: the view carries a kept session's record, and its row shows the
  cadence and the time until the next ping.

## Capabilities

None. No requirement moves; one already-stated behavior is made true, so
`skip_specs: true`.

## Impact

- `SchedulePlan`, and the tests for it and for the panel.
- No file format changes: the record is read from the same schedule store.

## Routing

0. **Does observable behavior change?** Yes, and it is the change: a kept row now
   shows the cadence it holds. It is the behavior the spec already promises, so no
   requirement moves.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes. Before the fix, `tick.View` is empty for
   a kept session and `row.Schedule` is null; the two new tests fail. After it, both
   pass.
3. **Does anything outside the change depend on it?** No. The view is a per-tick
   value the panel reads and nothing writes to disk.

Level: **L0**.
