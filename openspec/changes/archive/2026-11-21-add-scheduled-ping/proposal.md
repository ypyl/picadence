# Scheduled pings: the tray wakes a session on a cadence (L2)

## Why

Some pi sessions hold a standing instruction that only a clock can act on: check
the news, poll the queue, look at the deploy. pi has no timer, so the session
sits idle until the user remembers to type something, and the whole point of a
periodic job is that nobody has to remember.

piclaw already knows every live session on this machine and is already always
running, so it is the cheapest clock there is. What it lacks is permission to
speak.

## What Changes

- A session row can be armed with a cadence: every 15 minutes, every hour, every
  week, every month, every year, or off.
- The tray keeps the schedule itself, with the next and last fire, so it
  survives a tray restart, a reboot, and a session that is resumed later.
- When a schedule is due and its session is idle, the session starts one turn
  from a message that names piclaw, the time and the cadence, and tells the agent
  to follow the session's standing instruction. The message carries no
  instruction of its own.
- A busy session is skipped and the window counts as missed. A schedule that
  fell due while nothing was running catches up once when its session returns,
  never as a burst.
- **BREAKING** for the product's stated behavior: the requirement that the tray
  only observes is replaced by one that lets it start a turn in a session the
  user armed. Every other refusal stays: no input stream, no keystrokes, no
  starting or stopping a session, no network.
- Nothing here starts a pi process. With no tab open there is nothing to wake,
  so the schedule waits.

## Capabilities

### New Capabilities

- `scheduled-ping`: a session can be armed with a cadence, the tray remembers it
  across restarts and resumes, a due schedule wakes the session once, and the
  panel shows the cadence and what comes next.

### Modified Capabilities

- `agent-status`: the panel's "a session row takes no action" and the tray's
  "only observes" requirement change. Everything else about state reporting
  stays as it is.

## Impact

- `extensions/piclaw.js`: consumes a ping, starts a turn, decides to skip. New
  unit tests beside the existing ones.
- `src/Piclaw`: a schedule store, the cadence arithmetic, an arming control in
  the panel, and one more step in the existing one-second poll.
- `openspec/specs/agent-status/spec.md`: two requirements replaced.
- `ARCHITECTURE.md`: a second store and a second direction on one arrow, so
  `architecture-doc` rides with this change.
- `DESIGN.md`: the arming control and the armed row.
- `README.md`: what the product does, and the line that calls it "not an
  orchestrator".
- `CHANGELOG.md`.
- No new dependency, no new process, no network call.

## Routing

0. **Does observable behavior change?** Yes, twice. A person can arm a row and a
   session wakes on its own (`scheduled-ping`), and the tray's authority changes
   (`agent-status`, "The tray only observes", "a session row takes no action").
1. **Can it be undone easily?** Yes: the commits revert, and the schedules are
   files in a directory the feature introduced. Nothing else on the machine
   knows about them.
2. **Can a machine prove it correct?** Yes. The cadence arithmetic, the due and
   catch-up decision, and the store's round trip are pure functions with unit
   tests; the extension's consume-or-skip decision is a pure function with
   `node --test`. A test that names each scenario is part of the plan, not a
   hope.
3. **Does anything outside the change depend on it?** Yes. The state directory
   is a contract between two components written in two languages, and this
   change adds two more file shapes to it. The contract is documented in
   `ARCHITECTURE.md` (Store) and specified in `design.md`; both move here.

Highest justified level: **L2**. The decision is
[ADR 0002](../../../adr/0002-the-tray-may-wake-a-session.md). It is not L3
because nothing is deleted, migrated or published, and the agent gains no
authority it did not already have; the tray decides the moment, not the
permission. A later change that lets a scheduled run act on something the user
did not arm for is L3.

## Contract

The file contract under `%LOCALAPPDATA%\piclaw` between `Piclaw.exe` and
`extensions/piclaw.js`. A change to it breaks both halves, so it is named here
rather than left implicit.

## Approval

- Gate: L2 ADR
- Approved by: Yauhen Pyl
- Approved at: 2026-11-21
- Artifact: adr/0002-the-tray-may-wake-a-session.md sha256:1169a736a233c608cd79679199851522fa0fc85233b83ea69a2fc59f2a6327f5
- Artifact: openspec/changes/add-scheduled-ping/proposal.md sha256:b6dd5f3ff8230d85781f232bb38b226fb9b8c716bece1368173c978277c95572
- Words: "(none)"

## Approval

- Gate: L2 delta spec
- Approved by: Yauhen Pyl
- Approved at: 2026-11-21
- Artifact: openspec/changes/add-scheduled-ping/specs/agent-status/spec.md sha256:e803cc8dbd4725beb8f637d6b776710ac6c3197eb9acc92487dd100a875604d4
- Artifact: openspec/changes/add-scheduled-ping/specs/scheduled-ping/spec.md sha256:b2b2685e53cfaa5dca2f85c18df423e10a5e94d3c938bcef3c71190447ae55f0
- Artifact: openspec/changes/add-scheduled-ping/proposal.md sha256:b6dd5f3ff8230d85781f232bb38b226fb9b8c716bece1368173c978277c95572
- Words: "(none)"
