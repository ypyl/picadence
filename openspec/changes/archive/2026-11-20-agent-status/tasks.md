# Tasks

## 1. The pi side

- [x] 1.1 Write `extensions/trayclaw.js`: the reducer, the file name, the atomic
      write, and the event wiring from the design table. Verified: it loads in
      node with no build step, and every write goes through a temp file and a
      rename.
- [x] 1.2 Delete the file on `session_shutdown`, and gate reporting to the root
      TUI session so a headless or child session writes nothing. Verified: the
      clean-exit and mapping tests below.
- [x] 1.3 Write `extensions/trayclaw.test.mjs` under `node --test`, covering the
      lifecycle mapping, the outcome rule for `done` and `error`, the aborted
      case, and the clean exit. Verified: `node --test extensions` passes.

## 2. The tray side

- [x] 2.1 `AgentState`, `AgentSession`, and `StateFile`: the state names, the
      record, and `JsonDocument` parsing that ignores unknown fields and rejects
      a body without a usable state. Verified: the parse tests.
- [x] 2.2 `AgentStatus`: the aggregate colour, the tooltip, and the menu rows,
      including age formatting. Verified: the aggregation, tooltip, and menu
      tests, one per scenario title.
- [x] 2.3 `SessionStore`: enumerate the directory, drop and delete entries whose
      process is gone, and never write anything else. Liveness is a delegate, so
      a test can kill a session without a process. Verified: the ended-session
      test, which also asserts the surviving file is untouched.
- [x] 2.4 `IconPixels`: a pure 16x16 buffer per state, and the Win32 call that
      turns it into an icon. Verified: the pixel test asserts a distinct colour
      per state and a circle mask that is set in the middle and clear in the
      corners.
- [x] 2.5 Wire it into `Program.cs`: a one-second timer, the tooltip, the menu,
      the icon swap, and a rebuild only when the content changed. Verified: the
      runtime smoke below.
- [x] 2.6 Test that the interop class names no networking library, so the
      no-network promise is a check and not a comment. Verified: that test.

## 3. Documents

- [x] 3.1 Update Project facts: `Test` runs both suites, and add the extension
      install command where a new session will find it. Verified:
      `bash scripts/verify.sh` runs both.
- [x] 3.2 Update `README.md`: what the icon means, how to install the extension,
      and what a session started before the install does. Verified: the command in the README is
      the one `pi install` documents; it was not run, because installing into the
      live pi profile is the human's call.
- [x] 3.3 Refresh `ARCHITECTURE.md` with the extension, the state directory, and
      the pi process, which are containers now. Verified: `architecture-doc`
      shapes, and every element has a path in the repository.
- [x] 3.4 Add the `CHANGELOG.md` entry and the `CONVENTIONS.md` rule for the
      extension's plain JavaScript and its test command. Verified: the changelog
      heading is intact and names no version.

## 4. Gates and finish

- [x] 4.1 Run `bash scripts/verify.sh`, then the tray by hand with one session
      writing state, and check the icon, the tooltip, and the menu. Verified: the
      output of both, quoted in the archive commit.
- [x] 4.2 Run `review-change` with the security checklist for the new file reads
      and the delete path. Verified: every finding fixed or recorded.
- [x] 4.3 Archive, commit `feat` and `chore: archive`, and merge without
      squashing. Verified: no active change, the delta spec becomes
      `openspec/specs/agent-status/spec.md`, and trunk carries the three commits.
