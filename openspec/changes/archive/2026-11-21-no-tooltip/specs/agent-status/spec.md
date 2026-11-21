# Spec Delta

## REMOVED Requirements

### Requirement: The tooltip counts sessions by state

**Reason**: the panel's summary line carries the same counts, on a surface the
user opened on purpose, and a tooltip that appears unbidden covers whatever is
behind the icon.

**Migration**: nothing reads the tooltip. The counts are in the panel header, and
the icon's colour still says the most urgent state at a glance.

## MODIFIED Requirements

### Requirement: An ended session stops being shown

The tray SHALL treat a session whose process is no longer running as gone: it
SHALL NOT show it, and it SHALL remove the state that session left behind.

#### Scenario: an ended session is not shown and its state is removed

- **WHEN** a session's process has ended and its state is still on disk
- **THEN** the session is absent from the indicator and the panel, and the
  leftover state is removed
