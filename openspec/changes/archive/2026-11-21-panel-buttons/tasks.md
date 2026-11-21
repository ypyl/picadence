# Tasks

## 1. The controls

- [x] 1.1 `PanelActionRow` carries `LabelBounds` and `ValueBounds`, computed from
      `ActionInset` and `ActionPadding`. Verified: the inset test.
- [x] 1.2 The controls are 32 DIPs tall and the paint uses the new boxes.
      Verified: the height assertion and the capture.
- [x] 1.3 Two tests: the content never touches a control's edge, and the autostart
      value reads on or off. Verified: `dotnet test`.

## 2. The document

- [x] 2.1 Write `DESIGN.md`: principles, tokens, spacing ramp, type ramp, layout
      with measurements, states, the accessibility floor, and the deliberate
      omissions, with the sources that shaped it. Verified: every number in it is
      a constant in `PanelModel` or `PanelTheme`, and every ratio is one the
      contrast test computes.
- [x] 2.2 Point `README.md` and `CONVENTIONS.md` at it. Verified: a reader who
      lands on either finds `DESIGN.md` before changing a colour.

## 3. Finish

- [x] 3.1 `bash scripts/verify.sh`, a capture with a control hovered, the
      changelog entry. Verified: the capture shows the padding.
- [x] 3.2 Archive, commit, merge. Verified: the check passes and trunk carries
      the three commits.
