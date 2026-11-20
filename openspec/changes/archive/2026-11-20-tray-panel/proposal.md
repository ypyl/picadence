# A styled panel replaces the plain menu (L1)

## Why

The tray answers "is anything waiting for me" with a coluored dot, and the rest
with a Windows menu: grey system rows, system font, and a state word buried in a
line of text. A menu cannot show a status line, cannot show a colour per session,
and looks like Windows 2015. The user asked for the details on a left click, in a
dark panel that reads well.

## What Changes

- A left click opens a panel above the icon instead of a menu. A right click opens
  the same panel.
- The panel has a header with the session count, one row per session with a
  colour dot, the folder, the state and detail, and the age, an empty state, and
  a footer with Quit.
- The panel stays on screen when the icon sits near an edge, closes when it loses
  focus or on Escape, and refreshes while it is open.
- The plain Windows menu and its stock look are gone. **BREAKING** for anyone who
  relied on the menu: it is the same information in a different surface.
- Contrast is a test, not a taste: every text colour clears 4.5:1 against the
  surface it sits on.

## Capabilities

- **New**: none. This is the same capability, told better.
- **Modified**: `agent-status`. The requirement about the menu becomes the panel,
  and the panel's opening, closing, placement, refresh, empty state, and contrast
  are added.

## Impact

- `src/Trayclaw`: new `PanelModel`, `PanelPlacement`, `PanelTheme`, `PanelScale`,
  and a panel window in `Panel.cs`; `Program.cs` routes both clicks to it and
  drops the menu code; `Native.cs` gains the GDI and window calls the panel needs.
- `tests/Trayclaw.Tests`: one test per scenario, all on pure functions except the
  runtime smoke.
- `ARCHITECTURE.md`, `README.md`, `CHANGELOG.md`, `CONVENTIONS.md`, and
  `openspec/specs/agent-status/spec.md` after archive.
- Not changed: the ADR. This stays inside "Win32 directly, no GUI toolkit"; a
  toolkit would be the only thing that changed that decision.
- The contract another change could break: none. The panel reads what the tray
  already reads.

## Routing

0. **Does observable behavior change?** Yes: a click opens a different surface,
   and the panel has states the menu did not have.
1. **Can it be undone easily?** Yes. One commit and no stored data.
2. **Can a machine prove it correct?** Partly, and that half is written down: the
   row model, the placement when an edge is close, the hit test, the refresh, and
   the contrast ratios are pure functions with tests. Whether the panel looks good
   is yours to judge, and that is why the design decisions are on the page.
3. **Does anything outside the change depend on it?** No.

Level L1: behavior moves, nothing outside depends on it, and the part a machine
cannot judge is only the look.

## Design decisions on the page

Taken from Microsoft's guidance for spacing and layering, and from what Windows 11
itself does with flyouts:

- **One surface, dark, layered.** The panel is `#202020` with rows on `#2B2B2B` and
  a 1px `#3A3A3A` outline, because in dark mode a brighter surface reads as closer
  to the user. Text is `#FFFFFF`, secondary text `#C5C5C5`.
- **Spacing on the 4px grid:** 12px padding, 8px between rows, 16px between the
  header and the first row. Windows growth guidance uses 8 and 12 between content
  areas, and 16 at the edge.
- **Type:** Segoe UI Variable Text at 14px for a folder, 12px for state and age,
  12px semibold for the header. Body and caption, nothing invented.
- **State is a word and a colour**, never colour alone: `waiting`, `working`,
  `failed`, `done`, `quiet`, each with a 8px dot.
- **Accent used sparingly**: only the dot and the row that is waiting get colour;
  everything else is neutral, which is what the colour guidance asks for.
- **Anchored, not centred**: 8px above the icon, right-aligned to it, flipped
  below when there is no room, and clamped into the work area of the monitor the
  icon is on.
- **Light dismiss**: losing focus closes it, Escape closes it, and it never steals
  a click from the window underneath.
- **Readable at a glance, not a dashboard**: no charts, no history, no badges.

## Out of scope

- Toasts, autostart, a settings window, theming, a light-mode variant, acrylic or
  mica, animation, and clicking a row to focus its terminal tab.
- Any window that stays open. The panel is a flyout, not a main window.

## Approval

- Gate: L1 delta spec
- Approved by: Yauhen Pyl
- Approved at: 2026-11-20
- Artifact: openspec/changes/tray-panel/specs/agent-status/spec.md sha256:9e10516b08c0c390e72e658d609886cc5ba20ff9a63d19cdd3d664f7a8dd94d9
- Artifact: openspec/changes/tray-panel/proposal.md sha256:68af09640ee2f0f193bb493fc16effca941c0a64b23b3a9c7ee6988334eb2947
- Words: "(none)"
