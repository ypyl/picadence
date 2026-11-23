# The gate knows what an active change removes (L0)

## Why

A durable spec keeps a requirement until the archive moves it. Deleting the code
and the tests for a removed requirement therefore made `scripts/check-change.sh`
fail on its own scenario titles, which meant a change could not delete a
requirement without either editing the gate in the same commit or reordering the
archive ahead of its commit. Neither is a good trade.

The script now reads the `## REMOVED Requirements` sections of the active deltas and
skips those titles, and says so with one line of output so an exemption is never
silent. This change is the record of that edit, and the sentence in `AGENTS.md` that
describes what the script checks.

## What Changes

- `AGENTS.md` gains four words: scenario titles have to appear in tests "unless an
  active change removes that requirement".

## Capabilities

None. A gate's own description, so `skip_specs: true`.

## Impact

- `AGENTS.md`. The script itself landed with `a-stale-pid-is-not-a-session`, where
  the need for it first appeared; its behaviour is proven below.

## Routing

0. **Does observable behavior change?** The product does not. A change that removes
   a requirement stops tripping a check that was written for changes that add one.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes, and it was: a throwaway change folder
   whose only content was a removed requirement with an untested scenario passed the
   script with "scenario titles of requirements an active change removes are not
   required", and the folder was deleted afterwards.
3. **Does anything outside the change depend on it?** `AGENTS.md` describes this
   script, and is updated here.

Level: **L0**.
