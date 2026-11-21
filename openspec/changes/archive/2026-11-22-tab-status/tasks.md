# Tasks

- [x] 1.1 Add `tabTitle(state, label)` to `extensions/picadence.js`: `●`, `▲`,
  `✖`, `✓` before the label, nothing when idle. Verified by one test per state,
  and by a test that an empty label never produces a bare mark.
- [x] 1.2 Call `ctx.ui.setTitle(...)` in `flush()`, inside the same guard that
  keeps a reporting failure away from the session. Verified by the extension test
  that drives a fake pi through a transition and reads the title it was given, and
  by the existing test that a console run reports nothing.
- [x] 1.3 Add `tabProgressSequence(state)` and write it only when
  `PICADENCE_TAB_PROGRESS=1`, so the unproven channel cannot surprise anyone who
  has not asked for it. Verified by tests on the exact sequences, and by a session
  with the variable unset writing nothing.
- [x] 1.4 Update `README.md`: what the tab shows, the `PICADENCE_TAB_PROGRESS`
  switch, and the two Terminal behaviours that can override the title.
- [x] 1.5 Run `bash scripts/verify.sh` and record both suites in the archive
  commit.
