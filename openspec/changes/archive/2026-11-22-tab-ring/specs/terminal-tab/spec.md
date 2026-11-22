# Spec Delta

## ADDED Requirements

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
