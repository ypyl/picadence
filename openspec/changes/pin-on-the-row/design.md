# Design

## Where it sits

The row is 44 DIPs tall and already spends its right edge on the age, 44 wide.
The pin goes immediately left of that, a 22 by 22 target centred in the row:

```text
[dot]  trayclaw                          [pin]  24s
       done · every hour, next in 12m
```

The text column now stops at the pin rather than at the age, so a long folder
name ellipsises a little earlier instead of running under the control.

## What it looks like

Two glyphs from `Segoe MDL2 Assets`, the icon font every Windows install carries,
checked by rendering before they were chosen:

| state | glyph | drawn |
|---|---|---|
| not kept | `E840`, an outline pin | `PanelTheme.SecondaryText` |
| kept | `E842`, a solid pin | `PanelTheme.PrimaryText` |

Shape carries the state and colour repeats it, which is the rule the state dot
already follows. The control is 22 DIPs wide because the glyph at 12 DIPs inside
it leaves a target a mouse can hit without care.

## What a click does

`PanelHit` gains `Pin`, and the hit test checks it before the row, because the
control is inside the row's rectangle. A click there toggles: an unkept live
session writes a pin, a kept one removes it, and a kept session that is not
running loses its row.

The cursor turns to a hand over the control and stays an arrow over the rest of
the row, which is the same promise the other controls make.

## What leaves the menu

The right-click menu keeps only what is not on the row:

- a live row: the six cadences
- a kept row that can restart: `Restart`
- a kept row that cannot: no menu at all

`Keep` and `Stop keeping` are gone from it, because a menu that repeats a control
the row already carries is one more thing to read.

## Tests

The layout and the hit test are functions, so the behaviour that can be proven is
proven:

- every row has a pin, inside the row and left of the age
- a click on the pin returns `Pin`, and a click on the row's body returns `Row`
- a kept row draws the solid glyph, an unkept row the outline one
- a kept row that cannot restart has no menu
- a live row's menu holds the six cadences and nothing else

The glyph itself is a rendering check: a picture of the panel with one kept row
and one not.
