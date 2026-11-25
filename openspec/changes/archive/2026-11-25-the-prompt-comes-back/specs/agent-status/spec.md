# Spec Delta

## ADDED Requirements

### Requirement: Starting picadence gives the terminal back

When picadence is started from a command shell, the shell SHALL be free to take
commands as soon as the tray is running, and the tray SHALL keep running when that
shell, or the window it was in, is closed. When picadence is started any other way,
the process that was started SHALL be the tray, with no second copy.

#### Scenario: a start from a command shell leaves the shell free

- **WHEN** picadence is started from a command shell
- **THEN** the shell takes commands again while the tray keeps running

#### Scenario: a start from a shortcut runs the tray in place

- **WHEN** picadence is started from a shortcut, a login entry, or a build rather
  than from a shell
- **THEN** that process is the tray, and no second copy of it is started

### Requirement: One tray runs at a time

A second start SHALL NOT add a second indicator. It SHALL say that picadence is
already running, where it was started, and exit without starting a tray.

#### Scenario: a second start adds no second icon

- **WHEN** picadence is started while a tray is already running
- **THEN** the second start says picadence is already running and exits, and the
  indicator the tray shows is unchanged
