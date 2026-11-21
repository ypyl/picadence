# The cadence limits are greppable (L0)

## Why

`design.md` in `add-scheduled-ping` names two deferred limits and calls them
`shortcut:` markers, but neither was written into the code. The debt rule in
`AGENTS.md` says a known limit is a `shortcut:` comment with an upgrade trigger,
and a marker that lives only in an archived design is a marker nobody greps.

## What Changes

- A `shortcut:` comment on the poll interval in `extensions/piclaw.js`: a ping is
  delivered up to ten seconds late, and the upgrade trigger is a cadence that
  needs second precision.
- A `shortcut:` comment on the missed-window cap in `src/Piclaw/Cadence.cs`: the
  count stops at 1000 and the message says "at least", and the trigger is someone
  needing an exact number for a longer gap.

No behavior changes. No requirement moves: the two specs the previous change
wrote stay exactly as they are.

## Capabilities

None. Comments only, so `skip_specs: true`.

## Impact

- `extensions/piclaw.js`, `src/Piclaw/Cadence.cs`.

## Routing

0. **Does observable behavior change?** No. Two comments.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes: `scripts/check-change.sh` refuses a
   `shortcut:` marker with no upgrade trigger, and the tree still builds and
   tests.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
