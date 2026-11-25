# A monthly ping keeps its day (L0)

## Why

A cadence of every month is advanced with `AddMonths(1)` from the previous fire. Arm
one on 31 January and it fires on 28 February, which is what the spec asks for. Then it
advances again from 28 February, so it fires on 28 March, 28 April, and the 31st never
comes back. A yearly cadence armed on 29 February loses the leap day the same way: 2029
gives 28 February, and 2032 continues from the 28th.

The spec says the opposite: "A monthly or yearly cadence SHALL keep its calendar day, so
a day that does not exist in the next month moves to that month's last day." The test
covers one hop from an unclamped day, so it passes while the promise is already broken
one month later.

## What Changes

- A schedule records the day of the month it was armed on, so a month that has to
  shorten the day is not a month that redefines the cadence.
- `CadenceExtensions.Advance` takes that day, clamps it to the month it lands in, and
  moves the way it always did for the other cadences.
- The record gains the field where the file is read and written; a file without it
  behaves exactly as today, so nothing on disk has to change.
- Three tests: the second hop past February, a leap day that comes back in 2032, and a
  due monthly record that advances from 28 February to 31 March.

## Capabilities

None. The behavior is the one the spec already states, so `skip_specs: true`.

## Impact

- `Cadence`, `Schedule`, `ScheduleFile`, `SchedulePlan`, `PingFile`, and
  `Panel.SetCadence`.
- The schedule file gains one optional number. An older file loses nothing.

## Routing

0. **Does observable behavior change?** Yes: after February a monthly cadence fires on
   its own day again. That is what the requirement already says, so no requirement moves.
1. **Can it be undone easily?** Yes, one commit. The field is additive and ignored by
   everything else.
2. **Can a machine prove it correct?** Yes: `Advance(28 February, 31)` is 31 March, and
   the tests fail before this change.
3. **Does anything outside the change depend on it?** No. The tray is the only reader and
   writer of the record, and the extension never sees it.

Level: **L0**.

## A limit this does not touch

The arithmetic runs on the value it is given, as it always has. A record read back from
disk carries the offset it was written in, so a session armed within an hour of midnight
can land on the neighbouring calendar day. That is older than this change and is left
where it is.
