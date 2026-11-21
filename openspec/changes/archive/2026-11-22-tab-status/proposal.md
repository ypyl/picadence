# The tab shows its session (L1)

## Why

The tray's one open question in `ARCHITECTURE.md` is which terminal tab a session
lives in: Windows Terminal exposes no handle, so the panel shows a folder and a
session name and asks you to do the matching. The tab itself has no such problem,
because it is the tab.

pi already hands an extension the title of the terminal it runs in, so the state
the tray already knows can be written to the place where the session actually is.
Anyone using pi without a tray on another platform gets the same report from the
same file.

## What Changes

- The pi extension puts a mark and the session's label in the terminal title on
  every state change: working, waiting, failed, done, and plain label when idle.
- A terminal that understands the ConEmu progress sequence, Windows Terminal
  among them, can also show the state as a ring on the tab and on the taskbar.
  That is off by default, because it means writing the sequence directly to the
  terminal and nobody has watched a live session with it yet.
- Nothing changes in the tray. It keeps reading the same state files.

## Capabilities

### New Capabilities

- `terminal-tab`: what a session's tab says about that session.

## Impact

- `extensions/picadence.js` and its tests.
- `README.md`: what the tab shows, and the two Terminal behaviours that can
  override it.
- No new dependency, no new process, no network.

## Routing

0. **Does observable behavior change?** Yes. A tab carries a mark it did not
   carry, which is new information in a new place.
1. **Can it be undone easily?** Yes, one commit; the extension is a file pi
   loads, and the tray is untouched either way.
2. **Can a machine prove it correct?** Yes. The composition is a pure function on
   state and label, one test per state, and the extension's own test proves the
   title is set on a transition and not on a console run.
3. **Does anything outside the change depend on it?** No. The state file contract
   does not move, and the tray reads the same thing it did.

Level: **L1**.

## What is not proven yet

The progress ring. pi's extension API has no channel for it, so it is a raw
escape sequence written to the terminal while pi owns the screen. The sequence is
correct and the terminal honours it, but whether a write like that can disturb
pi's repaint is unproven, so it ships off, behind `PICADENCE_TAB_PROGRESS=1`.

## Approval

- Gate: L1 delta spec
- Approved by: Yauhen Pyl
- Approved at: 2026-11-21
- Artifact: openspec/changes/tab-status/specs/terminal-tab/spec.md sha256:888be2cdd7dd1553325f4111bee5cb7582cfa29c76ec27776fef5e9657c50e08
- Artifact: openspec/changes/tab-status/proposal.md sha256:812988508b200539ef5cad9c7e66b7034165f697bdb13b1954b60003111ac936
- Words: "(none)"
