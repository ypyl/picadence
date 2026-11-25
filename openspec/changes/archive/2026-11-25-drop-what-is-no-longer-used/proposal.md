# Drop what earlier releases stopped using (L0)

## Why

Four releases removed features faster than they removed the code behind them, and
one fact went stale the moment 0.3.0 was tagged. Autostart moved to the Startup
folder, so five registry imports and four constants have no caller. The X moved
into the header, so the action row's height and padding have no reader. A hit
result and a hit test branch describe a control that no longer exists, and
`SessionStore.ReadAll` still takes the timestamp from when liveness was decided by
age. The tray's start stamp table keeps one entry for every session it has ever
started. And `package.json` says 0.2.0 while 0.3.0 shipped.

Nothing here changes what a person sees. The reason to do it is that the interop
test is the product's smallest promise, "no network, only these libraries", and it
currently allows `advapi32.dll` on behalf of code that cannot run.

## What Changes

- Delete the five `advapi32` declarations, `SW_SHOWNA`, `DT_WORDBREAK`, `NIF_TIP`,
  and `CommandQuit` from `Native`.
- Delete `PanelHit.Autostart` and the hit test branch that returns it, and the
  action row's three unused constants and the unused `IconSize` from
  `PanelModel`.
- Drop the `now` parameter `SessionStore.ReadAll` never reads.
- Prune `Program`'s start stamps once they are older than the grace they exist
  for, so the table is bounded by the sessions started in the last half minute
  rather than by every session the tray has ever started.
- `package.json` names the release it ships, 0.3.0, and Project facts says that
  the field moves with the tag.

## Capabilities

None. No requirement moves: the tray shows the same rows, the same menu, and the
same notices, and prints the same icon, so `skip_specs: true`.

## Impact

- `Native`, `PanelModel`, `SessionStore`, `Program`, `package.json`, and
  `AGENTS.md` Project facts.
- Two test files: the interop allow list loses `advapi32.dll`, and the session
  store tests lose an argument.

## Routing

0. **Does observable behavior change?** No. Every deletion is a declaration with no
   caller, checked by grep across source, tests, and fixtures.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes: the build treats warnings as errors, the
   181 tests cover the code that stays, and the interop test now fails if anything
   registry-shaped comes back. The prune is small enough to read: it removes a
   stamp only after the tick that could have used it.
3. **Does anything outside the change depend on it?** No. Nothing reads the deleted
   members, and the manifest's version is read by pi's package install, which is
   why it moves to the shipped release rather than away.

Level: **L0**.
