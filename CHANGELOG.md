# Changelog

One entry per user-facing change, newest first, under `## Unreleased`. The rules
are in `AGENTS.md` under **Versions and changelog**.

## Unreleased

- The status moved from a plain Windows menu into a panel: click the icon and a
  dark flyout opens with a summary line, one row per session with its colour,
  folder, state and how long ago it changed, and Quit. It follows the icon,
  stays on screen at any edge, and updates while it is open.
- The tray now shows what your pi sessions are doing: red for a failure, amber
  for a session waiting on you, green for work in progress, grey when nothing
  needs you. The tooltip counts sessions by state, and the menu lists each one
  with its folder, its state, and how long ago it changed.
- trayclaw starts: the project, its stack, and its tests.
