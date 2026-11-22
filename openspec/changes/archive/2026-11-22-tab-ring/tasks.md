# Tasks

- [x] 1.1 Add `wantsTabProgress(env)`: off when the switch says off, on when it
  says on, otherwise on only for Windows Terminal. Verified by a test per
  combination, including the two override cases.
- [x] 1.2 Use it where the sequence is written today, instead of the bare
  "must be 1" check. Verified by an extension test that drives a transition with
  the switch on and reads the bytes, and one with it off that reads nothing.
- [x] 1.3 Add the two `CHANGELOG.md` entries the tab feature was missing: the mark
  on the tab, and the ring Windows Terminal draws from it.
- [x] 1.4 Update the README: the ring is on in Windows Terminal, `=0` turns it
  off, `=1` forces it elsewhere, and pi's own progress switch collides with it.
- [x] 1.5 Run `bash scripts/verify.sh` and record both suites in the archive
  commit.
