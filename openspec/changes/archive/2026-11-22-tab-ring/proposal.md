# The tab reports itself, without being asked (L1)

## Why

The tab's mark landed and works. The ring that Windows Terminal can draw on the
tab and on the taskbar is implemented and off, behind a variable, because nobody
had seen it run in a live session. Now that the mark is confirmed on a real tab,
keeping a second signal dark until someone reads a README is a defect of nerve,
not of design.

The changelog is also missing the tab feature entirely, so the next release page
would not mention it.

## What Changes

- The ring turns itself on where it can work: Windows Terminal, detected from the
  environment it sets. Every other terminal gets nothing, which is what it got
  before.
- `PICADENCE_TAB_PROGRESS=0` keeps it off, and `=1` forces it on anywhere, so the
  person using the tool always has the last word.
- `CHANGELOG.md` gains the entries the tab feature should have had: the mark, and
  the ring.

No other behaviour changes. The mark, the label, and everything the tray does stay
exactly as they are.

## Capabilities

### Modified Capabilities

- `terminal-tab`: gains the progress signal on Windows Terminal, and the switch
  that overrides it.

## Impact

- `extensions/picadence.js`, its tests, `README.md`, `CHANGELOG.md`.
- No new dependency, no new process, no network.

## Routing

0. **Does observable behavior change?** Yes: a tab in Windows Terminal now carries
   a ring it did not carry, and a red or amber taskbar entry with it.
1. **Can it be undone easily?** Yes, one commit, and the switch is the same file
   either way.
2. **Can a machine prove it correct?** Yes: the decision is a pure function over
   the environment, with a test for each combination, and the sequences already
   have tests that check them character by character.
3. **Does anything outside the change depend on it?** No. The state file contract
   does not move.

Level: **L1**.

## Approval

- Gate: L1 delta spec
- Approved by: Yauhen Pyl
- Approved at: 2026-11-22
- Artifact: openspec/changes/tab-ring/specs/terminal-tab/spec.md sha256:8d68ad6b24f4350e6c37ba3e48ba31e741fcd36943c2f1f09db4de02575f5bca
- Artifact: openspec/changes/tab-ring/proposal.md sha256:b52935d15558796cc6c6a56f62cae90b3daa37c228a3ff480fa249c702e72420
- Words: "(none)"
