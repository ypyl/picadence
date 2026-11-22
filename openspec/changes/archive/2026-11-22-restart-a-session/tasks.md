# Tasks

## 1. The pin

- [x] 1.1 `Pin` and `PinStore`: one file per session id under `pins/`, written when
  the user keeps a session and deleted when they stop. A corrupt file is ignored,
  never repaired. Verified by `PinStoreTests`, including a fresh store reading what
  another instance wrote.
- [x] 1.2 Transcript lookup: search the subdirectories of pi's `sessions` directory
  for `*<session-id>.jsonl`, newest wins. Verified by a test that plants the file in
  a subdirectory it did not choose.

## 2. The restart

- [x] 2.1 `Launch`, `RestartPlan.TryBuild`, and the refusal enum: running, no
  conversation, no folder, no terminal. Pure, one test per refusal.
- [x] 2.2 The quoting function, with a folder containing a space and a trailing
  backslash. Verified by asserting the exact argument list, not the joined string.
- [x] 2.3 Title sanitising: control characters and quotes cannot reach it. Verified
  by a session name containing a newline and an escape character.
- [x] 2.4 Resolve `wt.exe` (the WindowsApps alias, then `PATH`) and start it with
  `CreateProcessW`, `CREATE_NO_WINDOW`, no wait, handles closed. Verified by the
  injected launcher in tests and by one real restart on this machine.

## 3. The panel

- [x] 3.1 Rows carry a kind, whether a restart is offered, and the reason when it
  is not. Kept rows come after live rows, oldest first. Verified by `PanelTests`.
- [x] 3.2 The flyout becomes a short menu: cadences and `Keep` for a live row,
  `Restart` and `Stop keeping` for a kept row that is not running. Verified by the
  item list per row kind.
- [x] 3.3 Wire the actions in `Program`: keep, stop keeping, restart. A restart
  that fails to start says so in the row for one tick. Verified by the injected
  launcher.

## 4. The record

- [x] 4.1 `AGENTS.md`, "What this is": the tray may start one tab, for a session
  the user kept, and never anything else.
- [x] 4.2 `README.md`: what keeping a session does, and the one thing it starts.
- [x] 4.3 `CHANGELOG.md`: one entry for the user.
- [x] 4.4 `bash scripts/verify.sh`, then the human check: pin a session, close its
  tab, restart it from the panel, confirm the conversation returns.
