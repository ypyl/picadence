# Say that it is unsigned, and how to check it (L0)

## Why

The winget pull request tells a reviewer that the executable is not Authenticode
signed. The README, which is what a person reads, does not: they download, Windows
warns, and nothing in the document tells them whether the warning means anything.

A certificate is a yearly cost and this project is not buying one, so the honest
thing is to say so and give the reader a way to check the file themselves. The
release notes already carry the SHA256; the README never mentioned it.

## What Changes

- An **If Windows warns you** section under the install instructions: why the
  prompt appears, that winget does not take the browser route, the one command that
  prints the hash of the file in hand, and what "More info → Run anyway" actually
  means.
- The asset name in the README moves to 0.3.0, which the previous release missed.
- `REPO-FACTS.md` records the decision and what would reverse it, so the next
  session does not rediscover the price of an EV certificate.

## Capabilities

None. Prose, so `skip_specs: true`.

## Impact

- `README.md`, `REPO-FACTS.md`.

## Routing

0. **Does observable behavior change?** No.
1. **Can it be undone easily?** Yes, one commit.
2. **Can a machine prove it correct?** The hash in the README is the hash of the
   release asset, which `gh release view` reports.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
