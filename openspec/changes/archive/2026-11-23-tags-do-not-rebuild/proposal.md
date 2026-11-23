# A tag does not rebuild what already ran (L0)

## Why

`on: push:` with no filter runs the workflow for a branch push and for a tag push.
Cutting `picadence-v0.2.0` therefore built the same commit twice: once for the
merge that carried it, once because a tag landed on it. The second run costs a
runner and answers nothing; the release takes the first run's artifact.

## What Changes

- The push trigger names the trunk branch. Pull requests and `workflow_dispatch`
  keep their runs, so a branch push, a review, and a hand start all still work.
- The release keeps using the run for the commit being released, which is green
  before the tag is cut.

## Capabilities

None. CI wiring, so `skip_specs: true`.

## Impact

- `.github/workflows/ci.yml`, and one line in `REPO-FACTS.md` recording the related
  fact that cost the most time today: a running tray locks the executable the build
  writes, so `scripts/verify.sh` fails with `MSB3027` until the process stops.

## Routing

0. **Does observable behavior change?** The product does not. A tag stops producing
   a second run.
1. **Can it be undone easily?** Yes, one line.
2. **Can a machine prove it correct?** Yes: push a tag and watch that no run
   appears, and push a branch and watch one do.
3. **Does anything outside the change depend on it?** The release process reads a
   run for the commit it tags, and that run is the trunk push, which still exists.

Level: **L0**.
