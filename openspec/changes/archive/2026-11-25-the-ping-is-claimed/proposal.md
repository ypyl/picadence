# A ping is claimed before it is read (L0)

## Why

The tray writes a wake-up as one file per session and the extension reads it, then
deletes it. Those two steps are separate: `readFileSync`, then `unlinkSync`. The tray
can land a new ping between them, and the delete then destroys words nobody has read.
The window is small, and the loss is silent: the tray already moved the record forward,
so the window is never retried.

The same two lines have a second fault. A file that is not JSON leaves the parse
throwing, and the delete only runs when the parse succeeded, so a half-written file is
re-read and re-parsed every ten seconds for the life of the machine.

## What Changes

- `pollPings` renames the ping to a claim of its own before reading it. The tray's own
  name is free while the words are read, so a new ping survives and the next poll finds
  it. A claim that cannot be removed is left behind holding words nobody reads again,
  which is the harmless side.
- The claim is removed whether or not its body parses, so an unreadable ping is deleted
  rather than kept forever.
- Two tests: one wraps `fs.readFileSync` so the second ping is written exactly between
  the read and the delete, and one proves an unreadable ping is removed.

## Capabilities

None. It restores **A due cadence wakes an idle session once** and leaves every rule
about skipping and staleness where it is, so `skip_specs: true`.

## Impact

- `extensions/picadence.js`: one function.
- `extensions/picadence.test.mjs`: one new test, one assertion changed from "nothing"
  to "unreadable".
- No file format changes: the claim is a name the extension makes, deletes, and never
  shows to the tray.

## Routing

0. **Does observable behavior change?** A wake-up is no longer lost in a race with the
   tray's write, which is what the requirement already promises. No requirement moves.
1. **Can it be undone easily?** Yes, one commit; nothing on disk outlives a poll.
2. **Can a machine prove it correct?** Yes. The race test writes the next ping from
   inside the read of the current one, which is exactly the window; before this change
   the new file is gone, and after it the next poll sends it.
3. **Does anything outside the change depend on it?** No. The file names on the tray's
   side do not change.

Level: **L0**.
