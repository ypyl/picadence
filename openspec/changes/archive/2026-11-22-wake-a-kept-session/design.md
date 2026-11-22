# Design

## One tick, two ways to deliver

`SchedulePlan.Plan` already decides, per record, whether the session is live and
free. It gains one question: when the session is not live, did the user keep it,
and can it be started?

```text
session live and idle      -> ping                         (today)
session live and busy      -> skipped, window missed       (today)
session not live, kept     -> start a tab, then ping       (new)
session not live, not kept -> skipped                      (today)
```

The plan stays pure. It is handed the kept list, which already carries each pin's
`RestartPlan`, so "can it be started" is a value, not a probe:

```csharp
public static ScheduleTick Plan(
    IReadOnlyList<AgentSession> sessions,
    IReadOnlyList<Schedule> schedules,
    IReadOnlyList<KeptSession> kept,
    DateTimeOffset now,
    DateTimeOffset? startedAt /* per session, in memory */)
```

A new `Ping` kind carries what the tick wants done:

```csharp
public enum Wake { Send, StartAndSend, Skip }

public sealed record Ping(Schedule Schedule, string Text, DateTimeOffset At, Wake Wake, Launch? Launch);
```

`Program` then performs it in one order that cannot half-happen:

1. Write the ping file. If that fails, stop: no tab, no advance, the window is
   retried on the next tick, which is the rule the ping file already has.
2. Start the tab, when the wake is `StartAndSend`.
3. Advance the record, with the missed count.

## The one-tab rule

Between step 2 and the new session writing its state file there is a window of a
second or two. A tick inside it sees a session that is still not live, a pin that
still says it can be started, and would start a second tab.

So the tray remembers, in memory, when it started each session, and the plan
skips a session it started less than thirty seconds ago. Memory is the right home:
a tray restart loses the note, and by then the tab has long since written its
state, so the worst case is a duplicate tab after a restart in the same second,
which nobody can type fast enough to cause.

## A start that cannot work

`Restart.View` already answers, with a sentence: `the conversation is gone`, `the
folder is gone`, `windows terminal is not installed`, `pi is not installed`. A
window due for a session with any of those is counted as missed and scheduled
forward one full cadence, which is the whole difference between a tray with a bug
and a tray that keeps trying.

## What the person sees

- A kept row says `1h, next in 12m` while it waits, as any armed row does.
- At the due time the row leaves the panel's kept section and appears as a live
  session, because a tab is now running it.
- The new tab's first turn is the catch-up message, which says how many windows
  passed while it was closed.

## Tests

| behaviour | test |
|---|---|
| a not-running kept session with a due window is started and pinged | `SchedulePlanTests` |
| a not-running session that is not kept is skipped | `SchedulePlanTests` |
| a kept session whose plan refuses is counted as missed, not started | `SchedulePlanTests` |
| a session started within thirty seconds is not started again | `SchedulePlanTests` |
| a live session's cadence is unchanged | existing tests |
| a missing conversation counts the window as missed | `SchedulePlanTests` |

The tab itself is not started in a test: `Program` passes the launcher in, and the
plan only says what to do.
