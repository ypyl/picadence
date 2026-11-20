# Design

The decision to draw Win32 ourselves stands (ADR 0001), so the panel is a borderless
window painted with GDI, not a toolkit control. Everything below the Win32 line is
a pure function, because that is the part a machine can check.

## The window

- `WS_POPUP`, no caption, extended style `WS_EX_TOOLWINDOW` so it stays out of
  Alt+Tab and the taskbar. Class style `CS_DROPSHADOW` for the shadow Windows
  gives a popup, which is cheaper and more correct than drawing one.
- Rounded corners come from DWM (`DWMWA_WINDOW_CORNER_PREFERENCE` = round), not
  from a region, so the edge stays antialiased.
- `SetProcessDpiAwarenessContext(PER_MONITOR_AWARE_V2)` at startup; every size in
  this file is in DIPs. The window is sized once from the DPI of the monitor it is
  created on, and the paint scale is derived from the client rect it actually got,
  so content can never overflow the frame. Mixed-DPI setups get one scale, which
  is the `shortcut:` in `TraySettings`' neighbourhood and a later change.
- Painting: `WM_ERASEBKGND` returns 1, `WM_PAINT` renders into a memory bitmap and
  blits once, so a refresh does not flicker. Fonts are created once per DPI and
  cached.
- Focus: `SetForegroundWindow` before showing, so the panel receives Escape and
  `WM_ACTIVATE` reports the loss of focus that closes it. A click outside reaches
  the other window, which is the light-dismiss behaviour Windows already has.

## Layout, in DIPs

| Part | Value |
|---|---|
| width | 320 |
| padding | 12 |
| corner gap from the icon | 8 |
| header | 40 tall, 12px semibold, summary on the right |
| row | 44 tall, 8 between rows: 8px dot, folder at 14px, state and detail at 12px, age right-aligned |
| empty state | 44 tall, two lines of 12px |
| footer | 40 tall, Quit right-aligned at 12px |

Total height is the sum, so a list of five sessions is 40 + 5x44 + 4x8 + 40, about
330 DIPs. There is no scrolling in this change: the panel caps the list at eight
rows and the last row says how many are not shown, because a flyout that covers
the screen is worse than one that admits it is incomplete.

## Palette

| Use | Colour | On | Ratio |
|---|---|---|---|
| surface | `#202020` | | |
| row | `#2B2B2B` | | |
| outline | `#3A3A3A` | | |
| primary text | `#FFFFFF` | surface | 16.3 : 1 |
| secondary text | `#C5C5C5` | surface | 9.4 : 1 |
| primary text | `#FFFFFF` | row | 14.2 : 1 |
| secondary text | `#C5C5C5` | row | 8.2 : 1 |

The ratios are computed by the test from the same constants the painter uses, so a
palette change that drops below 4.5 : 1 fails the build. State colours are the dot
only, never text on colour: `#DC2828` failed, `#E6A000` waiting, `#28B450`
working, `#969696` quiet.

## Placement

`PanelPlacement.Place(size, iconRect, workArea)` returns the top-left corner:

1. Right edges align, so the panel grows leftwards from the icon.
2. Sit above the icon with the gap. If the top would leave the work area, sit
   below it instead.
3. Clamp both axes into the work area, which is the multi-monitor case: the work
   area comes from the monitor under the icon, not the primary one.
4. If the panel is taller than the work area, pin it to the top and let the row
   cap keep it short.

## Files

| File | Holds |
|---|---|
| `PanelModel.cs` | the layout in DIPs, rows, empty state, hit test |
| `PanelPlacement.cs` | the placement rule above |
| `PanelTheme.cs` | palette, contrast maths |
| `Panel.cs` | the window, the paint loop, the input |
| `Program.cs` | routes both clicks, owns the timer, keeps the panel current |

`AgentStatus.Menu` and its `MenuEntry` type go away; `PanelModel` replaces them and
the tests move with it.

## What is not tested

The GDI calls and the DWM attributes, by design: the runtime smoke covers the
window appearing, and the look is the human's call. The smoke for this change also
captures the panel to a PNG so the result can be looked at, not just asserted.

## Deferred

Scrolling past eight rows, light mode, mica, animation, keyboard navigation
between rows, and a click on a row doing something.
