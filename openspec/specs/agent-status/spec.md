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
SHALL NOT show it, and it SHALL remove the state that session left behind. A
session the user kept SHALL be the exception: the panel SHALL keep one row for it,
after the rows of the sessions that are running, saying it is not running, and the
indicator SHALL not count it.

#### Scenario: an ended session is not shown and its state is removed

- **WHEN** a session's process has ended and its state is still on disk
- **THEN** the session is absent from the indicator and the panel, and the
  leftover state is removed

#### Scenario: a kept session stays after it ends

- **WHEN** a session the user kept ends
- **THEN** the panel still shows one row for it, saying it is not running, and the
  indicator does not count it

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

### Requirement: The panel lists the live sessions

The panel SHALL list every live session, the most recently changed first, each
row naming the session's folder, its state in words, and how long ago it changed,
with a colour that repeats the state. It SHALL end with its controls: a way to
start picadence with Windows, and a way to quit. A left click on a session row
SHALL take no action, except on the row's pin control.

Each row SHALL carry one small control that keeps that session or stops keeping
it, and that control SHALL show which of the two it is by its shape, not only by
its colour.

#### Scenario: the panel lists each session with its folder, state, and age

- **WHEN** two live sessions changed at different times
- **THEN** the panel shows both, newest first, each with its folder, its state,
  its age, and the colour of that state, followed by the two controls

#### Scenario: a session row takes no action

- **WHEN** the panel is open and a session row is left-clicked anywhere but on its
  pin control
- **THEN** nothing happens to that session and the panel stays open

#### Scenario: the pin control keeps and unkeeps a session

- **WHEN** the pin control on a session's row is clicked twice
- **THEN** the session is kept and the control shows the kept shape, and then it
  is not kept again and the control returns to the other shape

### Requirement: A click opens the panel

A left click on the icon SHALL open the panel, and so SHALL a right click. A
second click on the icon, a click anywhere outside the panel, or the Escape key
SHALL close it.

#### Scenario: a click opens and closes the panel

- **WHEN** the icon is clicked
- **THEN** the panel opens, and the same click again closes it

#### Scenario: losing focus closes the panel

- **WHEN** the panel is open and another window takes focus
- **THEN** the panel closes

#### Scenario: Escape closes the panel

- **WHEN** the panel is open and the user presses Escape
- **THEN** the panel closes

### Requirement: The panel stays on screen

The panel SHALL open next to the icon, with a small gap, and SHALL always be
fully inside the work area of the monitor the icon sits on, moving to the other
side of the icon when there is no room.

#### Scenario: the panel stays inside the work area

- **WHEN** the icon sits in a corner and the panel would otherwise cross an edge
- **THEN** the panel moves so that it is fully inside the work area

### Requirement: The panel stays current while it is open

The panel SHALL show a change in any session's state within two seconds of that
change, without being reopened.

#### Scenario: the panel updates while it stays open

- **WHEN** the panel is open and a session changes state
- **THEN** the panel shows the new state within two seconds

### Requirement: The panel says when nothing is running

With no live session, the panel SHALL say so in words and SHALL still offer the
way to quit.

#### Scenario: an empty panel says nothing is running

- **WHEN** the panel is open and no session is live
- **THEN** it says that no sessions are running and still offers Quit

### Requirement: The panel is readable

The panel SHALL use a dark surface with light text, and every text colour SHALL
have a contrast ratio of at least 4.5 to 1 against the surface it is drawn on.
Every state SHALL be named in words as well as shown in colour.

#### Scenario: the text keeps its contrast

- **WHEN** any text colour in the panel is measured against its surface
- **THEN** the ratio is at least 4.5 to 1

#### Scenario: every state is named as well as coloured

- **WHEN** a session is in any state the panel can show
- **THEN** the row names that state in words, not only by its dot

### Requirement: Starting with Windows is a control, not a config file

The panel SHALL offer a control that turns starting with Windows on and off, and
SHALL show whether it is currently on. Turning it on SHALL make Windows start
picadence when the user signs in; turning it off SHALL stop that. The control
SHALL need no elevation and SHALL affect only the current user.

#### Scenario: the autostart control turns it on and off

- **WHEN** the user clicks the start-with-Windows control while it is off, and
  then clicks it again
- **THEN** it reads on and Windows is set to start picadence, and then it reads
  off and that setting is gone

### Requirement: The registration follows the app

When starting with Windows is on, the registration SHALL name the executable
that is running, and a later start from another path SHALL correct the
registration rather than leave the old path behind.

#### Scenario: the registered path follows the app

- **WHEN** the app is set to start with Windows and later runs from a different
  executable path
- **THEN** the registration names the new path, with one entry and no leftover

#### Scenario: a path with spaces still starts

- **WHEN** the executable path contains a space
- **THEN** the registration quotes it so Windows starts the same file

### Requirement: The tray starts only what the user asked for

The tray SHALL NOT send input to a running session, stop one, or modify a live
session's state. It SHALL start one program and no other: a terminal tab that
resumes a session the user kept, and only when the user asks for it. It SHALL open
no network connection.

#### Scenario: the tray links no network entry point

- **WHEN** the tray's interop surface is inspected
- **THEN** it names only user, shell, and kernel entry points, and no networking
  library at all

#### Scenario: an unarmed session is never woken

- **WHEN** a session has no cadence armed
- **THEN** the tray never starts a turn in it, however long it sits idle

#### Scenario: the tray starts nothing the user did not ask for

- **WHEN** the tray is running, sessions have ended, and no restart has been asked
  for
- **THEN** no process is started and the indicator follows the sessions that are
  live

### Requirement: The tray writes only beside its own state

The tray SHALL write only inside the state directory it owns: the record for a
cadence the user armed, the ping file for that cadence, the record for a session
the user kept, and the removal of state a dead process left behind. It SHALL NOT
write into pi's session storage.

#### Scenario: the tray writes nothing into pi's storage

- **WHEN** every path the tray can write is listed
- **THEN** each one is inside the state directory, and none is inside pi's
  sessions directory

### Requirement: A session can be kept after it ends

The user SHALL be able to keep a session from the panel and to stop keeping it.
A kept session SHALL stay in the panel across a restart of the tray and a reboot,
until the user stops keeping it, and keeping it SHALL change nothing about the
session itself.

#### Scenario: a kept session outlives the tray

- **WHEN** the user keeps a session and the tray is then restarted
- **THEN** that session is still in the panel, saying it is not running if its
  process has ended

#### Scenario: stopping the keeping removes the row

- **WHEN** the user stops keeping a session that is not running
- **THEN** its row is gone from the panel, and nothing else about that session
  changes

### Requirement: A kept session can be restarted

For a kept session that is not running, the panel SHALL offer a restart that opens
one new terminal tab in that session's folder and continues the same session, its
earlier conversation included. The restarted session SHALL be the same session, so
whatever the tray held for it before it ended continues to apply.

#### Scenario: restarting a kept session resumes that session

- **WHEN** the user restarts a kept session that is not running
- **THEN** one new terminal tab opens in that session's folder, the session
  continues with its earlier conversation, and the panel shows it as running

#### Scenario: a restarted session keeps what was armed for it

- **WHEN** a kept session with an armed cadence is restarted
- **THEN** that cadence applies to the restarted session without being armed again

### Requirement: A restart is offered only when it can work

The panel SHALL offer no restart for a session that is running, and SHALL name the
reason instead of offering one when a kept session's conversation is no longer on
disk, its folder no longer exists, or the terminal it needs is not installed.

#### Scenario: a running session offers no restart

- **WHEN** a kept session is running
- **THEN** its row offers no restart

#### Scenario: a missing conversation is named

- **WHEN** a kept session's conversation is no longer on disk
- **THEN** its row says that the conversation is gone and offers no restart

#### Scenario: a missing folder is named

- **WHEN** a kept session's folder no longer exists
- **THEN** its row says that the folder is gone and offers no restart

#### Scenario: a missing terminal is named

- **WHEN** the terminal a restart needs is not installed
- **THEN** the row says so and offers no restart
