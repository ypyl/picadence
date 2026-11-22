# Spec Delta

## MODIFIED Requirements

### Requirement: A cadence belongs to one session

A cadence SHALL survive a tray restart, a reboot, and a session that is resumed
later, and SHALL wake only the session it was armed on. A cadence whose session is
not running SHALL wake nothing, unless the user kept that session, in which case
it SHALL start one tab for it as a click would and wake it there.

#### Scenario: a cadence follows its session across restarts and resumes

- **WHEN** a cadence was armed and its session is resumed after the tray, the
  machine, or both were restarted
- **THEN** the cadence is still armed, and it wakes that session at the next due
  time

#### Scenario: a cadence never wakes another session

- **WHEN** a session is armed and a different session is running in another tab
- **THEN** the other session is never woken by that cadence

## ADDED Requirements

### Requirement: A cadence opens the tab of a kept session

When a window falls due for a session that is not running and that the user kept,
the tray SHALL open exactly one terminal tab for that session and deliver the
wake-up in it, counting the windows that passed while it was closed. It SHALL open
nothing for a session the user did not keep.

#### Scenario: a due cadence opens a kept session's tab

- **WHEN** an armed session is not running, the user kept it, and its next ping
  arrives
- **THEN** one tab opens for that session, the wake-up arrives in it, and the
  message says how many windows passed while it was closed

#### Scenario: a cadence opens nothing the user did not keep

- **WHEN** an armed session is not running and the user did not keep it
- **THEN** no tab opens, and the window counts as missed

#### Scenario: a window opens one tab, not two

- **WHEN** the tray starts a kept session and the next tick arrives before that
  session's state is on disk
- **THEN** no second tab is started for it

### Requirement: A kept session that cannot be started is not attempted

When a due window finds a kept session that cannot be started, because its
conversation is gone, its folder is gone, Windows Terminal is not installed, or pi
is not installed, the tray SHALL start nothing, SHALL count the window as missed,
and SHALL schedule the next one normally.

#### Scenario: a missing conversation counts the window as missed

- **WHEN** a window falls due for a kept session whose conversation is gone
- **THEN** no tab opens, the window counts as missed, and the next window is a full
  cadence away
