# Spec Delta

## MODIFIED Requirements

### Requirement: The panel lists the live sessions

The panel SHALL list every live session, the most recently changed first, each
row naming the session's folder, its state in words, and how long ago it changed,
with a colour that repeats the state. It SHALL end with its controls: a way to
start picadence with Windows, and a way to quit. A left click on a session row
SHALL take no action, except on the row's pin control.

Each row SHALL carry one small control that keeps that session or stops keeping
it, and that control SHALL show which of the two it is by its shape, not only by
its colour.

#### Scenario: the panel lists each session with its folder, state, and age

- **WHEN** two live sessions changed at different times
- **THEN** the panel shows both, newest first, each with its folder, its state,
  its age, and the colour of that state, followed by the two controls

#### Scenario: a session row takes no action

- **WHEN** the panel is open and a session row is left-clicked anywhere but on its
  pin control
- **THEN** nothing happens to that session and the panel stays open

#### Scenario: the pin control keeps and unkeeps a session

- **WHEN** the pin control on a session's row is clicked twice
- **THEN** the session is kept and the control shows the kept shape, and then it
  is not kept again and the control returns to the other shape
