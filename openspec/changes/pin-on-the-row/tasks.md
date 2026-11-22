# Tasks

- [ ] 1.1 Add the pin rectangle to every row and stop the text column at it.
  Verified by a test that asserts the rectangle is inside the row and left of the
  age, for a live row and for a kept one.
- [ ] 1.2 Add `PanelHit.Pin` and test it before the row in `HitTest`. Verified by a
  click at the pin's centre and one at the row's body.
- [ ] 1.3 Draw the glyph: `E840` for a session that is not kept, `E842` for one
  that is, in `Segoe MDL2 Assets`, with a hover fill and a hand cursor.
- [ ] 1.4 A click on the pin keeps or unkeeps that row's session, the same way the
  menu did. Verified by the panel's own keep path, which the pin now calls.
- [ ] 1.5 Remove `Keep` and `Stop keeping` from the flyout, and give a kept row
  that cannot restart no flyout at all.
- [ ] 1.6 Update `PanelTests` and the README if a screenshot no longer matches.
- [ ] 1.7 Run `bash scripts/verify.sh`, then look at the panel with one kept row.
