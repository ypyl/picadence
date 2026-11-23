# Spec Delta

## ADDED Requirements

### Requirement: The tab asks the terminal to notify you

When a session stops to wait for the user, finishes a run, or fails, its tab SHALL
emit the terminal's own desktop-notification sequence, naming picadence and that
session in words. A terminal that does not understand the sequence SHALL be
unaffected, and the user SHALL be able to turn this off.

#### Scenario: a session that needs you asks the terminal to notify

- **WHEN** a run ends with the session waiting for the user, finished, or failed
- **THEN** its tab emits one notification sequence naming picadence and that
  session

#### Scenario: a quiet session asks for nothing

- **WHEN** a session starts, works, or is cancelled back to idle
- **THEN** its tab emits no notification sequence

#### Scenario: the switch turns it off

- **WHEN** the switch is off and a session finishes a run
- **THEN** its tab emits no notification sequence

#### Scenario: the words cannot break the sequence

- **WHEN** a session's folder name contains the sequence's own separators
- **THEN** the notification carries the name with those characters removed, and the
  sequence still has one title and one body
