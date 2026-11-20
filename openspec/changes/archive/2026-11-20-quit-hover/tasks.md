# Tasks

## 1. The affordance

- [x] 1.1 `PanelTheme.Hover`, and add primary text on it to the contrast pairs.
      Verified: the contrast test covers the new pair.
- [x] 1.2 `PanelCursor.For(hit)`: hand for Quit, arrow otherwise. Verified: the
      pointer test.
- [x] 1.3 `Panel.cs`: track the pointer over the panel, paint a rounded highlight
      behind Quit while hovered, set the hand cursor over it, and clear both on
      mouse leave or when the panel hides. Verified: the hover capture.
- [x] 1.4 Centre the Quit label in its box. Verified: the capture.
- [x] 1.5 Test that a point over Quit hits Quit and asks for the hand, and that a
      point on a row asks for the arrow. Verified: `dotnet test`.

## 2. Finish

- [x] 2.1 Run the tray, open the panel, move the pointer over Quit, capture it,
      and look. Verified: the highlight is visible in the capture.
- [x] 2.2 `bash scripts/verify.sh`, changelog entry, archive, merge. Verified: the
      check passes and the archive carries the result.
