# Spec Delta

## RENAMED Requirements

- FROM: `### Requirement: The tray only observes`
- TO: `### Requirement: The tray only pokes the session the user armed`

## MODIFIED Requirements

### Requirement: The tray only pokes the session the user armed

The tray SHALL NOT send input to a pi session, start or stop one, modify a live
session's state, or touch pi's own session storage. Apart from waking a session
the user armed with a cadence, its only write SHALL be removing the state a dead
process left behind, and it SHALL open no network connection.

#### Scenario: the tray links no network entry point

- **WHEN** the tray's interop surface is inspected
- **THEN** it names only user, shell, and kernel entry points, and no networking
  library at all

#### Scenario: an unarmed session is never woken

- **WHEN** a session has no cadence armed
- **THEN** the tray never starts a turn in it, however long it sits idle

#### Scenario: the tray never starts a session

- **WHEN** the tray is running and no session is live
- **THEN** no pi process is started and the indicator stays quiet

### Requirement: The panel lists the live sessions

The panel SHALL list every live session, the most recently changed first, each
row naming the session's folder, its state in words, and how long ago it
changed, with a colour that repeats the state. It SHALL end with its controls: a
way to start piclaw with Windows, and a way to quit. A left click on a session
row SHALL take no action.

#### Scenario: the panel lists each session with its folder, state, and age

- **WHEN** two live sessions changed at different times
- **THEN** the panel shows both, newest first, each with its folder, its state,
  its age, and the colour of that state, followed by the two controls

#### Scenario: a session row takes no action

- **WHEN** the panel is open and a session row is left-clicked
- **THEN** nothing happens to that session and the panel stays open
