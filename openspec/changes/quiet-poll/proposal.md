# The poll stops re-reading what has not changed (L0)

## Why

Every second the tray enumerates the state directory, reads every file and parses
it again, whether or not pi has written anything. pi writes a state file only on
a transition, so almost every tick re-parses five identical files.

That churn is what drives the managed heap: the process warms up to about 16.4 MB
private bytes and 28.6 MB working set and then stays there. Measured on this
machine on 2026-11-21 with five live sessions, flat for three minutes after a
90-second warm-up. The figure in `REPO-FACTS.md` and `README.md` is a ten-second
sample taken during that warm-up, which is why it reads 3.4 MB.

Nothing leaks. The number is the GC holding a budget it was given because of the
allocation rate, not memory that grows without bound.

## What Changes

- The tray remembers the last parse of each state file, keyed by name, the file's
  length and its last write time. An unchanged file costs one stat per tick
  instead of a read and a parse. A changed one is read again, as before.
- An entry whose file disappears is dropped, so a tray left running for weeks
  keeps one entry per live session and no more.
- `README.md` and `REPO-FACTS.md` carry the measured steady state instead of a
  warm-up sample.

No behavior changes. The requirement that a state change appears within two
seconds is not touched: a rewritten file changes its write time, so the next tick
reads it as it always did.

## Capabilities

None. An internal read cache, so `skip_specs: true`.

## Impact

- `src/Piclaw/SessionStore.cs`, the tests beside it, and the two documents that
  quote a memory figure.

## Routing

0. **Does observable behavior change?** No. The same sessions, in the same order,
   with the same states, within the same two seconds.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes: a test asserts that an unchanged file
   returns the same parsed session, that a rewritten file is parsed again, and
   that a touched write time is noticed. The memory figure is evidence in
   `REPO-FACTS.md`, not the check.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
