# Ask the terminal to notify you (L1)

## Why

Microsoft will not add a way for anything outside a tab to focus that tab: #19783
is closed as not planned, and the pull request that implemented `wt focus-tab
--session` was rejected because focus-stealing sets a precedent no platform wants.
What they added instead is **OSC 777**, a notification an application inside a tab
emits, which the terminal shows and dispatches on click: "when this notification is
clicked, it summons the terminal window that sent it".

That is the same goal picadence exists for, reached with the terminal's consent
rather than in spite of it. The extension already sits inside the tab and already
knows when a session needs you.

## What Changes

- On a session that is waiting for the user, finished a run, or failed, the tab
  emits one OSC 777 sequence naming picadence and the folder.
- `PICADENCE_NOTIFY=0` turns it off, in the same spirit as the progress switch.
- Nothing is brought to the front by picadence: a terminal that does not know the
  sequence ignores it, and one that does shows a notification the user chooses to
  click.

On this machine it changes nothing visible yet: Windows Terminal 1.25 ignores the
sequence. 1.26 ships the support behind `compatibility.allowOSC777`, off by
default.

## Capabilities

### Modified Capabilities

- `terminal-tab`: the tab may ask the terminal for attention.

## Impact

- `extensions/picadence.js`, its tests, the README, `CHANGELOG.md`, and one line in
  `REPO-FACTS.md` recording this machine's Terminal version and the opt-in.

## Routing

0. **Does observable behavior change?** Yes: a terminal that supports it shows a
   notification where before it showed nothing.
1. **Can it be undone easily?** Yes: one commit, and a switch at runtime.
2. **Can a machine prove it correct?** Yes: the sequence is bytes, and the four
   states are four assertions.
3. **Does anything outside the change depend on it?** The sequence is a published
   protocol, which is a reason to match it exactly and no reason to change anything
   else.

Level: **L1**.

## Approval

- Gate: L1 delta spec
- Approved by: Yauhen Pyl
- Approved at: 2026-11-23
- Artifact: openspec/changes/notify-a-session/specs/terminal-tab/spec.md sha256:d98f6e6eaefab61a38f353c7861896436cd4f249109217e6a6119fb3d1245540
- Artifact: openspec/changes/notify-a-session/proposal.md sha256:638694607939f57745c6fa3f295a8bfd23d5bde556f17f40c8a0965e80d6d1a4
- Words: "(none)"
