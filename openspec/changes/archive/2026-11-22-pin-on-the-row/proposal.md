# Keep a session from the row (L1)

## Why

Keeping a session is the thing you decide while looking at that session's row,
and right now it hides in the right-click menu behind six cadences. The pin is
one click, and it also says at a glance which sessions are kept, which the menu
cannot do because it is closed.

## What Changes

- Every row gets a small pin control on its right, next to the age. Clicking it
  keeps the session, and clicking it again stops keeping it.
- The control shows its state by shape: an outline pin when the session is not
  kept, a solid pin when it is.
- `Keep` and `Stop keeping` leave the right-click menu, which goes back to being
  about cadences. A kept row that cannot be restarted has no menu at all, because
  the pin is where the other action lives.

## Capabilities

### Modified Capabilities

- `agent-status`: every row carries a pin control, and a row click outside it
  still does nothing.

## Impact

- `PanelModel`, `Panel`, its tests, and the screenshots the README shows.
- No file changes on disk, no new dependency.

## Routing

0. **Does observable behavior change?** Yes: a row gains a control that acts on a
   click.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes: the layout is a rectangle and the hit
   test is a function, so where the control sits and what a click there returns
   are both tests. That the glyph exists in the font is a visual check.
3. **Does anything outside the change depend on it?** No.

Level: **L1**.

## Approval

- Gate: L1 delta spec
- Approved by: Yauhen Pyl
- Approved at: 2026-11-22
- Artifact: openspec/changes/pin-on-the-row/specs/agent-status/spec.md sha256:6ecb24f0410248e9fc462000eabc70c4ec86a4d14ba2ff9ef27d9482a866c0f3
- Artifact: openspec/changes/pin-on-the-row/proposal.md sha256:b7151b9707eb15972ff3ab0426f7c64f6b493dacfd0da2d9299d024baa5f0e97
- Words: "(none)"
