# A pin follows the name of the session it names (L0)

## Why

Rename a session while it runs and its row follows, because the row reads the state
file. Close the tab and the row falls back to the pin, which was written when the
session was kept and holds the name it had at that moment. So the name appears while
the session is active and disappears when it ends, which reads as a bug in the name
and is a bug in the copy.

## What Changes

- On every refresh, a pin whose session is live takes that session's name and folder
  when they differ, and is written back once. Nothing else about the pin moves.
- Two paths are compared without their trailing separator, so a pin is not rewritten
  for a difference that is not a difference.

## Capabilities

None. The requirement it restores: **A session can be kept after it ends**, whose row
should name the session it holds, so `skip_specs: true`.

## Impact

- `Pin.Follow`, `Program` (one pass over the pins before the panel), tests.

## Routing

0. **Does observable behavior change?** A kept row shows the name its session took.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes: the follow decision is a comparison of two
   records, with tests for a new name, an unchanged one, and a session with no folder
   of its own.
3. **Does anything outside the change depend on it?** No. It writes the pin files the
   tray already owns.

Level: **L0**.
