# 0001. A native tray app fed by a pi extension

- Status: Accepted
- Date: 2026-11-20

## Context

The product is a Windows tray indicator that shows what the pi coding agents in
terminal tabs are doing, and which one needs attention. It must feel free to
leave running, so the whole process should stay near 10 MB, and it must work
with pi as it already ships.

Two constraints shaped the decision. pi already computes the exact states needed
(`working`, `blocked`, `done`, `error`, `idle`) and reports them to the terminal
over OSC 7501, but a third process cannot read that stream: Windows Terminal
owns the pane buffer, and tmux and screen do not forward the sequence. The other
route, inferring state from the session files under `~/.pi/agent/sessions`, sees
"recent activity" but cannot tell "waiting for you" from "the tab was killed
yesterday".

Measured on this machine, a tray process with one icon and a 2 s poll: C# 10
treated with NativeAOT is an 870 KB exe at 3.1 MB private bytes and 11.0 MB
working set; the same code without AOT is 6.2 MB private and 24.5 MB working
set. WinForms and WPF cannot be published with NativeAOT, so their floor is the
higher one.

## Decision

We will build two halves in this repository. A C# 10 NativeAOT Windows
executable owns the tray icon and the UI, using Win32 directly rather than a GUI
toolkit. A pi extension in TypeScript observes pi's own lifecycle events and
writes one small JSON file per session, named by process id and session id,
under `%LOCALAPPDATA%\trayclaw`. The tray app reads that directory, treats a dead
process id as a session that is gone, and never talks to the network.

## Consequences

The tray stays in the low single-digit megabytes and starts in milliseconds. pi
gains a few hundred kilobytes because the extension runs inside a Node process
that is already loaded. State is exact rather than inferred, and the "needs you"
state works because pi raises `ui_prompt_start` and `agent_settled`.

The cost is that pi must load the extension, so a pi started without it shows
nothing, and the product only covers pi, not other agents. Raw Win32 interop
replaces a GUI framework: no designer, no data binding, and a toast needs more
work than a framework would need. The state file is a contract between two
components in this repository, so changing its shape changes both, and a stale
file left by a crash must be reaped by process liveness rather than trusted.

## Alternatives

- Rust with `windows-sys`: a second toolchain and the same unsafe Win32 surface,
  for 2-3 MB private bytes, a saving the running pi sessions make irrelevant.
- C# with WinForms or WPF: fastest to write, 30-60 MB, and NativeAOT is not
  supported, so the RAM budget is lost.
- Tauri: a WebView2 process tree, 100 MB and up, for a UI that is one icon and a
  menu.
- Electron: 100-200 MB, the wrong shape for a status indicator.
- Go with a systray library: cgo and a third build system on Windows.
- Reading pi's session JSONL files instead of an extension: no pi change, but it
  cannot distinguish waiting for the user from a dead tab, so the one state that
  matters is a guess.
