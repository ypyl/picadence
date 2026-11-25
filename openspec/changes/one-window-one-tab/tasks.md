# Tasks

- [x] 1.1 `Panel.Launcher` takes the session id and the plan.
- [x] 1.2 `Program` records the start stamp when a tab really started, and both callers go through it.
- [x] 1.3 `Panel.RestartRow` passes the row's session id.
- [x] 1.4 The source-shape test in `RestartTests` guards the single place: one stamp write, one start call, and the panel's one launcher call carries the session id. It failed before this change.
- [x] 1.5 Changelog entry under `## Unreleased`.
- [x] 1.6 Run `bash scripts/verify.sh` and record the counts.
