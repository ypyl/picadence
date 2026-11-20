# Tasks

## 1. The rules

- [x] 1.1 `Autostart` with `CommandFor`, `IsSetTo`, `NeedsRepair`, and `Toggle`,
      all pure. Verified: the tests below.
- [x] 1.2 The registry calls in `Native.cs` and one thin wrapper that reads,
      writes, and removes the value. Verified: the smoke reads the value back
      with `reg query`.

## 2. The panel

- [x] 2.1 `PanelActionRow` and `PanelHit.Autostart`, with the footer drawn as two
      full-width rows. Verified: the row tests.
- [x] 2.2 Hover and hit test for both rows, so the second control behaves like
      the first. Verified: the hover test.
- [x] 2.3 Clicking the row toggles the registration, repaints with the new value,
      and leaves the panel open. Verified: the toggle test and the smoke.

## 3. The wiring

- [x] 3.1 At startup, repair the registration when it is on and stale.
      Verified: the follow test, plus a smoke that rewrites a stale value on
      launch.
- [x] 3.2 `README.md`, `CONVENTIONS.md`, `ARCHITECTURE.md`, and the changelog.
      Verified: the install story is one click, and no document says autostart
      is missing.

## 4. Gates and finish

- [x] 4.1 `bash scripts/verify.sh`, then the smoke: toggle on, read the registry,
      toggle off, read it again. Verified: both readings in the archive commit.
- [x] 4.2 `review-change` with the security checklist for the new write path.
      Verified: every finding fixed or recorded.
- [x] 4.3 Archive, commit, merge. Verified: the delta updates the panel
      requirement and adds the autostart one.
