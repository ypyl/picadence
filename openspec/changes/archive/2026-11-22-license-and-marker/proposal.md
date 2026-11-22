# The repository has a license and a greppable debt marker (L0)

## Why

The repository is public and carries no license, which means the default applies:
all rights reserved. Anyone who found the release page may look and may not legally
use, copy or change it. That is the opposite of what a published tool is for.

And one `shortcut:` marker, the one recording that CI builds with a different .NET
SDK from this machine, sits in `REPO-FACTS.md`, where the debt query in `AGENTS.md`
does not look. A deferred limit that cannot be found is not recorded.

## What Changes

- `LICENSE`: MIT, the usual choice for a single-file tool that people are meant to
  take and use.
- `README.md`: one line at the end saying which license, and pointing at the file.
- The `shortcut:` marker moves from `REPO-FACTS.md` into the workflow, above the
  step it is about. The fact itself stays in `REPO-FACTS.md`, without the marker
  prefix, because the marker belongs in code where the debt query finds it.

No behavior changes and no requirement moves.

## Capabilities

None. Documentation, so `skip_specs: true`.

## Impact

- `LICENSE` (new), `README.md`, `.github/workflows/ci.yml`, `REPO-FACTS.md`.

## Routing

0. **Does observable behavior change?** No. The terms only widen: nothing that was
   permitted stops being permitted.
1. **Can it be undone easily?** Mostly. Removing the file later stops new grants;
   rights already granted to anyone who took a copy cannot be recalled, which is
   why the choice of MIT is recorded here and in the commit rather than made
   quietly. Nothing technical depends on it.
2. **Can a machine prove it correct?** Yes for the marker: `scripts/check-change.sh`
   reads it and fails a marker with no upgrade trigger. For the license, the check
   is the file and GitHub's own license detection on the repository page.
3. **Does anything outside the change depend on it?** Only in the sense that a
   license is a promise to people outside, and that promise is the point.

Level: **L0**. It changes no behavior, and the decision itself is the human's,
taken in this session, and named in the commit that carries it.
