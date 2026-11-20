# Tasks

## 1. The gap

- [x] 1.1 `PanelModel.RowPadding = 12`, and each row gains `AgeBounds`: a 44 DIP
      box whose right edge sits 12 DIPs inside the row. Verified: the test below.
- [x] 1.2 The paint draws the age into `AgeBounds`; the folder and state column
      ends 8 DIPs before it. Verified: the screenshot in the smoke.
- [x] 1.3 Test that no row's age reaches the row edge, for every row the model
      produces. Verified: `dotnet test`.

## 2. Finish

- [x] 2.1 Run the tray, open the panel, capture it, and look. Verified: the gap is
      visible and the row still fits its text.
- [x] 2.2 `bash scripts/verify.sh`, changelog entry, archive, merge. Verified: the
      check passes and the archive carries the result.
