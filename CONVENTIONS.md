# Conventions

How the code is written here. One rule per heading, two lines each, and a rule
with no trigger is ignored. The ownership rule and how to add one are in
`AGENTS.md` under **Files**.

## Formatting and naming

`dotnet format picadence.sln --verify-no-changes` decides formatting, and
`.editorconfig` is the only place its settings change. Trigger: a build that
rewrites a file nobody edited.

## Error handling and logging

The tray writes no log: a failure returns a non-zero exit code from `Main` and
the icon never appears. Trigger: a failure a user cannot explain by a missing
icon.

## Testing

A test names the behaviour it proves, sits beside the code it covers, and the
suite runs with `dotnet test`. Trigger: a fix whose bug no test reproduces.

## Sizes

A size in the panel is written in device-independent pixels and scaled once, at
paint time, by the scale the window actually got. The values themselves, and what
each one is for, are in `DESIGN.md`. Trigger: a magic pixel number in a paint
call.

## The registry

The app writes one value, under `HKEY_CURRENT_USER`, and never asks for
elevation. Anything that would need `HKEY_LOCAL_MACHINE` is a different decision
and a different change. Trigger: a call that opens a key outside the user's hive.

## The pi side

`extensions/picadence.js` is plain ESM with no build step, because pi loads it
directly and `node --test` has to import the same file. Trigger: an import that
needs compiling.

A handler in that file catches its own errors and reports nothing rather than
breaking a session. Trigger: an exception that reaches pi's event loop.

## Dependencies

The tray has no package reference at all; a package is added only when the
platform cannot do the job, and every package is pinned in a committed
`packages.lock.json`. Trigger: a change that edits a lock file by hand.

## Repository

Ignore build output by a path anchored at the repository root: `/dist/`, never
`dist/`. An unanchored pattern also matches a directory of that name deeper in
the tree, so a build can drop files a page loads and still succeed on the machine
that already has them. The one exception is `**/bin/` and `**/obj/`, because
every project here writes its own.

Commit the lock files and never the build output. Trigger: `git status` shows a
`bin/` or an `obj/` path.
