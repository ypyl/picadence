# Tasks

- [x] 1.1 Add `LICENSE` (MIT) and one line in `README.md` pointing at it.
  Verified by the file being present and by GitHub detecting it on the repository
  page after the push.
- [x] 1.2 Move the `shortcut:` marker into `.github/workflows/ci.yml`, above the
  step it is about, and leave the fact itself in `REPO-FACTS.md` without the
  marker prefix. Verified by `scripts/check-change.sh`, which reads the markers
  and refuses one with no upgrade trigger.
- [x] 1.3 Run `bash scripts/verify.sh` and record both suites in the archive
  commit.
