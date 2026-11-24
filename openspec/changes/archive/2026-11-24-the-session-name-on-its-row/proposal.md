# The session's own name appears on its row (L0)

## Why

pi lets a session carry a name of its own, and the tab already shows it. The panel
showed only the folder, so two sessions in the same folder were the same row, and a
session called "Adopting Aspire research" was indistinguishable from any other
checkout of `yauhenpylgithubio`.

## What Changes

- The second line of a row begins with the session's name, then its state, then its
  cadence: `Adopting Aspire research · done · every hour, next in 12m`.
- A session whose name is its folder anyway says it once. Blank names are absent,
  which is what most sessions have.
- Kept rows use the name the pin recorded.

## Capabilities

None. The row still names the folder, the state and the age, which is what the spec
asks; this adds a fourth thing it already knew, so `skip_specs: true`.

## Impact

- `PanelModel` (a field and one rule), `Panel` (one composition), tests, `README.md`,
  `CHANGELOG.md`.

## Routing

0. **Does observable behavior change?** A row shows one more string. No requirement
   moves.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes: which names reach a row is a rule over
   two strings, with a test either side of it.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
