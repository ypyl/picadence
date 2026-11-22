# Spec Delta

## REMOVED Requirements

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

## MODIFIED Requirements

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

## ADDED Requirements

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
