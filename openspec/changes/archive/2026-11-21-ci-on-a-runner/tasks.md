# Tasks

- [x] 1.1 Write `.github/workflows/ci.yml`: the three triggers, `windows-latest`,
  the hook path, the OpenSpec CLI at the version Project facts names,
  `bash scripts/verify.sh`, the AOT publish with a placeholder version, and the
  artifact. Verified by `scripts/check-change.sh`, which reads the CI block
  against the workflow file, and by the run itself.
- [x] 1.2 Push the branch and let the run happen. Read the whole log, not the
  conclusion: this is the first time this repository has been checked anywhere but
  this machine. Verified by a green run, or by the list of what it found.
- [x] 1.3 Fix what the run finds, in this change if it is plumbing, or in its own
  change if it is the product. Verified by a second run.
- [x] 1.4 Fill the `CI` block in `AGENTS.md` with the four commands actually run
  against this workflow, in the form that worked. Verified by running each one
  again.
- [x] 1.5 Record the first run in `REPO-FACTS.md`: the image, the SDK it shipped,
  the wall clock, and anything the runner does differently from this machine.
- [x] 1.6 Run `bash scripts/verify.sh` locally and record both results in the
  archive commit.

## Workflow follow-up

- Approve the ADR gate and write the approval block into `proposal.md`.
- Archive, commit `feat` and `chore: archive`, push the branch, let the run finish,
  put the run URL and its conclusion into the archive commit, then merge on green.
