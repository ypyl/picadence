# Spec Delta

## REMOVED Requirements

### Requirement: The wake-up comes from piclaw, not from the user

**Reason**: The product is called picadence. The requirement, and its scenario,
were named after a product that no longer exists.

**Migration**: The same behavior is added below under the new name, with the same
message, the same scenarios and the new product name in the words a person reads.
Nothing to do on the machine: no code reads the old text.

## ADDED Requirements

### Requirement: The wake-up comes from picadence, not from the user

The message that wakes a session SHALL name picadence, the time, and the armed
cadence, and SHALL ask the session to follow its own standing instruction. It
SHALL carry no instruction of its own. It SHALL appear as a message from
picadence and never as something the user typed.

#### Scenario: the wake-up names picadence and the cadence and gives no instruction

- **WHEN** a session is woken by its cadence
- **THEN** the message names picadence, the time and the cadence, asks for the
  session's standing instruction, and asks for nothing else
