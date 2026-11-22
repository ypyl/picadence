# Ignore the editor folder (L0)

## Why

`.vscode/` sits in the working tree and shows up as untracked noise in every
`git status`. Editor state belongs to whoever opens the folder, not to the
project, and `settings.json` is already ignored for the same reason.

## What Changes

One line in `.gitignore`. Nothing moves, nothing is deleted; the folder stays on
disk and stops appearing in `git status`.

## Capabilities

None. Ignore rules are not behavior, so `skip_specs: true`.

## Impact

- `.gitignore`.

## Routing

0. **Does observable behavior change?** No.
1. **Can it be undone easily?** Yes, one line.
2. **Can a machine prove it correct?** Yes: `git status` stops listing it.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
