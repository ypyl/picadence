# The panel resizes for its menu (L1)

## Why

Right-click a row and the cadence list opens inside the panel's own window. With one
or two sessions the panel is shorter than that list: 116 DIPs of panel against 217
DIPs of list, because the list has a heading and six cadences. The layout already
grows to hold it, and the model test proves the list fits inside the layout. The
window does not grow with it, so everything below the panel's bottom edge is never
painted and never clicked. With one session the user sees the heading and two
cadences, and "every year" and "off" are unreachable.

The reason is one missing call. `Update` re-places the window when the layout grows,
but `Rebuild` — the path that opens and closes the list, and the only path that can
add the extra height — replaces the layout and repaints without re-placing. When the
next tick arrives, the layout already carries the taller height, so `Update` sees no
change and does not place either. Nothing ever corrects it.

## What Changes

- `Panel.Rebuild` remembers the height it is replacing and calls `Place()` when the
  new layout is taller or shorter, the same rule `Update` applies.
- The list is then painted inside a window that is as tall as the list, so every
  cadence can be seen and clicked, and the panel shrinks back when the list closes.
- One changelog entry.

## Capabilities

None. No requirement moves: this restores what two requirements already promise, that
the panel offers each session every cadence and a way to turn it off, and that it
stays inside the work area. So `skip_specs: true`.

## Routing

0. **Does observable behavior change?** Yes, and it is the change: the menu is fully
   visible. It is the behavior the spec already requires, so no requirement moves.
1. **Can it be undone easily?** Yes, one commit, and nothing is stored.
2. **Can a machine prove it correct?** No. The layout is already proven, and the gap
   is a missing Win32 call in a handler no unit test can drive: the test would have
   to open a window and measure it. A human sees it in one right click with one
   session open. That is why this is L1 and not L0.
3. **Does anything outside the change depend on it?** No.

Level: **L1**, `skip_specs: true`: the gate is the whole change.

## How to check it by hand

Start the tray with one session running, click the icon, right click the row. All six
cadences and "off" are visible inside the panel, and the panel is taller than before.
Pick "off" and the panel shrinks back to its single row.

## Approval

- Gate: L1 whole change (skip_specs)
- Approved by: Yauhen Pyl
- Approved at: 2026-11-25
- Artifact: openspec/changes/the-panel-resizes-for-its-menu/proposal.md sha256:671d2382be7db2053369383b5e988522bd9de620a8a27f5996bcf94610c87630
- Words: "(none)"
