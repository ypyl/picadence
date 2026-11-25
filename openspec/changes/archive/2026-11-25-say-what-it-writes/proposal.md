# Say what it writes and reads (L0)

## Why

The README says one thing about storage: state lands in
`%LOCALAPPDATA%\picadence\agents` and nothing leaves the machine. That answers
where the state goes and nothing else.

So a person cannot ask the README what picadence keeps about them, and uninstall
does not answer it either: `dotnet tool uninstall -g Ypyl.Picadence` takes the
command and the exe and leaves every record of your sessions behind, because none
of it lives in the tool store. The three lanes the tray owns, what it reads out of
pi's own folder, and what stays after an uninstall are each a reasonable
question, and the README answers none of them.

## What Changes

- A section that names every folder picadence creates, who writes it, what is in
  it, and when an entry goes away.
- The same for what it reads and never writes, with the reason for each, so the
  claim "nothing leaves the machine" has the list behind it.
- An uninstalling section: stop the tray first and why, what the tool uninstall
  removes, what it leaves, and what to delete for a clean slate.
- The existing sentence about `agents` points at the new section instead of
  restating one line of it.

## Capabilities

None. No behavior moves: the tray writes and reads exactly what it wrote and read
before, and an uninstall does exactly what it did before. The change only stops
the README from hiding it, so `skip_specs: true`.

## Impact

- `README.md`.
- No code, no tests, no dependency, no store, no contract document.

## Routing

0. **Does observable behavior change?** No. Every sentence added describes what
   the product already does.
1. **Can it be undone easily?** Yes: one commit, one file.
2. **Can a machine prove it correct?** The parts that can be checked are checked
   by what already runs: `scripts/check-change.sh` refuses a spec with rendering
   detail, and the paths named here are the ones `Paths` resolves. A wrong
   sentence about a path is caught by reading it against `Paths.cs`, which is what
   writing it demanded.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
