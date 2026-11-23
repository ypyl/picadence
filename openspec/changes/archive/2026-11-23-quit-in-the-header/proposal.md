# Quit moves to the header, and Autostart goes (L1)

## Why

The foot of the panel is a row of controls that exists because there was once a
checkbox in it. With Autostart gone, the row holds one X, which is the one control
that belongs in the corner of a panel rather than under a list.

Autostart itself is the odd one out: it writes to the registry, it is the only
thing picadence does that is about Windows rather than about sessions, and it
turns the product into a service the user configures instead of a thing they
start. Starting picadence when the user signs in is a shortcut, the shortcut
already exists in the Startup folder, and every download of the exe carries a
crafted `Startup` entry less code than this did.

## What Changes

- The X moves into the header, at its trailing edge. The summary makes room for
  it.
- Autostart is removed: the control, the registry writer, the startup correction,
  and the two requirements that described them. Nothing else about starting at
  login changes; Windows still has a Startup folder.
- The footer row disappears, so the panel is one row shorter.

## Capabilities

### Modified Capabilities

- `agent-status`: the panel's controls, and the deletion of the autostart
  capability.

## Impact

- `Autostart.cs` and its tests go. `PanelModel` loses the checkbox and the footer,
  `Panel` loses a hover target, `Program` loses the startup correction.
- On this machine no `Run` value exists, so there is nothing to clean up. A machine
  that had one keeps starting picadence at sign-in until the user removes it from
  Windows, which this change says plainly in the README.

## Routing

0. **Does observable behavior change?** Yes: a capability disappears and a control
   moves to another corner.
1. **Can it be undone easily?** Yes, one commit, and the registry entry was the
   only external effect and it is not touched.
2. **Can a machine prove it correct?** Yes: the header rectangle and the absence of
   an autostart action are layout values with tests.
3. **Does anything outside the change depend on it?** Two requirements of the
   durable spec do, and they are removed here. A `Run` value on somebody's machine
   is affected only in that picadence stops maintaining it.

Level: **L1**.

## Approval

- Gate: L1 delta spec
- Approved by: Yauhen Pyl
- Approved at: 2026-11-23
- Artifact: openspec/changes/quit-in-the-header/specs/agent-status/spec.md sha256:b9548e37f3a28d27255fa491a8d59b6fc6e7f6272eb6800654469ae519873d9b
- Artifact: openspec/changes/quit-in-the-header/proposal.md sha256:ae3ed11dbe2fe5cb5d8f451e282157dff6064362dae3e7bbe05d601342fdd7e8
- Words: "(none)"
