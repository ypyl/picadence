# A pin keeps the place, not the name (L0)

## Why

Clicking the pin writes a pin file and the row does not change. The cause is one
wrong field: the panel's row carries the folder's *name*, because that is what a
row shows, and the pin was recording it as if it were a path. A pin whose folder
is `trayclaw` fails the pin file's own rule that requires an absolute path, so it
is rejected on the way back in and the tray never sees it. The click works, the
file is written, and nothing is shown, which is the worst combination.

pi already reports both: `folder` for the name a row shows, and `cwd` for the
place. The tray read `cwd` only to derive the name and threw the path away.

## What Changes

- The session the tray reads keeps the working directory pi reported, beside the
  name it shows.
- The pin records that place, so a restart can start in it.
- A pin whose folder is a name rather than a place is still readable, and the row
  says `the folder is gone` instead of the click doing nothing. Refusing at read
  time is what made a broken pin invisible; refusing at restart time says why.

No extension change: `cwd` has been in the state file all along.

## Capabilities

None. The behaviour is what `agent-status` already promises, that a kept session
restarts in its own folder, so `skip_specs: true`. The requirement it restores:
**A kept session can be restarted**.

## Impact

- `AgentSession`, `StateFile`, `PinFile`, `Restart`, `Panel`, `PanelModel`, tests.
- The two pins on this machine were written by the broken build and cannot be
  read; they are removed when this lands.

## Routing

0. **Does observable behavior change?** The pin starts showing what it was already
   supposed to show. No requirement moves; the defect is what is being fixed.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes: the chain from a state file with `cwd`,
   through a pin, back out of the store, is one test, and it failed before this.
3. **Does anything outside the change depend on it?** No. The state file already
   carried the field.

Level: **L0**.
