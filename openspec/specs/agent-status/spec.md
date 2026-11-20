# agent-status Specification

## Purpose
Tell a person who runs several pi sessions, each in its own terminal tab, which
one is working, which one is waiting for them, and which one failed, without
leaving the current window.

## Requirements

### Requirement: One indicator reflects the most urgent state

The tray SHALL show exactly one indicator for all live sessions, and its
appearance SHALL be decided by the most urgent state among them, in this order:
a failed run, then a session waiting for the user, then a session working, then
quiet. When no session is live, the indicator SHALL be quiet.

#### Scenario: the most urgent state wins

- **WHEN** one live session is working, one is waiting for the user, and one
  failed
- **THEN** the indicator shows the failed state

#### Scenario: no live session leaves the indicator quiet

- **WHEN** no session is live
- **THEN** the indicator is quiet

### Requirement: An ended session stops being shown

The tray SHALL treat a session whose process is no longer running as gone: it
SHALL NOT show it, and it SHALL remove the state that session left behind.

#### Scenario: an ended session is not shown and its state is removed

- **WHEN** a session's process has ended and its state is still on disk
- **THEN** the session is absent from the indicator, the tooltip, and the menu,
  and the leftover state is removed

### Requirement: The tooltip counts sessions by state

The tooltip SHALL name the number of live sessions in each state that has at
least one, in the order failed, waiting, working, done, quiet, and SHALL say so
plainly when no session is live.

#### Scenario: the tooltip counts sessions by state

- **WHEN** two live sessions are working and one is waiting for the user
- **THEN** the tooltip names two working and one waiting

### Requirement: The menu lists the live sessions

The menu SHALL list every live session, the most recently changed first, each
row naming the session's folder, its state in words, and how long ago it
changed. It SHALL end with the way to quit, and it SHALL offer no other action.

#### Scenario: the menu lists each session with its folder, state, and age

- **WHEN** two live sessions changed at different times
- **THEN** the menu shows both, newest first, each with its folder, its state,
  and its age, followed by Quit

#### Scenario: the menu offers no action on a session

- **WHEN** the menu is opened
- **THEN** every session row is a label that cannot be chosen, and Quit is the
  only choice

### Requirement: A state change appears within two seconds

The tray SHALL reflect a change in any session's state within two seconds,
without the user doing anything.

#### Scenario: a state change is shown within two seconds

- **WHEN** a session reports a new state
- **THEN** the tray shows it within two seconds

### Requirement: Each pi lifecycle step maps to exactly one state

The pi side SHALL report `idle` when a session starts and when the user cancels a
run, `working` while a run is in progress, `blocked` while pi waits on a
question for the user, `done` when a run completes, and `error` when a run ends
in failure.

#### Scenario: each pi lifecycle step maps to one state

- **WHEN** a session starts, runs, asks a question, finishes, and fails in turn
- **THEN** each step reports exactly the state named above

### Requirement: A clean exit leaves nothing behind

When pi exits normally, the state it reported SHALL be removed, so nothing stale
reaches the next run.

#### Scenario: a clean pi exit leaves nothing behind

- **WHEN** pi exits normally
- **THEN** no state from that session remains

### Requirement: The tray only observes

The tray SHALL NOT send input to a pi session, start or stop one, or modify a
live session's state. Its only write SHALL be removing state whose process has
ended, and it SHALL open no network connection.

#### Scenario: the tray links no network entry point

- **WHEN** the tray's interop surface is inspected
- **THEN** it names only user, shell, and kernel entry points, and no networking
  library at all
