# The tray shows what each pi session is doing (L1)

## Why

The tray icon exists and knows nothing. `bootstrap` left it with a menu that says
Quit and a tooltip that says no agent state yet, because the contract between pi
and the tray was designed but not built. Until it is built, the product does not
do the one thing it is for: telling you which tab needs you without an alt-tab.

## What Changes

- The tray icon gains a state: red for a failed run, amber for a session waiting
  on you, green for a session working, grey when nothing needs you.
- The tooltip counts the live sessions by state.
- The menu lists each live session with its folder, its state, and how long it
  has been there.
- A session whose process has ended stops being shown, and its state is removed.
- A pi extension reports `idle`, `working`, `blocked`, `done`, and `error` on
  pi's lifecycle events and removes its state when pi exits.
- The icon is painted per state instead of reusing a stock Windows icon.
- `ARCHITECTURE.md` gains the extension and the state directory, which now
  exist.
- No breaking change: nothing consumed the previous behavior.

## Capabilities

- **New**: `agent-status`. The whole of what the product promises lives here:
  what state each session is in, how the tray aggregates it, and how fast it
  reacts.
- **Modified**: none.

## Impact

- `src/Trayclaw`: new `AgentState`, `AgentSession`, `AgentStatus`,
  `SessionStore`, `StateFile`, `IconPixels`, `Age`, `MenuModel`; `Program.cs`
  gains a timer, the tooltip, the menu content, and the icon swap.
- `extensions/trayclaw.js`: new, the pi side. `extensions/trayclaw.test.mjs`:
  new, runs under `node --test`.
- `tests/Trayclaw.Tests`: one test per scenario, mostly on the pure functions.
- Project facts: `Test` becomes two commands, the .NET suite and the extension
  suite.
- `ARCHITECTURE.md`, `README.md`, `CHANGELOG.md`, `CONVENTIONS.md`.
- Not changed: the ADR. This change builds what ADR 0001 decided; it does not
  decide anything new.
- The contract another change could break: the state file, now real. Its shape
  is in `design.md`, and both halves live in this repository, so it moves with
  them.

## Routing

0. **Does observable behavior change?** Yes: the indicator, the tooltip, and the
   menu gain content, and a session's state reaches the screen.
1. **Can it be undone easily?** Yes. One commit, no stored data, nothing
   published. A state file left on disk is deleted by the next run.
2. **Can a machine prove it correct?** Yes. The aggregation, the tooltip, the
   menu, the staleness rule, the state derivation, and the icon colour are pure
   functions with tests, and the two-second budget is a constant a test reads.
3. **Does anything outside the change depend on it?** No. The state file is used
   only by these two halves, both in this repository. It is not a published
   contract.

Level L1: behavior moves, a machine can prove it, and nothing outside depends on
it. Not L2, because no dependency, no service, and no decision changes.

## Out of scope

- Toasts, autostart, an installer, a settings file, theming, and clicking a
  session to focus its tab.
- Sessions started before the extension is installed. They report nothing until
  pi restarts, which the README will say.
- `error` for a failure that pi retries and then repairs: only a run that ends in
  error is reported, which is the same rule pi uses for its terminal status.
- Any agent other than pi, and any platform other than Windows.

## Approval

- Gate: L1 delta spec
- Approved by: Yauhen Pyl
- Approved at: 2026-11-20
- Artifact: openspec/changes/agent-status/specs/agent-status/spec.md sha256:67381f3b8265bbef9b48b07856b8a0ce4ecb3164a27e417b2f6652992c13354e
- Artifact: openspec/changes/agent-status/proposal.md sha256:602d45b638dabdbe8535bb2bbdb02c60bc5ef5371afd1bf26cc7b766b680c258
- Words: "(none)"
