# Design

## Two controls, one side

```text
[dot]  docs-site                  [>]  [pin]  kept 2h
       not running
```

The read of a row, right to left: how long it has been kept, the pin that says it
is kept, and the start control that acts on it. The start control sits 6 DIPs left
of the pin and is the same 22 by 22 target, so the two are one cluster rather than
two scattered hits:

```text
age      right edge, 44 wide
pin      left of the age, 22 by 22
start    left of the pin, 22 by 22, only when the restart would work
text     stops at whichever control is leftmost
```

## The glyph

`E768` from `Segoe MDL2 Assets`, a play triangle, rendered before choosing it.
Play says "start this" without meaning "resume" so literally that it would be
wrong elsewhere; on a row that says `not running` it is exactly right. The pin's
two shapes stay the same, so the two controls never look alike: pins are pins,
play is play.

There is no hover text and no second state. The control exists or it does not.

## Why the menu loses the item

A menu that repeats a control the row already carries is one more thing to read,
and this is the second time the panel has had to be told that. After this, a
kept row has no menu, so a right click on one does nothing at all, which is
consistent: there is nothing left to offer.

A running row's menu is unchanged: six cadences, or fewer when the cadence code
offers fewer.

## Hit order

`Start` and `Pin` do not overlap, so order between them does not matter; both are
checked before `Row`, because both sit inside the row's rectangle. The row's body
stays inert, which is the requirement that keeps a misclick from doing something.

## Tests

| behaviour | test |
|---|---|
| a kept row that can restart carries a start control, left of the pin | `PanelPinTests` |
| a kept row whose plan refuses carries none | `PanelPinTests` |
| the start control answers as `Start`, and the pin still answers as `Pin` | `PanelPinTests` |
| a kept row has no menu | `PanelKeptTests` |
| a running row's menu is still the cadence list | existing test |
