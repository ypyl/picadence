# Tasks

- [x] 1.1 `Native`: delete the five registry declarations, `SW_SHOWNA`, `DT_WORDBREAK`, `NIF_TIP`, `CommandQuit`.
- [x] 1.2 `PanelModel`: delete `PanelHit.Autostart`, its hit test branch, and `ActionHeight`, `ActionSpacing`, `ActionPadding`, `IconSize`.
- [x] 1.3 `SessionStore.ReadAll`: drop the `now` parameter, and update `Program` and the tests.
- [x] 1.4 `Program.Refresh`: prune a start stamp once it is older than `SchedulePlan.StartGrace`.
- [x] 1.5 `package.json` names 0.3.0, and Project facts records that the field moves with the tag.
- [x] 1.6 The interop test allows one library fewer, so a registry call comes back as a failure.
- [x] 1.7 Run `bash scripts/verify.sh` and record the counts.
