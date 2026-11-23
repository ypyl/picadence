# Tasks

- [x] 1.1 Ask the kernel when the process behind a pid started, and compare it with
  the state file's write time. Verified by `SessionStore.StartedBefore` and its
  theory over the tolerance.
- [x] 1.2 A session whose file was written before its process started is treated as
  gone, and its file is removed. Verified by a store test with a reused id.
- [x] 1.3 Record the evidence in `REPO-FACTS.md`: the id, the handle, and what
  `Get-Process` could not see.
- [x] 1.4 Run `bash scripts/verify.sh`, then watch the stale row go.
