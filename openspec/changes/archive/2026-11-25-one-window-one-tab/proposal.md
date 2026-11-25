# One window opens one tab (L1)

## Why

The tray starts a tab in two cases: a cadence that falls due for a session the user
kept, and the play control on that session's row. Both call the same start, but only
the cadence path records when it started, and that record is the 30 second grace that
stops a second tab opening while the first is still booting.

So: a session is kept, armed with every hour, and due at 14:00. At 13:59:30 the user
clicks play. The tab opens, and pi takes a second or two to write its state file. The
next tick arrives first, sees a kept session that is not running, has no start stamp,
and finds the window due, so it opens a second tab on the same conversation. The
grace was written for exactly this and does not cover the one path a person uses.

## What Changes

- `Panel.Launcher` takes the session id as well as the plan, because the caller knows
  which session it is starting and the start needs to say so.
- `Program` records the start stamp there, after the tab is really started, so any
  tab the tray opens holds the same grace whatever asked for it.
- `Panel.RestartRow` passes the row's session id.
- One changelog entry.

## Capabilities

None. No requirement moves: a cadence still opens exactly one tab for a kept session,
which is already required. The fix makes that true for the one path that missed it, so
`skip_specs: true`.

## Routing

0. **Does observable behavior change?** Yes: one window opens one tab when the user
   starts a session by hand a moment before its cadence. That is what the requirement
   already says, so no requirement moves.
1. **Can it be undone easily?** Yes, one commit, nothing stored.
2. **Can a machine prove it correct?** No. The proof needs a real tab that has not yet
   written its state, a real clock, and a real click: `Program` and `Panel` are the two
   layers this project deliberately keeps out of unit tests, and the timing window is
   a second wide. A person can follow it by reading the two call sites, which is why
   this is L1 and not L0.
3. **Does anything outside the change depend on it?** No. The signature is internal to
   the tray.

Level: **L1**, `skip_specs: true`: the gate is the whole change.

## What a reader should check

That both callers of `Panel.Launcher` are the ones named above, and that the stamp is
recorded only after `Start` returns true — a failed start must not suppress the next
window.

## Approval

- Gate: L1 whole change (skip_specs)
- Approved by: Yauhen Pyl
- Approved at: 2026-11-25
- Artifact: openspec/changes/one-window-one-tab/proposal.md sha256:0966034f1cc83fe531d54efa4eb9e5192faf8549b03fb07bfc9b080c2e23fdfd
- Words: "(none)"
