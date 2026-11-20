# Spec Delta

## REMOVED Requirements

### Requirement: The menu lists the live sessions

**Reason**: the Windows menu is replaced by the status panel, which shows the same
sessions with a colour, a summary, and an empty state.

**Migration**: nothing reads the menu. The same sessions, in the same order, now
appear in the panel, which a left click opens.

## ADDED Requirements

### Requirement: The panel lists the live sessions

The panel SHALL list every live session, the most recently changed first, each
row naming the session's folder, its state in words, and how long ago it
changed, with a colour that repeats the state. It SHALL end with the way to quit,
and a session row SHALL take no action.

#### Scenario: the panel lists each session with its folder, state, and age

- **WHEN** two live sessions changed at different times
- **THEN** the panel shows both, newest first, each with its folder, its state,
  its age, and the colour of that state, followed by Quit

#### Scenario: a session row takes no action

- **WHEN** the panel is open and a session row is clicked
- **THEN** nothing happens to that session and the panel stays open

### Requirement: A click opens the panel

A left click on the icon SHALL open the panel, and so SHALL a right click. A
second click on the icon, a click anywhere outside the panel, or the Escape key
SHALL close it.

#### Scenario: a click opens and closes the panel

- **WHEN** the icon is clicked
- **THEN** the panel opens, and the same click again closes it

#### Scenario: losing focus closes the panel

- **WHEN** the panel is open and another window takes focus
- **THEN** the panel closes

#### Scenario: Escape closes the panel

- **WHEN** the panel is open and the user presses Escape
- **THEN** the panel closes

### Requirement: The panel stays on screen

The panel SHALL open next to the icon, with a small gap, and SHALL always be
fully inside the work area of the monitor the icon sits on, moving to the other
side of the icon when there is no room.

#### Scenario: the panel stays inside the work area

- **WHEN** the icon sits in a corner and the panel would otherwise cross an edge
- **THEN** the panel moves so that it is fully inside the work area

### Requirement: The panel stays current while it is open

The panel SHALL show a change in any session's state within two seconds of that
change, without being reopened.

#### Scenario: the panel updates while it stays open

- **WHEN** the panel is open and a session changes state
- **THEN** the panel shows the new state within two seconds

### Requirement: The panel says when nothing is running

With no live session, the panel SHALL say so in words and SHALL still offer the
way to quit.

#### Scenario: an empty panel says nothing is running

- **WHEN** the panel is open and no session is live
- **THEN** it says that no sessions are running and still offers Quit

### Requirement: The panel is readable

The panel SHALL use a dark surface with light text, and every text colour SHALL
have a contrast ratio of at least 4.5 to 1 against the surface it is drawn on.
Every state SHALL be named in words as well as shown in colour.

#### Scenario: the text keeps its contrast

- **WHEN** any text colour in the panel is measured against its surface
- **THEN** the ratio is at least 4.5 to 1

#### Scenario: every state is named as well as coloured

- **WHEN** a session is in any state the panel can show
- **THEN** the row names that state in words, not only by its dot
