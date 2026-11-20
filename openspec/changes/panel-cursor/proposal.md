# The panel shows a normal pointer (L0)

## Why

Hovering the panel shows the Windows "app starting" pointer, the arrow with the
spinning circle. The panel's window class names no cursor, and the process never
sets one, so the pointer stays whatever Windows had last. A status flyout that
looks busy makes the app look stuck.

## What Changes

- The panel's window class carries the standard arrow cursor.
- The class definition moves into a function so the cursor it names can be
  checked by a test instead of by hovering.

No behavior named in a spec moves. The requirement this restores is **The panel
is readable**: a panel whose pointer says "wait" is not reading as a plain
flyout, and nothing about what it shows changes.

## Capabilities

- **New**: none. `skip_specs` is set.
- **Modified**: none.

## Impact

- `src/Trayclaw/Panel.cs`: the class is built by `CreateWindowClass`, which sets
  the cursor.
- `src/Trayclaw/Native.cs`: `LoadCursorW`, `IDC_ARROW`, and the cursor field.
- One test asserting the class names the arrow cursor.
- No document changes beyond the changelog; the panel still looks and behaves as
  the archived `tray-panel` change left it.

## Routing

0. **Does observable behavior change?** No requirement moves. The pointer goes
   from wrong to right, which is the defect and not a new capability.
1. **Can it be undone easily?** Yes, one commit, nothing stored.
2. **Can a machine prove it correct?** Yes: the class is data, and the test reads
   the cursor it names. Before this change there was nothing to read.
3. **Does anything outside the change depend on it?** No.

Level L0: the four answers are benign, so no gate and no delta spec.
