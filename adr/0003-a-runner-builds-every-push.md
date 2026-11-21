# 0003. A GitHub runner builds and checks every push

- Status: Accepted
- Date: 2026-11-21

## Context

Until today the repository was local. `scripts/verify.sh` ran on this one machine,
started by the same agent that wrote the code it was checking, and every merge
went through it. The repository is now public, and the loop in `AGENTS.md` already
asks for something the project could not do: push the branch, let the CI block
run, and merge only on a green status for the commit being merged. With no
workflow that step was skipped, and the release artifact was built on the author's
laptop, so nothing independent says the tagged commit builds.

Two facts make a hosted runner cheap here. The `windows-2025` image carries Visual
Studio 2022 Enterprise with `VC.Tools.x86.x64`, which is the MSVC linker NativeAOT
calls, and .NET SDK 10.0.404, which is the version this machine builds with. A
public repository uses standard runners without charge, and the job is a build and
a test suite, not a service.

The one thing the image does not carry is the external tool the project depends
on: the OpenSpec CLI. `scripts/check-change.sh` skips its OpenSpec sections when
the command is missing, so a runner without it would quietly check less than this
machine does.

## Decision

We will run one workflow on GitHub, on every push to the trunk branch, on every
pull request, and by hand. It runs on `windows-latest`, configures the repository's
hook path, installs the OpenSpec CLI at the version `AGENTS.md` names, runs
`bash scripts/verify.sh`, publishes the AOT executable with a version derived from
the run, and uploads that executable as an artifact of the run.

It does not cut releases. No tag, no release page, no upload to one. Cutting a
release is a human act with a version decided after the fact, and automating it is
a separate decision.

The workflow calls the script the repository already owns rather than restating
its steps, so "how to check this repo" keeps one owner and a step added to
`verify.sh` reaches the runner without a second edit.

## Consequences

A merge now waits for a run, and a red run stops it. The check is no longer run by
the author of the change, which is the point.

The runner is a new external system in the delivery path: a hosted service, an
image that changes under us, and a workflow file that can rot. When it is red or
unavailable, the change does not merge; a red run becomes a change of its own
rather than something to work around.

The OpenSpec version is read from `AGENTS.md` at run time, so a version bump moves
one line and the runner follows. The .NET SDK comes from the image, so a new SDK
arrives when GitHub updates the image, not when this repository asks.

The workflow also becomes the first thing a contributor sees fail, so its output
has to stay readable, and its runtime has to stay inside a couple of minutes.

## Alternatives

- Stay local: no independent check, and the release exe is only as trustworthy as
  the machine that built it.
- A self-hosted runner: one more machine to patch and secure, with credentials on
  it, for a build that takes two minutes.
- Azure Pipelines: the same shape with another account to own, and no benefit over
  the remote this project already has.
- Another operating system: the product is Win32 and GDI, and NativeAOT links
  against MSVC, so the build belongs on Windows.
- A container with the toolchain: the same toolchain the image already ships, plus
  a Dockerfile to keep, and `CONVENTIONS.md` says no Docker.
