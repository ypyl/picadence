# Design

The decision is [ADR 0003](../../../adr/0003-a-runner-builds-every-push.md). This
file is the workflow itself: what runs, in what order, and which of the traps this
repository already knows about it has to step over.

## Shape

One job, one file, three triggers. A second job would only buy parallelism for a
run that takes a couple of minutes.

```yaml
on:
  push: { branches: [main] }
  pull_request:
  workflow_dispatch:
```

`workflow_dispatch` is there because the block in `AGENTS.md` may name a manual
trigger, and because a run you can start by hand is worth having when a branch
needs a second opinion.

## Steps

```text
checkout                 actions/checkout
hook path                git config core.hooksPath .githooks
openspec                 npm install -g @fission-ai/openspec@<version from AGENTS.md>
verify                   bash scripts/verify.sh
publish                  dotnet publish -c Release -p:Version=0.0.0-ci.<run_number>
artifact                 actions/upload-artifact, the published exe
```

**The hook path is not optional.** `scripts/check-change.sh` fails its Local gate
section when `core.hooksPath` is not `.githooks`, which a fresh clone never has.
Without this line the first run is red before anything compiles, and it looks like
the workflow is broken.

**The OpenSpec version comes from `AGENTS.md`,** not from a number copied into the
workflow:

```bash
version=$(sed -n 's/^- OpenSpec CLI: \([0-9][0-9.]*\).*/\1/p' AGENTS.md | head -1)
npm install -g "@fission-ai/openspec@$version"
```

A version with two owners drifts. Project facts owns it; the workflow reads it. When
the CLI is missing, `check-change.sh` skips its OpenSpec sections rather than
failing, so a silent skip is the failure mode this line prevents.

**The workflow calls `verify.sh` and does not restate it.** The script owns "how to
check this repo": the invariant script, then the lint, build and test commands
declared in Project facts. A step added there reaches the runner with no second
edit, and the two can never disagree about what green means.

**The version of a CI artifact is a placeholder.** `0.0.0-ci.<run_number>` says
"this is not a release" in the place where Windows shows a version. The real
version is a tag, cut from a trunk tip, and this workflow does not cut one.

## Traps this repository already paid for

| Trap | Where it is recorded | What the workflow does |
|---|---|---|
| A fresh clone has no hook path | `bootstrap` in `AGENTS.md`, and the Local gate check | sets it before verify |
| The OpenSpec CLI is not on the image | Project facts, one external tool | installs the pinned version |
| A test that needs an interactive desktop | nothing yet | if one fails, the test gets a guard, not the workflow a workaround |
| `bash` is not the default shell on a Windows runner | known: the default is pwsh | every script step names `bash` |
| The image's .NET SDK moves under us | Project facts pins 10.0.404 | the workflow takes whatever the image ships, and the first run records which |

## What it deliberately does not do

- No release, no tag, no upload to a release.
- No caching of NuGet or npm: the job is short, and a cache is another thing that
  can go stale and fail in a way that looks like a code problem.
- No matrix: one OS is the product.
- No branch protection is configured here. Making the run required for a merge is a
  repository setting, and it is the human's to set.

## Failure

A red run is information, not an obstacle. The rule in `AGENTS.md` is to fix it
before new work starts, with the run URL in the change that fixes it. If the runner
itself is unavailable, the merge waits rather than falling back to the local gate.

## Sequence for this change

1. Commit the plan, which holds the ADR and this file.
2. Approve the ADR gate.
3. Write the workflow, push the branch, and let it run. This is the first run and
   the record of what the image actually does.
4. Fix what it finds, then fill the `CI` block with the commands that were run.
5. Archive, commit, push, and merge on a green run of the branch head.
