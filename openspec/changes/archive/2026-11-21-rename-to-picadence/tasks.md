# Tasks

Every task says how it is checked. The whole change lands in one commit: a rename
has no useful intermediate state.

## 1. Code

- [x] 1.1 Rename the solution to `picadence.sln`, `src/Piclaw` to
  `src/Picadence`, `tests/Piclaw.Tests` to `tests/Picadence.Tests`, the project
  files with them, and every namespace and `InternalsVisibleTo`. Verified by
  `dotnet build picadence.sln -c Release` with no warnings.
- [x] 1.2 Rename `extensions/piclaw.js` and its test to `picadence.js` and
  `picadence.test.mjs`, the package name, and the extension's own state and ping
  directories. Verified by `node --test extensions/picadence.test.mjs`.
- [x] 1.3 Rename the window classes, the panel title, the run value name and the
  executable's own name. Verified by the interop test, which asserts the window
  class, and by launching the published exe.
- [x] 1.4 Rewrite the wake-up message to say `picadence ping`, and rename the
  custom message type to `picadence-ping`. Verified by the test now named `the
  wake-up names picadence and the cadence and gives no instruction`, which
  asserts the exact text.

## 2. The contract between the halves

- [x] 2.1 Point both halves at `%LOCALAPPDATA%\picadence`, and leave the old
  directory alone rather than deleting it. Verified by the state, schedule and
  ping directories appearing under the new name when the published exe runs, and
  by a test that the extension computes the new directory.

## 3. Documents

- [x] 3.1 Update `AGENTS.md`: the Product slug, and the four commands in Project
  facts that name the solution or the project directory. Verified by running
  each one.
- [x] 3.2 Update `README.md`, `CHANGELOG.md` under `## Unreleased`, `DESIGN.md`,
  `ARCHITECTURE.md`, `REPO-FACTS.md` and `CONVENTIONS.md` where the product is
  named. Verified by `grep -ri piclaw` naming only the archive and the accepted
  ADRs.
- [x] 3.3 Run `bash scripts/verify.sh` and record the commands and results in
  the archive commit.

## Workflow follow-up

- Approve the spec gate, write the approval block into `proposal.md`, then run
  the loop: apply, verify, archive, commit `plan`, `feat` and `chore: archive`,
  merge, delete the branch.
- Publish and restart the tray so the running copy is `picadence.exe`.
