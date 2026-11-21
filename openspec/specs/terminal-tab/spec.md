# terminal-tab Specification

## Purpose
Puts each session's state on the terminal tab that session runs in, so a tab says
whether it needs you without a second application and without anyone matching a
session to a tab.

## Requirements

### Requirement: A session shows its state where its tab is

While a pi session runs in a terminal tab, that tab SHALL show the session's state
as a mark before the session's own label: working, waiting for the user, failed,
done, and nothing at all when the session is idle.

#### Scenario: the tab carries the state as a mark

- **WHEN** a session in a tab is working, then waiting for the user, then failed,
  then done
- **THEN** its tab shows a distinct mark for each of those four states, in that
  order, and the mark for waiting and the mark for failed are different

#### Scenario: an idle session shows no mark

- **WHEN** a session starts, or the user cancels a run
- **THEN** its tab shows the session's label with no state mark before it

### Requirement: The tab still says which session it is

The mark SHALL be added to the session's own label, not replace it: the tab SHALL
carry the session's name when it has one and its folder otherwise, so a tab is
never identified by a mark alone.

#### Scenario: the tab keeps the session's label

- **WHEN** a named session and an unnamed session each change state
- **THEN** the named one's tab carries its name after the mark, and the unnamed
  one's tab carries its folder
