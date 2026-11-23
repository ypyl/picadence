# The winget submission carries 0.3.0 (L0)

## Why

The pull request was opened for 0.2.0 and had not merged when 0.3.0 shipped.
Submitting the older version and following it with a second pull request would put
two versions in the queue for a package nobody has yet; the community repository
wants one, for the newest release.

## What Changes

- The three manifests move to `0.3.0`, with the new asset URL, hash, size and date,
  and the 0.2.0 folder is gone from both this repository and the pull request.
- `packaging/winget/README.md` and `REPO-FACTS.md` say which version the pull
  request carries and why it replaced the first.

## Capabilities

None. Packaging, so `skip_specs: true`.

## Impact

- `packaging/winget/`, `REPO-FACTS.md`.

## Routing

0. **Does observable behavior change?** The program does not; which version winget
   will offer does.
1. **Can it be undone easily?** The pull request can be edited again until it
   merges, and the repository copies are one commit.
2. **Can a machine prove it correct?** Yes, and it was: `winget validate`, then an
   install and an uninstall of the 0.3.0 manifests on this machine.
3. **Does anything outside the change depend on it?** The pull request, which is a
   moderator's to merge.

Level: **L0**.
