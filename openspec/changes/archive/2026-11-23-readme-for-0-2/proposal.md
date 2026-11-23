# The README describes what 0.2.0 does (L0)

## Why

The README was written for 0.1.0 plus the tab mark. Since then the panel gained a
pin, a play control, scrolling, a checkbox footer and a dependency notice, and a
reader would not know any of it. A release page that links to a README describing
an older build is a small lie in the one place a newcomer starts.

## What Changes

- The panel description names the two row controls, the five-row window with its
  scroll legend, and the one-row footer.
- A new section says what the panel does when pi or Windows Terminal is missing,
  because that is what a person will hit first on a fresh machine.

## Capabilities

None. Prose only, so `skip_specs: true`.

## Impact

- `README.md`.

## Routing

0. **Does observable behavior change?** No.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Not by test; it is checked by reading it
   against the code, which is what this commit records.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
