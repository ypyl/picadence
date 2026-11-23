# Spec Delta

## REMOVED Requirements

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

## MODIFIED Requirements

### Requirement: The panel lists the live sessions

The panel SHALL list every live session, the most recently changed first, each
row naming the session's folder, its state in words, and how long ago it changed,
with a colour that repeats the state. Its header SHALL carry one control, the one
that stops picadence. A left click on a session row SHALL take no action, except
on the row's own controls.

Each row SHALL carry one small control that keeps that session or stops keeping
it, and that control SHALL show which of the two it is by its shape, not only by
its colour. A row for a session that is not running and can be restarted SHALL
carry a second small control that restarts it, and that control SHALL be absent
when the restart would not work.

#### Scenario: the panel lists each session with its folder, state, and age

- **WHEN** two live sessions changed at different times
- **THEN** the panel shows both, newest first, each with its folder, its state,
  its age, and the colour of that state, and the header carries the control that
  stops picadence

#### Scenario: a session row takes no action

- **WHEN** the panel is open and a session row is left-clicked anywhere but on one
  of its controls
- **THEN** nothing happens to that session and the panel stays open

#### Scenario: the pin control keeps and unkeeps a session

- **WHEN** the pin control on a session's row is clicked twice
- **THEN** the session is kept and the control shows the kept shape, and then it
  is not kept again and the control returns to the other shape

#### Scenario: the start control restarts a kept session

- **WHEN** the panel shows a kept session that is not running and can be restarted
- **THEN** its row carries a start control, and a click on it is the row's only
  way to start that session

#### Scenario: a kept row that cannot restart has no start control

- **WHEN** the panel shows a kept session whose conversation is gone
- **THEN** its row carries no start control, and the row says what is missing
