# A GitHub runner checks every push (L2)

## Why

The repository is public, and the loop in `AGENTS.md` says a merge waits for a
green run of the pushed branch. With no workflow, that step has never happened:
every merge so far was gated by `scripts/verify.sh` on this machine, run by the
same agent that wrote the code, and the v0.1.0 executable was built on this
laptop. Nothing independent says the tagged commit builds.

The runner can do it. The Windows image carries the MSVC linker that NativeAOT
needs and .NET SDK 10.0.404, the same version this machine uses, and a public
repository runs standard runners for free.

The decision is [ADR 0003](../../../adr/0003-a-runner-builds-every-push.md).

## What Changes

- One workflow, `.github/workflows/ci.yml`, on push to trunk, on pull request and
  by hand. It configures the hook path, installs the OpenSpec CLI at the version
  Project facts names, runs `bash scripts/verify.sh`, publishes the AOT
  executable and uploads it as an artifact of the run.
- The `CI` block in `AGENTS.md` is filled with the four commands that were
  actually run against this workflow, not commands that look right.

No requirement moves. The workflow runs the checks the durable specs already
describe; it adds a second place they run, not a new promise. `skip_specs: true`.

## Capabilities

None.

## Impact

- `.github/workflows/ci.yml`, new.
- `AGENTS.md`: the CI block, and Project facts if a command changes.
- `REPO-FACTS.md`: what the first run did, including anything the runner does
  differently from this machine.
- No change to the product, its specs or its dependencies.

## Routing

0. **Does observable behavior change?** No. The tray behaves as it did; what
   changes is who checks it, and a merge now waits for a machine that is not this
   laptop.
1. **Can it be undone easily?** Yes: delete the workflow and empty the block. The
   tag and the release that already exist are untouched.
2. **Can a machine prove it correct?** Yes, and the proof is the workflow run
   itself: it fails when the code does not build or a test fails, and its
   conclusion is the gate.
3. **Does anything outside the change depend on it?** Yes, and that is the point:
   every later merge depends on a hosted runner, its image and its workflow file.
   That is why this is L2 and not tooling.

## Contract

The `CI` block in `AGENTS.md`, which `scripts/check-change.sh` reads and which
every later merge stands on. A change that edits the workflow without editing that
block leaves the record claiming something that is no longer true.

## Approval

- Gate: L2 ADR
- Approved by: Yauhen Pyl
- Approved at: 2026-11-21
- Artifact: adr/0003-a-runner-builds-every-push.md sha256:4cac33ea4b1162e4490eb0e6024bba90c321c2404eaa28b0501a6b205f61659b
- Artifact: openspec/changes/ci-on-a-runner/proposal.md sha256:c6de7c37fa605520e7735e89e7c82c229ff1a893bfb2ee4fe78cebb4830227f1
- Words: "(none)"
