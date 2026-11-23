# Spec Delta

## MODIFIED Requirements

### Requirement: The panel names a missing dependency

When a dependency is missing, the panel SHALL name it and say what it costs. Three
things have to be there for picadence to be whole: pi, which is what reports a
session; the pi half, which is the extension that does the reporting; and Windows
Terminal, which is what can start a session. The tray SHALL keep running in every
case, and SHALL keep showing whatever it can.

When nothing has reported yet, the panel SHALL say which of these is true: that the
pi half is not installed, naming the command that installs it, or that it is
installed and has not reported, saying that it reports when a session starts. It
SHALL say nothing about the pi half once a session has reported.

#### Scenario: a missing pi is named instead of an empty list

- **WHEN** the panel is open, no session is live, and pi is not installed
- **THEN** the panel says that pi is not installed rather than that no sessions
  are running

#### Scenario: a missing terminal is named beside the sessions

- **WHEN** the panel is open, sessions are live, and Windows Terminal is not
  installed
- **THEN** the panel still lists those sessions and also says that Windows
  Terminal is not installed

#### Scenario: a missing pi half is named with the command that installs it

- **WHEN** pi is installed, nothing has reported, and the pi settings list no
  picadence package
- **THEN** the panel says the pi half is not installed and names the command that
  installs it

#### Scenario: a declared pi half that has not reported says so

- **WHEN** pi is installed, the pi settings list picadence, and nothing has
  reported since the tray started
- **THEN** the panel says the pi half is installed and reports when a session
  starts, and does not ask for it to be installed again

#### Scenario: a reported session silences the pi half notice

- **WHEN** a session has reported to the tray
- **THEN** the panel says nothing about the pi half
