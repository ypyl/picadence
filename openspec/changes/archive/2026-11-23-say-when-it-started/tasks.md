# Tasks

- [x] 1.1 The extension writes `startedAt` from uptime, on every write. Verified by a
  test over the derivation, including a missing uptime.
- [x] 1.2 The tray parses it and compares it with the kernel's creation time, with
  two seconds of slack. Verified by a theory either side of the slack.
- [x] 1.3 A file without the field falls back to the write-time rule. Verified by a
  store test whose file has no `startedAt`.
- [x] 1.4 `REPO-FACTS.md` records why the write time was not enough.
- [x] 1.5 Run `bash scripts/verify.sh`, then watch the `folio` row go.
