# Spec Delta

## MODIFIED Requirements

### Requirement: The panel lists the live sessions

The panel SHALL list every live session, the most recently changed first, each
row naming the session's folder, its state in words, and how long ago it
changed, with a colour that repeats the state. It SHALL end with its controls: a
way to start piclaw with Windows, and a way to quit. A session row SHALL take no
action.

#### Scenario: the panel lists each session with its folder, state, and age

- **WHEN** two live sessions changed at different times
- **THEN** the panel shows both, newest first, each with its folder, its state,
  its age, and the colour of that state, followed by the two controls

#### Scenario: a session row takes no action

- **WHEN** the panel is open and a session row is clicked
- **THEN** nothing happens to that session and the panel stays open

## ADDED Requirements

### Requirement: Starting with Windows is a control, not a config file

The panel SHALL offer a control that turns starting with Windows on and off, and
SHALL show whether it is currently on. Turning it on SHALL make Windows start
piclaw when the user signs in; turning it off SHALL stop that. The control SHALL
need no elevation and SHALL affect only the current user.

#### Scenario: the autostart control turns it on and off

- **WHEN** the user clicks the start-with-Windows control while it is off, and
  then clicks it again
- **THEN** it reads on and Windows is set to start piclaw, and then it reads off
  and that setting is gone

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
