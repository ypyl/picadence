# A branch push starts its run (L0)

## Why

The loop merges on a green run for the branch being merged, and three places say a
branch push produces one: `AGENTS.md` under **CI** ("Trigger: git push", and "the
workflow is on every branch, not only trunk"), the comment at the top of `ci.yml`
("A branch push gets a run of its own"), and the loop's step 4.

It does not. `ci.yml` filters `push` to `branches: [main]`, and no pull request is
opened, so the only way a change branch ever runs is `gh workflow run ci.yml` by hand.
That was done six times today, and GitHub answered two of those with 500 and 503, which
is a manual step that can fail at the one moment the loop waits on it.

## What Changes

- `ci.yml` drops the branch filter, so a push starts a run on the branch that was
  pushed, which is what every document already describes. `workflow_dispatch` stays,
  because a re-run without a new commit is still occasionally needed.
- The comment above the trigger says which runs are wanted and which are not.

## Capabilities

None. No requirement moves; this is the pipeline, not the product, so
`skip_specs: true`.

## Impact

- `.github/workflows/ci.yml` only.
- One run per pushed branch instead of none, and no run for a tag.

## Routing

0. **Does observable behavior change?** Not in the product. A pushed branch now
   produces a run; that is what the documents promise.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** Yes: push the branch that carries this change and
   read `gh run list --branch <name>`. The run this change was merged on is the proof.
3. **Does anything outside the change depend on it?** The CI block in `AGENTS.md` names
   the trigger; it already says `git push`, so no document needs to move.

Level: **L0**.
