# Changelog

One entry per user-facing change, newest first, under `## Unreleased`. The rules
are in `AGENTS.md` under **Versions and changelog**.

## Unreleased

- The panel grows to hold the cadence menu, so every cadence and "off" can be
  reached with one session open, and it shrinks again when the menu closes.

- A kept session that is not running shows its cadence and when it will next open
  its tab, instead of saying only that it is not running.

- A session with its own name shows it on its row, under the folder, before its
  state. A session named after its folder is not made to say it twice.


## picadence-v0.3.0

- A session that needs you, finishes, or fails asks Windows Terminal for a
  notification, and clicking it brings that terminal forward. Terminal 1.26 shows
  it once the profile allows it (`compatibility.allowOSC777`); older versions
  ignore the request. `PICADENCE_NOTIFY=0` turns it off.
- The panel says when something it needs is missing: pi itself, the pi extension
  that does the reporting, or Windows Terminal. For the extension it names the
  command that installs it, and it stays quiet once a session has reported.
- The pi half installs from this repository as a pi package:
  `pi install git:github.com/ypyl/picadence`.
- The X that stops picadence moved into the panel's header, and the row of controls
  under the list is gone, so the panel is a row shorter. Autostart is removed;
  Windows already has a Startup folder (`shell:startup`), and a shortcut in it does
  the same thing without a registry value to keep in step.
- The keep control is a star, outline or solid, and the age column is as wide as the
  ages it holds rather than a fixed width with a hole in it.
- A session whose process ended stops being shown even when Windows has given its
  process id to another program. The state file now records when the writing process
  started, and the tray compares that with the kernel's answer.
- winget carries the tray as `Ypyl.Picadence`.

## picadence-v0.2.0

- Each pi tab now says what its session is doing, in the tab itself: a mark and
  the session's name, so a tab that needs you looks different from one that is
  working, even with picadence not running. In Windows Terminal the same state
  shows as a ring on the tab and a progress bar on the taskbar;
  `PICADENCE_TAB_PROGRESS=0` turns the ring off and `=1` forces it on elsewhere.
- A session can be kept with the pin on its row. It stays in the panel after its
  tab closes, saying it is not running, and a play control beside the pin opens a
  new tab that continues the conversation where it stopped.
- A cadence can reach a session you kept and closed. At its due time the tray
  opens that session's tab and the wake-up arrives in it, saying how many windows
  passed while it was closed. A session you did not keep is still never woken.
- The panel shows five sessions at a time and scrolls to the rest, with a line
  saying where in the list you are.
- The panel names a missing dependency. Without pi it says so instead of claiming
  the machine is quiet, and without Windows Terminal it still lists your sessions
  and tells you they cannot be started.
- The foot of the panel is one row: a checkbox that starts picadence with Windows,
  and an X that stops it.
- The repository is MIT licensed.

## picadence-v0.1.0

- The tool is called picadence. The name it carried, piclaw, is another pi
  project by another author, and "claw" has meant nothing for a while. Nothing
  changes except what you read and where the exe lives; a schedule you armed
  keeps its cadence, and the old `%LOCALAPPDATA%\piclaw` folder is left alone.
- A session can be armed with a cadence. Right-click a row and pick every 15
  minutes, every hour, every week, every month, every year, or off. When the time
  comes, the session wakes itself, follows whatever standing instruction it was
  given, and picadence tells you on the row when the next one is due.
- A cadence belongs to a session and survives a restart of picadence, a reboot, and
  a session you resume, so a periodic job keeps running across your day without
  you remembering to type anything.
- The two controls at the foot of the panel now have real padding and a taller
  hit area, so a hovered one reads as a button instead of a highlighted row.
- `DESIGN.md` records the visual language: tokens, spacing, type, states, and the
  contrast floor.
- Hovering the tray icon no longer shows a tooltip. The session counts are on the
  panel's summary line, where you open them on purpose.
- picadence can start with Windows. The panel has a Start with Windows control that
  turns it on and off, with no administrator and no settings file, and the entry
  follows the exe if you move or rebuild it.
- Quit now looks like a button: hovering it paints a highlight and turns the
  pointer into a hand.
- The age in a session row no longer touches the edge of its box, so it stops
  looking clipped.
- The panel no longer shows the busy pointer while you hover it. It was the
  Windows "app starting" arrow, because the panel's window never named a cursor.
- The status moved from a plain Windows menu into a panel: click the icon and a
  dark flyout opens with a summary line, one row per session with its colour,
  folder, state and how long ago it changed, and Quit. It follows the icon,
  stays on screen at any edge, and updates while it is open.
- The tray now shows what your pi sessions are doing: red for a failure, amber
  for a session waiting on you, green for work in progress, grey when nothing
  needs you. The tooltip counts sessions by state, and the menu lists each one
  with its folder, its state, and how long ago it changed.
- picadence starts: the project, its stack, and its tests.
