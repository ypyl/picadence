# A tray indicator for pi agents (L2)

## Why

pi sessions run in terminal tabs, and nothing outside those tabs can tell you
which one finished, which one needs an answer, and which one died. You either
alt-tab through every tab or leave work waiting. The information already exists
inside pi; it just has no way out of the terminal.

The proposed answer is a small tray indicator, and it is the first change in a
fresh copy of the template: this is bootstrap. It also settles the stack, since
the RAM budget decides it.

## What Changes

- A Windows tray application scaffold: one process, one icon, one menu, no GUI
  toolkit.
- A test project, so the repository has a test command from the first change.
- The product README, `CONVENTIONS.md` entries, `.editorconfig`, the ignore
  policy, and the first changelog entry.
- `adr/0001-native-tray-fed-by-pi-extension.md`, which decides the stack and the
  way agent state leaves pi.
- **BREAKING**: none. Nothing shipped before this change.

No behavior ships in this change. The tray icon appears and quits; it shows no
agent state. The state contract is designed here and built in the next change.

## Capabilities

- **New**: none. This change ships no observable behavior, so `skip_specs` is
  set and the durable spec directory stays empty.
- **Modified**: none.

The next change introduces the `agent-status` capability and with it the first
delta spec: which states exist, what the tray shows for each, and how fast it
reacts.

## Impact

- New: `src/Trayclaw`, `tests/Trayclaw.Tests`, `trayclaw.sln`, `ARCHITECTURE.md`.
- New dependencies: xunit and the .NET test SDK, test-only. The tray itself has
  no package dependency.
- Rewritten: `README.md`, `CONVENTIONS.md`, Project facts and **What this is** in
  `AGENTS.md`, `CHANGELOG.md`, `adr/README.md`, `.gitignore`.
- Untouched: the process files under `.agents/skills`, `.githooks`, and
  `scripts`, which stay byte-identical to the template revision.
- The contract document another change could break: none outside this repository
  yet. The state file under `%LOCALAPPDATA%\trayclaw` is the first one, and
  `design.md` fixes its shape.

## Routing

0. **Does observable behavior change?** No. Nothing observes an agent yet. The
   work is scaffolding, documents, and a decision.
1. **Can it be undone easily?** Yes. One commit, no data, no published contract,
   no migration.
2. **Can a machine prove it correct?** Yes. `scripts/check-change.sh` and
   `scripts/verify.sh` run over the whole repository, lint, build, and tests
   included, and the latter must pass on an empty scaffold for the change to
   finish.
3. **Does anything outside the change depend on it?** Yes, and this is what
   lifts it above L1: the stack decision and the state contract bind every later
   change. The ADR is the contract document.

Level L2: the highest answer wins, and a stack choice is L2 by the rules in
`AGENTS.md`. `skip_specs: true`, because behavior is unchanged.

## Answers

| # | Question | Answer |
|---|---|---|
| 1 | What does it do | A small app that "sits in tray and monitor my pi code agent which are run in terminal tabs", "ideally consuming less than 10MB ram". |
| 2 | Who is it for | One person, on this machine. No other user in the first year. |
| 3 | What it is not | Not an orchestrator: it never sends input, starts, or stops a tab. Not cross-platform. No history, analytics, web UI, or cloud. |
| 4 | What data, and where | Process id, session id, working directory, session name, model, current tool, state, and timestamps. Project folder paths are the only mildly personal part. It lives in `%LOCALAPPDATA%\trayclaw`, never in the repository. No prompts, no model output, no file contents. |
| 5 | Load and users | One tray process, 5 to 15 sessions, a few state changes per second at peak. One user in the first year. |
| 6 | Platform | Windows 11 desktop, tray, one exe, per user, no installer. |
| 7 | Stack preference | "Up to you." Recorded as a decision in `adr/0001-native-tray-fed-by-pi-extension.md`: C# 10 with NativeAOT, Win32 directly, plus a pi extension in TypeScript. |
| 8 | Where it runs | This machine only. No server, no cloud, no network call of any kind. |
| 9 | CI | None yet, because there is no remote. Both pipeline blocks stay empty; GitHub Actions running `scripts/verify.sh` is the candidate when a remote exists. |
| 10 | Secrets | None. The halves exchange local files, so there is no credential to hold, locally or in CI. |

## Assumptions

Each one was taken instead of an answer, and each names the trigger that makes it
a question again.

| # | Assumption | Revisit when |
|---|---|---|
| A1 | Product slug `trayclaw`, from the folder name. | Never, unless the product is renamed; then the release tags change with it. |
| A2 | Trunk is `main`. | A remote or a team convention says otherwise. |
| A3 | The tray shows an aggregate dot, a per-session tooltip, and a menu. No toasts in the first version. | The dot proves too quiet, so a finished session goes unnoticed. |
| A4 | The pi extension is installed per user, outside this repository, and the tray app is started by hand. No autostart, no installer. | The human asks for one, or the second machine appears. |
| A5 | State files live in `%LOCALAPPDATA%\trayclaw`. | A second agent host needs the same status, or the files must be shared with another tool. |
| A6 | Rust was rejected on the strength of the measured C# numbers in the ADR, not on a Rust measurement. | The C# build exceeds 10 MB private bytes in the finished product. |

## Out of scope

- Reading anything from pi (the extension and the state contract are the next
  change).
- Toasts, autostart, an installer, a settings file, theming.
- Mapping a session to a specific terminal tab. Windows Terminal exposes no such
  handle; the menu shows the working directory and session name instead.
- Any agent other than pi, any platform other than Windows.
- A remote, CI credentials, and deploy credentials, which stay the human's.

## Approval

- Gate: L2 ADR
- Approved by: Yauhen Pyl
- Approved at: 2026-11-20
- Artifact: adr/0001-native-tray-fed-by-pi-extension.md sha256:1282776da42e2ee4a23be6ff8c1c0d7d3b5dfcb4b984462b064acf8f926c8d38
- Artifact: openspec/changes/bootstrap/proposal.md sha256:fa3d3cbc0beeee88aa07a79c473b8caef398ddbee28e7d73e6fda5e9eb9b3586
- Words: "(none)"
