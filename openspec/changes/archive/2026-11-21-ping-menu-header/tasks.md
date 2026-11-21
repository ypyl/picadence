# Tasks

- [x] 1.1 Add the heading to the cadence list: a `Repeat ping` label with its
  own row above the cadences and a one-pixel rule under it, so the list says what
  it is before the first item. Verified by a layout test named `the cadence list
  says what it is`, which asserts the heading, that it sits inside the list, and
  that no cadence overlaps it.
- [x] 1.2 Paint it: the heading in the primary text at the same weight as the
  panel title, and the rule in the outline colour. Verified by the existing
  contrast test, which covers primary text on the panel surface.
- [x] 1.3 Record it in `DESIGN.md` under the cadence list tokens and the layout
  notes. Verified by reading it against the constants in `PanelModel`.
- [x] 1.4 Run `bash scripts/verify.sh` and capture the list again to confirm the
  heading reads at the same size as the rest of the panel.
