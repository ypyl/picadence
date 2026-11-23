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

### Requirement: Windows Terminal shows the state on the tab as well

In Windows Terminal, a session SHALL also show its state as progress on its own
tab, and SHALL clear that progress when the session is idle or done. In any other
terminal, the session SHALL write no progress at all.

#### Scenario: a session in Windows Terminal carries its state there

- **WHEN** a session in Windows Terminal is working, then waiting for the user,
  then failed, then done
- **THEN** its tab shows progress for each of those states, and none once the run
  is done or the session is idle

#### Scenario: another terminal is written nothing

- **WHEN** a session runs in a terminal that is not Windows Terminal
- **THEN** no progress is written to it, and the tab's mark and label are the
  only state it shows

### Requirement: A person can turn that off

The user SHALL be able to keep the progress signal off in Windows Terminal, and
to force it on in a terminal the product does not recognise, without changing the
extension's code.

#### Scenario: the switch overrides what the product detected

- **WHEN** the switch is set to off in Windows Terminal, and to on in another
  terminal
- **THEN** Windows Terminal shows no progress, and the other terminal is sent the
  progress it was forced to take

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
