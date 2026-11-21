# Threat model

Run before the design was fixed, for an L2 change that adds a store and a new
direction of authority. It is short because nothing crosses a boundary this
machine does not already have.

## What the change adds

- A store the tray writes: `schedules/`, one record per session in the user's own
  `%LOCALAPPDATA%`.
- A message lane the tray writes and pi reads: `pings/`.
- One new capability: the tray can start a turn in a session the user armed.

## Assets

| Asset | Why it matters |
|---|---|
| The agent's authority to run tools | A turn can edit files, run commands, and reach the network through the model. |
| The user's money | Every turn is billed. A cadence multiplies turns by time. |
| The user's attention | A wake-up is a session that becomes busy without the user touching it. |
| The state directory | The tray parses what it finds there, so a wrong file must not crash it. |

## Boundaries

No new trust boundary. Every file is in the current user's profile, written and
read by that user's processes. No elevation, no service, no port, no network
call, and nothing is read from another user's profile. Files that arrive from
outside the machine are not a case: nothing here downloads anything.

## Threats

| Threat | Verdict |
|---|---|
| Another process of the same user writes a `pings/` file and starts a turn | Accepted. A same-user process can already run `pi` itself; the tray grants no privilege it did not have. |
| A path escapes the directory through a session id | Covered. `SessionRef.IsPlainStem` and `isPlainSessionId` reject anything outside letters, digits and dashes, in both halves, before the value becomes a file name. |
| A malformed or enormous file hurts the tray | Covered. The state reader caps a file at 64 KB and ignores what it cannot parse; the schedule reader is written to the same rule. |
| Unbounded spend through a cadence | Mitigated by making each cadence a deliberate click, showing it on the row, and offering nothing faster than 15 minutes. There is no default schedule and no way to arm one in bulk. |
| An unattended turn performs a destructive action | Accepted, and recorded in [ADR 0002](../../../adr/0002-the-tray-may-wake-a-session.md). The session's standing instruction is the user's authorization; the tray only decides when. A scheduled run has exactly the permissions a typed prompt has. |
| A stale ping wakes a session long after its window | Covered. A ping older than five minutes is deleted unread. |
| The tray deletes something inside pi's session storage | Excluded by the design: the tray writes only its own directory and never removes anything it did not create. |
| Load: many armed sessions | Bounded by the panel's eight rows and by how many sessions a person runs. Each schedule costs one small file read per tick. |

## Result

No finding requires a design change. The two accepted risks, a same-user process
planting a ping and an unattended turn doing what its standing instruction says,
are consequences of the feature rather than defects in it, and both are recorded
in the ADR.
