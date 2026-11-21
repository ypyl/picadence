# Tasks

- [x] 1.1 Cache the parse of each state file in `SessionStore`, keyed by name,
  length and last write time, and drop the entry when the file is gone. Verified
  by three tests: `an unchanged state file is not read again` for the cache hit,
  a rewrite for the miss, and a touched write time for the other half of the key.
- [x] 1.2 Keep the existing store tests green: a dead session is still removed, a
  file that cannot be parsed is still ignored, and a missing directory still reads
  as nothing. Verified by `dotnet test piclaw.sln -c Release`.
- [x] 1.3 Measure the published exe with the same five sessions: nine minutes
  without interference, and a build that forces a full collection so the live
  heap can be read. There is no plateau and no retention; record the drift, the
  live heap, and what a full collection returns, in `REPO-FACTS.md`. Verified by
  the samples themselves.
- [x] 1.4 Correct the memory sentence in `README.md` to the measured steady
  state. Verified by reading it against the numbers in `REPO-FACTS.md`.
- [x] 1.5 Run `bash scripts/verify.sh` and record the outcome in the archive
  commit.
