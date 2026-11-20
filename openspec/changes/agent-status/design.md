# Design

Builds what `adr/0001-native-tray-fed-by-pi-extension.md` decided. The state
names are pi's own, from the terminal status protocol it already emits, so the
vocabulary does not have to be learned twice.

## The state file

`%LOCALAPPDATA%\trayclaw\agents\<pid>-<sessionId>.json`, one per session, written
by the extension in the pi process and read by the tray.

```json
{
  "pid": 12345,
  "sessionId": "01a1fa93-ff0c-71eb-afd8-f7959610ae72",
  "folder": "trayclaw",
  "cwd": "C:\\Users\\ypyl\\projects\\trayclaw",
  "name": "trayclaw bootstrap",
  "model": "opencode-go/deepseek-v4.1-flash",
  "state": "working",
  "detail": "edit",
  "updatedAt": "2026-11-20T21:10:03.512Z"
}
```

- `state` is one of `idle`, `working`, `blocked`, `done`, `error`.
- `detail` is a short label: the tool name while working, the prompt title while
  blocked. It is shown in the menu after the state, never as markup.
- Everything except `pid`, `sessionId`, and `state` is optional. The tray ignores
  a field it does not know, and ignores a file it cannot read.
- Written to a temporary file in the same directory and renamed over the target,
  so a reader never sees half a file. The extension writes on every transition
  and on nothing else.

## The extension

`extensions/trayclaw.js`, plain ESM so the same file can be tested by
`node --test`. It exports the reducer, and installs itself through the default
export.

| pi event | state written |
|---|---|
| `session_start` (mode `tui` only) | `idle` |
| `agent_start` | `working` |
| `turn_start`, `tool_execution_start` | `working`, with the tool name in `detail` |
| `ui_prompt_start` | `blocked`, with the prompt title in `detail` |
| `ui_prompt_end` | back to `working` if a run is active, otherwise `idle` |
| `agent_before_settle` | remembers `event.outcome`: `completed`, `aborted`, `error` |
| `agent_settled` | `idle` when aborted, otherwise `error` or `done` from the outcome |
| `session_shutdown` | the file is deleted |

`agent_settled` is used rather than `agent_end` because pi may retry, compact,
and continue after `agent_end`; only `agent_settled` means pi will not continue
on its own. `agent_before_settle` is where the outcome is readable, which is why
the last outcome is remembered across those two events.

Only the root TUI session reports. A reload replaces the extension instance, and
`session_start` with `reason: "reload"` re-reports, so a reload does not leave a
stale state.

## The tray

Every second, one pass over the state directory:

1. Enumerate `*.json`, parse the name with `SessionRef`, read and parse the body
   with `JsonDocument`.
2. Drop a file whose `pid` is not a running process, and delete it. Liveness is
   `OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION)`.
3. Aggregate what remains, update the tooltip and the icon colour, and rebuild
   the menu only when its content changed, so an open menu is not reset every
   second for nothing.

The pass is one directory read of a handful of small files. On this machine the
whole process measured 10.35 MB working set and 3.11 MB private bytes while doing
exactly this.

`shortcut: poll every second instead of watching the directory, switch to a change notification when a machine runs more than about twenty sessions`

## Icon

Four states, four colours, painted into a 16x16 bitmap and turned into an icon
with `CreateIconIndirect`: red `#DC2828`, amber `#E6A000`, green `#28B450`, grey
`#969696`. The pixel function is pure and tested; only the Win32 wrapper around
it is not.

Stock Windows icons were rejected: they differ by shape, not by colour, and the
colour is the whole point of a glance.

## Testing split

- Pure and tested: state derivation in the extension (`node --test`), and state
  parsing, aggregation, tooltip, menu rows, age formatting, icon pixels, and the
  interop allowlist in C#.
- Not tested, and thin by design: the Win32 calls and the message loop, which the
  runtime smoke covers.

## Deferred

Toasts, autostart, focusing a session, per-session rows that can be clicked, and
history. None of them changes the contract above.
