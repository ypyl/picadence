# Design

A rename is mechanical, and the only decisions worth writing down are the three
places where a name is also a contract.

## The state directory

`Paths.Root` and the extension's `stateDirectory()` both become
`%LOCALAPPDATA%\picadence`. The old directory is left on disk, not deleted and
not read: it holds a handful of files that a running session rewrites on its next
transition, and the tray would delete the stale ones anyway. Nothing is armed in
it today, so nothing is lost. Deleting a directory inside another product's
profile is the kind of act that needs its own decision; a rename does not.

A session that is running during the upgrade writes its state to the old
directory and is invisible until pi restarts. The README already says a session
that was running when the extension was installed reports nothing until pi
restarts, so this is the same rule, not a new one.

## The message type

The wake-up is a custom message with `customType: "piclaw-ping"`, which is
persisted in pi's session files. It becomes `"picadence-ping"`. Old entries keep
the old type and render exactly as they did, because no renderer is registered
for either: both fall back to the plain custom-message card.

## The tag prefix

`AGENTS.md` says a release is tagged `<product>-v<semver>`. That becomes
`picadence-v<semver>`. No tag exists yet, so nothing has to be retagged.

## What is not renamed

Accepted ADRs keep their text, including `%LOCALAPPDATA%\trayclaw` in ADR 0001 and
the product name in ADR 0002. An accepted decision is the record of what was
decided then; the repository explains the name it carries today in
`README.md`, and the changelog carries the rename.

## Order

Mechanical, in one commit: solution, directories, project files, namespaces,
`InternalsVisibleTo`, window class names, the extension file and its tests, the
path constants, the message text and the icon's tooltip-free title. Then
`verify.sh`, which builds from the renamed solution and runs both suites.
