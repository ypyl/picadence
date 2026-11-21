# scheduled-ping Specification

## Purpose
Lets a person arm a pi session with a cadence, so the session wakes itself on a
schedule and does whatever standing instruction it was given, without anyone
typing.

## Requirements

### Requirement: A session can be armed with a cadence

The panel SHALL offer each session a cadence of every 15 minutes, every hour,
every week, every month, or every year, and a way to turn it off. The row SHALL
show the armed cadence and how long until the next ping.

#### Scenario: arming a row sets the cadence and shows what comes next

- **WHEN** the user arms a session row with every hour
- **THEN** that row shows every hour and the time until the next ping

#### Scenario: turning a cadence off removes it

- **WHEN** the user turns off an armed session row
- **THEN** the row shows no cadence and that session is never woken again

### Requirement: A due cadence wakes an idle session once

When an armed session is idle and its next ping has arrived, the session SHALL
start exactly one turn.

#### Scenario: a due cadence wakes an idle session

- **WHEN** an armed session is idle and its next ping arrives
- **THEN** the session starts one turn, and its state becomes working

### Requirement: A busy session is skipped and the window counts as missed

A session that is working or waiting for the user SHALL NOT be woken. That
window SHALL count as missed.

#### Scenario: a busy session is skipped and counted as missed

- **WHEN** a ping falls due while the session is working or waiting for the user
- **THEN** no message arrives, the session stays as it was, and the missed count
  goes up by one

### Requirement: A cadence belongs to one session

A cadence SHALL survive a tray restart, a reboot, and a session that is resumed
later, and SHALL wake only the session it was armed on. A cadence whose session
is not running SHALL wake nothing.

#### Scenario: a cadence follows its session across restarts and resumes

- **WHEN** a cadence was armed and its session is resumed after the tray, the
  machine, or both were restarted
- **THEN** the cadence is still armed, and it wakes that session at the next due
  time

#### Scenario: a cadence never wakes another session

- **WHEN** a session is armed and a different session is running in another tab
- **THEN** the other session is never woken by that cadence

### Requirement: A gap produces one catch-up, not a burst

When one or more windows fall due while the armed session is absent or busy, the
tray SHALL wake it at most once when it next becomes available, and that message
SHALL say how many windows were missed.

#### Scenario: a gap produces one wake-up and says how many windows were missed

- **WHEN** an hourly session is absent for a day and then returns
- **THEN** it is woken once, and the message says how many hourly windows were
  missed

### Requirement: A cadence is an interval, not a time of day

Each cadence SHALL be measured from the previous fire. A monthly or yearly
cadence SHALL keep its calendar day, so a day that does not exist in the next
month moves to that month's last day.

#### Scenario: a monthly cadence keeps its calendar day

- **WHEN** a cadence armed on 31 January fires, and the next month is February
- **THEN** the next ping is due on the last day of February, not in March

### Requirement: A cadence is kept until it is turned off

The tray SHALL NOT discard a cadence on its own, however long its session has
been absent, and SHALL remove it only when the user turns it off.

#### Scenario: an unused cadence is still remembered a year later

- **WHEN** a session armed with every week has not run for a year
- **THEN** the cadence is still armed, and it wakes that session once when it
  returns

### Requirement: The wake-up comes from picadence, not from the user

The message that wakes a session SHALL name picadence, the time, and the armed
cadence, and SHALL ask the session to follow its own standing instruction. It
SHALL carry no instruction of its own. It SHALL appear as a message from
picadence and never as something the user typed.

#### Scenario: the wake-up names picadence and the cadence and gives no instruction

- **WHEN** a session is woken by its cadence
- **THEN** the message names picadence, the time and the cadence, asks for the
  session's standing instruction, and asks for nothing else
