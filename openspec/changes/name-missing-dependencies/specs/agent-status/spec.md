# Spec Delta

## MODIFIED Requirements

### Requirement: The panel says when nothing is running

With no live session, the panel SHALL say so in words and SHALL still offer the
way to quit. When pi is not installed, it SHALL say that instead of saying that
nothing is running, because there is a difference between a quiet machine and a
machine that cannot report.

#### Scenario: an empty panel says nothing is running

- **WHEN** the panel is open and no session is live
- **THEN** it says that no sessions are running and still offers Quit

## ADDED Requirements

### Requirement: The panel names a missing dependency

When a dependency is missing, the panel SHALL name it and say what it costs: pi
is what reports a session, and Windows Terminal is what can start one. The tray
SHALL keep running in both cases, and SHALL keep showing whatever it can.

#### Scenario: a missing pi is named instead of an empty list

- **WHEN** the panel is open, no session is live, and pi is not installed
- **THEN** the panel says that pi is not installed rather than that no sessions
  are running

#### Scenario: a missing terminal is named beside the sessions

- **WHEN** the panel is open, sessions are live, and Windows Terminal is not
  installed
- **THEN** the panel still lists those sessions and also says that Windows
  Terminal is not installed
