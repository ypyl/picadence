# Design

## Where the X goes

The header is 40 dips tall and holds a title on the left and a summary on the
right. The X takes the trailing edge, in a 22 by 22 target centred vertically, and
the summary ends 8 dips before it:

```text
picadence                    1 working, 4 done   [X]
```

Hit testing already asks the controls before the rows, and the header is not a row,
so an action whose rectangle sits in the header works without new machinery: the
one `Quit` action keeps its type and changes its rectangle.

## What disappears

- `Autostart.cs`: the `Run` key, the quoting, the follow-the-executable logic.
- The checkbox control, its two shapes, its hover, its click.
- `PanelModel.ActionRow` for it, and the row of actions under the list. The
  `Actions` list survives with one entry, because the header X is still a control.
- `Program`'s startup correction.
- Two requirements and three scenarios in `agent-status`, and `AutostartTests`.

## What the spec keeps saying

"The panel says when nothing is running" promises the way to quit is still offered
with an empty list. It is, in the header, and that requirement does not move.

## The one thing outside the repository

Windows has a Startup folder and every user has `shell:startup`. A person who
wants picadence at sign-in puts a shortcut there, which is one drag, needs no code,
and cannot go stale when the exe moves. The README says so in one sentence, and
says what to do if an older version had registered a `Run` value on your machine.

## Tests

| behaviour | test |
|---|---|
| the X is in the header, at the trailing edge, and answers as Quit | `PanelTests` |
| the summary ends before it | `PanelTests` |
| the panel has no footer row | `PanelTests` |
| nothing in the panel is autostart | the enum, by deletion |
