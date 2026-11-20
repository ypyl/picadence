# Tasks

## 1. Inside the repository

- [x] 1.1 Replace the name in every file outside `openspec/changes/archive`:
      namespace, project files, window classes, tooltip, panel title, state
      directory, extension, tests, and documents. Verified: a search for
      `trayclaw` outside the archive returns nothing.
- [x] 1.2 Rename the paths: `src/Piclaw`, `tests/Piclaw.Tests`, `piclaw.sln`,
      `extensions/piclaw.js`, `extensions/piclaw.test.mjs`. Verified:
      `dotnet build piclaw.sln` and `node --test extensions/piclaw.test.mjs`.
- [x] 1.3 Run lint, build, format, and both test suites. Verified: the commands
      and their output go into the archive commit.

## 2. On this machine

- [ ] 2.1 Stop the running tray, remove the old pi package entry, delete the old
      state directory. Verified: `pi list` no longer names the old path.
- [ ] 2.2 Rename the repository folder to `piclaw`. Verified: `git status` clean
      in the new folder.
- [ ] 2.3 Install the extension from the new path and start the tray. Verified:
      the tooltip and the panel say piclaw, and the state directory is
      `%LOCALAPPDATA%\piclaw`.

## 3. Finish

- [x] 3.1 `bash scripts/verify.sh`, changelog entry, archive, and merge.
      Verified: the check passes and the archive carries the result.
