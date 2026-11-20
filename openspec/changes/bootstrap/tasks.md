# Tasks

## 1. Decisions

- [x] 1.1 Write `adr/0001-native-tray-fed-by-pi-extension.md` with the stack, the
      rejected options, and the consequences. Verified: the file follows the MADR
      sections, `Status: Proposed`, and `adr/README.md` carries its row.
- [x] 1.2 Run the threat model and write `threat-model.md` in this folder.
      Verified: every threat names a path and an asset, and the one control a
      machine can test is listed at the bottom.
- [x] 1.3 Write `proposal.md` with the level on the title line, the routing
      answers, the ten interview answers, the assumptions with revisit triggers,
      and the out-of-scope list. Verified: `openspec validate bootstrap` and
      `bash scripts/check-change.sh` accept it, and the first line names L2.
- [x] 1.4 Write `design.md`: the scaffold layout, the state contract, and what is
      deferred. Verified: every path it names exists or is created by a task
      below.

## 2. Scaffold

- [ ] 2.1 Create `trayclaw.sln`, `src/Trayclaw/Trayclaw.csproj` (net10.0,
      WinExe, win-x64, `PublishAot`, no package references), and
      `tests/Trayclaw.Tests/Trayclaw.Tests.csproj` (xunit). Verified:
      `dotnet build trayclaw.sln -c Release` succeeds from a clean `obj/`.
- [ ] 2.2 Implement `Native.cs` and `Program.cs`: message-only window, one tray
      icon with a tooltip, a right-click menu with Quit, and a message loop that
      removes the icon on exit. Verified: `dotnet run --project src/Trayclaw`
      leaves an icon in the notification area, and Quit removes it and exits
      with code 0.
- [ ] 2.3 Implement `SessionRef` (encode and decode `<pid>-<session>.json`,
      sanitizing the session id to `[A-Za-z0-9-]`). Verified: it is covered by
      the tests in 2.4 rather than by inspection.
- [ ] 2.4 Add the tests, including one per hostile session id (`..`, `/`, `\`,
      `:`), each asserting the produced name stays inside the state directory
      and decodes back. Verified: `dotnet test trayclaw.sln -c Release` passes and
      the hostile-id tests are named for the case they cover.
- [ ] 2.5 Run `dotnet format trayclaw.sln --verify-no-changes` and fix what it
      reports. Verified: it exits 0, which is what Project facts will name as
      the lint.

## 3. Product documents

- [ ] 3.1 Replace `README.md` with the product: what it is, who it is for, what
      it is not, how to build and run it, and where the next change starts.
      Verified: no template wording survives, and every command in it is one
      that task 2 ran.
- [ ] 3.2 Fill **What this is** in `AGENTS.md` with the same paragraph, and fill
      every line of Project facts: slug, stack, toolchain, trunk, template
      source, revision, date, lint, build, test, run, secrets, environments.
      Verified: `scripts/check-change.sh` reports no unfilled Project facts, and
      each command line is one bare command.
- [ ] 3.3 Write the first `CONVENTIONS.md` entries: formatter, error handling,
      tests, dependencies, and the ignore policy. Verified: each entry names the
      command or setting that proves it, and none contradicts `AGENTS.md`.
- [ ] 3.4 Add `.editorconfig`, extend `.gitignore` with `bin/` and `obj/`, and
      record the lockfile policy in `CONVENTIONS.md`. Verified: a fresh clone
      builds without a committed build output, and `git status` stays clean
      after `dotnet build`.
- [ ] 3.5 Write `ARCHITECTURE.md`: C4 context and container views for the pi
      process, the extension, the state directory, and the tray app. Verified:
      it names only containers that exist in this change or are marked as the
      next one, and `architecture-doc` has been read before writing it.
- [ ] 3.6 Add the first `CHANGELOG.md` entry under `## Unreleased`, in the words
      of what the user can now do. Verified: the heading is present, and the
      entry names no version.
- [ ] 3.7 Leave the CI and Deploy blocks empty and say why in the proposal.
      Verified: the check does not warn about a half-filled block, because the
      blocks are empty rather than partial.

## 4. Gates and finish

- [ ] 4.1 Run `bash scripts/verify.sh`. Verified: it reports the invariant
      checks passing and runs lint, build, and test, and the output is quoted in
      the archive commit.
- [ ] 4.2 Run `review-change` over the diff, with the security checklist for the
      path handling in `SessionRef`. Verified: every finding is either fixed or
      written down as accepted.
- [ ] 4.3 Archive the change, commit `feat` and `chore: archive`, and merge the
      branch without squashing. Verified: `openspec list` shows no active change,
      the specs directory stays empty, and `git log` on trunk carries the three
      commits.
