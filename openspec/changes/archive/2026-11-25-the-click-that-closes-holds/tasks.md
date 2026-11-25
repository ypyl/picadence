# Tasks

- [x] 1.1 `PanelController.After` takes "the panel just closed itself" as an argument.
- [x] 1.2 `Panel` records the moment it hid on focus loss, guards the next icon click for 400 ms, and clears the record on open.
- [x] 1.3 Test: an icon click in that window does not open the panel, and one after it does.
- [x] 1.4 Run `bash scripts/verify.sh` and record the counts.
- [ ] 1.5 Check by hand, after the merge: open the panel, click the icon again, and confirm it is gone.
