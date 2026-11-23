# Design

The visual language of the panel: what it looks like, why each number is that
number, and where those numbers live in the code. Read this before changing how
picadence looks. Behavior is in `openspec/specs/`, decisions are in `adr/`, and this
file is only about appearance.

The panel is one small surface: a flyout anchored to a tray icon, read for two
seconds at a time. Everything here serves that: glanceable, quiet, and honest
about state.

## Principles

1. **Glanceable before readable.** The icon's colour answers "does anything need
   me" from across the screen. The panel only answers "which one", and it is read
   on purpose.
2. **Quiet.** No animation, no badge, no notification unless the state itself is
   the news. A status surface that draws attention when nothing is wrong is a
   status surface people stop looking at.
3. **One meaning per signal.** Colour never carries information alone: every dot
   has a word beside it. A person who cannot separate red from green reads
   `failed` and `working` instead.

These follow the intent behind Fluent's guidance: natural on the platform, built
around focus, and consistent because the same few values are reused everywhere.

## Tokens

### Colour

| Token | Value | Used for |
|---|---|---|
| surface | `#202020` | panel background |
| row | `#2B2B2B` | session row |
| hover | `#3A3A3A` | control or cadence under the pointer |
| outline | `#3A3A3A` | the panel's 1px edge, and the cadence list's |
| text primary | `#FFFFFF` | folder, control label |
| text secondary | `#C5C5C5` | state, detail, age, control value |

In dark mode a lighter surface reads as closer to the reader, which is why the
row sits above the panel and the hover sits above both, rather than using
shadows or borders to separate them.

State colours are dots only, never text on colour:

| State | Colour | Word |
|---|---|---|
| failed | `#DC2828` | failed |
| waiting | `#E6A000` | waiting |
| working | `#28B450` | working |
| done, quiet | `#969696` | done, quiet |

Contrast, measured by `PanelTheme.ContrastRatio` and asserted by a test:

| Text on | Ratio |
|---|---|
| primary on surface | 16.3 : 1 |
| secondary on surface | 9.4 : 1 |
| primary on row | 14.2 : 1 |
| secondary on row | 8.2 : 1 |
| primary on hover | 10.3 : 1 |

The floor is 4.5 : 1, and `PanelTheme.MinimumContrast` is that number, so a
palette edit below it fails the build.

### Spacing

Fluent's global ramp is a 4px system with a few half steps for icon alignment:
2, 4, 8, 12, 16. Those five are all this panel needs, and they are the only
spacing values in `PanelModel`.

| Token | Value | Where |
|---|---|---|
| `Gap` | 8 | between the icon and the panel |
| `Padding` | 12 | panel edge to content |
| `RowSpacing` | 8 | between session rows |
| `ActionSpacing` | 2 | between the two controls |
| `ActionInset` | 2 | the hover fill inside its row |
| `ActionPadding` | 12 | label and value inside the fill |
| `RowPadding` | 12 | the age inside the row's right edge |

### Cadence list

| Token | Value | Where |
|---|---|---|
| `FlyoutWidth` | 180 | wide enough for "every 15 minutes" and no wider |
| `FlyoutItemHeight` | 30 | one cadence: a list item, not a control |
| `FlyoutPadding` | 6 | the list's own edge to its first item |
| `FlyoutHeaderHeight` | 24 | the heading's row, so the list says what it is |
| `FlyoutRule` | 1 | the hairline between the heading and the cadences |

### Type

| Role | Size | Weight | Face |
|---|---|---|---|
| panel title | 12 | semibold (600) | Segoe UI Variable Text |
| header summary | 12 | regular (400) | Segoe UI Variable Text |
| session folder | 14 | regular | Segoe UI Variable Text |
| session state, detail, age | 12 | regular | Segoe UI Variable Text |
| a cadence in the list | 12 | regular, semibold when chosen | Segoe UI Variable Text |
| the list heading | 12 | semibold (600) | Segoe UI Variable Text |
| control label, value | 12 | regular | Segoe UI Variable Text |

Body is 14, caption is 12, and nothing else is used. `Segoe UI` is the fallback
GDI picks when the variable face is absent.

### Shape and stroke

| Token | Value | Where |
|---|---|---|
| panel radius | 8 | from DWM, so the edge stays antialiased |
| control radius | 4 | the hover fill |
| dot | 8 × 8, radius 6.5 in a 16 × 16 icon | the tray icon |
| stroke | 1 | the panel outline |

No shadows are drawn. The shell gives a popup its shadow, and a second one drawn
by hand reads as a mistake.

## Layout

```
┌─────────────────────────────────────────────┐ 320 wide
│ 12   trayclaw            1 waiting, 2 working │ 40 header
│                                              │
│ ┌──────────────────────────────────────────┐ │ 8
│ │ ●  yauhenpylgithubio              19s    │ │ 44 row
│ │    waiting (bash)                        │ │
│ └──────────────────────────────────────────┘ │
│ ┌──────────────────────────────────────────┐ │ 8
│ │ ●  trayclaw                        2m    │ │ 44 row
│ │    working (edit)                        │ │
│ └──────────────────────────────────────────┘ │
│ ┌──────────────────────────────────────────┐ │ 8
│ │  [x] Autostart            Quit          │ │ 32 control
│ └──────────────────────────────────────────┘ │ 2
│ ┌──────────────────────────────────────────┐ │
│ │  Quit                                    │ │ 32 control
│ └──────────────────────────────────────────┘ │ 12
└─────────────────────────────────────────────┘
```

- A session row is 44 tall: an 8 dot at 8 from the left, a 14 folder, a 12 state
  line under it, and a 12 age right-aligned 12 from the row's edge. An armed row
  adds its cadence to that state line: `done · every hour, next in 24m`.
- A control is 32 tall, which is Fluent's medium control height. Its hover fill
  is inset 2 from the row, its label starts 12 inside the fill, and its value ends
  12 inside the fill. Nothing in a button touches the button's edge.
- The cadence list is 180 wide and sits on the row it belongs to, right edges
  aligned, moved up when it would cross the bottom of the panel. It is painted
  inside the panel's own window, over the rows, with the same surface and outline
  as the panel.
- The list opens with a heading, `Repeat ping`, in the same 12 semibold the panel
  title uses, over a 1px rule in the outline colour. Six bare cadences read as a
  mystery the first time; one line of naming makes it a decision. The heading is
  a label and not a choice, so a click on it closes the list and arms nothing.
- The chosen cadence is the only item in full white and semibold; a pointer over
  an item fills it with the hover token.
- The list exists because a real popup menu would take activation, and the panel
  closes when it loses activation. It also keeps the dark surface, which the shell
  menu would not.
- The panel caps at 8 session rows and says how many are not shown. A flyout that
  covers the screen is worse than one that admits it is short.
- The panel opens above the icon, right edges aligned, flipped below when there is
  no room, and clamped into the monitor's work area.

## States

| Element | Rest | Hover | Pressed | Focus |
|---|---|---|---|---|
| session row | row fill | row fill (not a control) | none | none |
| control | no fill, plain text | hover fill, hand pointer | commits on click, no dwell | none |
| cadence item | surface, secondary text | hover fill, hand pointer | commits on click | none |
| panel | surface, 1px outline | — | — | — |

Hover exists to say "this is clickable". It appears on pointer entry, clears on
pointer leave, and never appears on a session row, because a row does nothing on
a left click. The cadence list is opened by a right click, and only then does a
row lead anywhere.

**Known gap:** there is no keyboard focus and no focus ring. The panel is opened
by pointer and closed by Escape; a keyboard user can close it but cannot reach a
control. Closing that gap needs a focus model and an access key, and it is its own
change, not a line here.

## What we deliberately do not do

- **No acrylic or mica.** A tray flyout that blurs whatever is behind it is
  harder to read over a terminal, and the panel is read against one.
- **No animation.** Opening, closing, and hover are instantaneous.
- **No elevation drawn by hand.** One 1px outline, and the shell's shadow.
- **No icons** beyond the state dot. A tool glyph beside a session would need a
  mapping from tool name to art, and the tool name in the detail line already
  says it.
- **No second typeface.** One family, two sizes, two weights.

## Where the numbers live

`PanelModel` holds every value in this document as a constant, and
`PanelTheme` holds the palette and the contrast maths. A test asserts the control
insets and the contrast ratios, so this file and the code cannot drift apart
without a failing build. If a number has to change, it changes in the constant and
in the table above, in the same commit.

## References

- Fluent 2, spacing ramp and the 4px system: <https://fluent2.microsoft.design/layout>
- Fluent 2, design tokens and the two-layer model (global values, then semantic
  use): <https://fluent2.microsoft.design/design-tokens>
- Windows design, layering and elevation in dark mode:
  <https://learn.microsoft.com/en-us/windows/apps/design/style/color>
- Windows design, spacing between content areas:
  <https://learn.microsoft.com/en-us/windows/apps/design/style/spacing>
